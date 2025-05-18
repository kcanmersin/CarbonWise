import pandas as pd
import numpy as np
from sklearn.ensemble import RandomForestRegressor
from sklearn.preprocessing import StandardScaler

from models.base_model import BasePredictionModel
from utils.data_utils import load_electric_data, split_train_test, calculate_metrics
from utils.preprocessing import preprocess_electric_data

class ElectricRandomForestModel(BasePredictionModel):
    def __init__(self):
        self.model_name = "RandomForest-Elektrik"
    
    def load_and_prepare_data(self, file_path):
        return load_electric_data(file_path)
    
    def preprocess_data(self, df):
        return preprocess_electric_data(df)
    
    def split_train_test(self, df, test_size=12):
        return split_train_test(df, test_size)
    
    def train_model(self, train_data, test_data):
        """RandomForest modeli eğitir ve değerlendirir"""
        # Özellikler ve hedef değişken
        non_feature_cols = ['AY', 'TARIH', 'TÜKETİM', 'BIRIM_FIYAT']
        features = [col for col in train_data.columns if col not in non_feature_cols]

        X_train = train_data[features]
        y_train = train_data['TÜKETİM']

        X_test = test_data[features]
        y_test = test_data['TÜKETİM']

        # RandomForest modelini oluştur ve eğit
        model = RandomForestRegressor(
            n_estimators=200,
            max_depth=10,
            min_samples_split=2,
            min_samples_leaf=1,
            random_state=42
        )

        model.fit(X_train, y_train)

        # Tahminler
        pred = model.predict(X_test)

        # Performans metrikleri
        metrics = calculate_metrics(y_test, pred)

        # Feature importance
        feature_importances = model.feature_importances_

        return {
            'model': model,
            'metrics': metrics,
            'importances': feature_importances,
            'feature_names': features,
            'test_dates': test_data['AY'],
            'y_test': y_test,
            'pred': pred
        }
    
    def predict_future(self, df, model_results, months=12):
        """Sonraki aylar için RandomForest modeli ile elektrik tüketim tahmini yapar"""
        # Son tarih bilgisi
        last_row = df.iloc[-1]
        last_date = last_row['TARIH']
        last_people = last_row['KİŞİ SAYISI']

        # Son birim fiyat bilgisi
        try:
            last_price_trend = last_row['BIRIM_FIYAT_TREND']
        except:
            last_price_trend = 0.05  # Varsayılan yıllık %5 artış

        # Model ve özellikler
        model = model_results['model']
        feature_names = model_results['feature_names']

        # Ay isimlerini tanımla
        month_names = {
            1: 'OCAK', 2: 'ŞUBAT', 3: 'MART', 4: 'NİSAN',
            5: 'MAYIS', 6: 'HAZİRAN', 7: 'TEMMUZ', 8: 'AĞUSTOS',
            9: 'EYLÜL', 10: 'EKİM', 11: 'KASIM', 12: 'ARALIK'
        }

        # Son 24 ay verilerini al
        historical_data = df.iloc[-24:].copy()

        # Ay bazlı mevsimsel endeks
        monthly_avg = df.groupby('AY_NO')['TÜKETİM'].mean()
        overall_avg = df['TÜKETİM'].mean()
        month_indexes = monthly_avg / overall_avg

        # Min yıl (YIL_TREND için)
        min_year = df['YIL'].min()

        # Sonraki aylar için tahmin yap
        future_data = []
        future_dates = pd.date_range(start=last_date + pd.DateOffset(months=1), periods=months, freq='MS')

        # Her ay için ayrı ayrı tahmin yap
        for i, date in enumerate(future_dates):
            future_year = date.year
            future_month = date.month

            # Temel bilgiler
            new_row = {
                'TARIH': date,
                'YIL': future_year,
                'AY_NO': future_month,
                'AY': f"{future_year}-{month_names[future_month]}",
                'KİŞİ SAYISI': last_people
            }

            # Yıl trendi
            new_row['YIL_TREND'] = future_year - min_year
            new_row['YIL_TREND_SQ'] = new_row['YIL_TREND'] ** 2

            # Mevsimsellik ve tatil bilgileri
            new_row['TATIL_AYI'] = 1 if future_month in [7, 8] else 0
            new_row['MEVSIM'] = 0 if future_month in [12, 1, 2] else \
                            1 if future_month in [3, 4, 5] else \
                            2 if future_month in [6, 7, 8] else 3

            # Elektrik kullanımına özel etkenler
            new_row['ISITMA_AYLARI'] = 1 if future_month in [1, 2, 3, 11, 12] else 0
            new_row['SOGUTMA_AYLARI'] = 1 if future_month in [6, 7, 8, 9] else 0

            # Fourier özellikleri
            new_row['SIN_MONTH'] = np.sin(2 * np.pi * future_month/12)
            new_row['COS_MONTH'] = np.cos(2 * np.pi * future_month/12)
            if 'SIN_QUARTER' in feature_names:
                new_row['SIN_QUARTER'] = np.sin(2 * np.pi * future_month/3)
            if 'COS_QUARTER' in feature_names:
                new_row['COS_QUARTER'] = np.cos(2 * np.pi * future_month/3)

            # Mevsim endeksi
            if 'MEVSIM_ENDEKS' in feature_names:
                new_row['MEVSIM_ENDEKS'] = month_indexes.get(future_month, 1.0)

            # Fiyat trendi
            if 'BIRIM_FIYAT_TREND' in feature_names:
                new_row['BIRIM_FIYAT_TREND'] = last_price_trend

            # Geçmiş değerler 
            if i == 0:
                # İlk ay için gerçek değerleri kullan
                prev_values = historical_data['TÜKETİM'].tolist()
                
                # Geçmiş değerleri ekle (eğer özelliklerde varsa)
                for lag in [1, 2, 3, 6, 12]:
                    if f'TÜKETİM_{lag}AY_ÖNCE' in feature_names:
                        lag_idx = min(lag, len(prev_values))
                        new_row[f'TÜKETİM_{lag}AY_ÖNCE'] = prev_values[-lag_idx]
                
                # Hareketli ortalamalar
                for window in [3, 6, 12]:
                    if f'ROLLING_MEAN_{window}' in feature_names:
                        new_row[f'ROLLING_MEAN_{window}'] = np.mean(prev_values[-window:])
                
                # Değişim oranları
                if 'AYLARA_GÖRE_DEĞİŞİM' in feature_names:
                    new_row['AYLARA_GÖRE_DEĞİŞİM'] = (prev_values[-1] - prev_values[-2]) / prev_values[-2] if len(prev_values) >= 2 and prev_values[-2] != 0 else 0
                if 'YILLIK_DEĞİŞİM' in feature_names:
                    new_row['YILLIK_DEĞİŞİM'] = (prev_values[-1] - prev_values[-12]) / prev_values[-12] if len(prev_values) >= 12 and prev_values[-12] != 0 else 0
                
                # Kişi değişim ve kişi başı değerler
                if 'KİŞİ_DEĞİŞİM' in feature_names:
                    new_row['KİŞİ_DEĞİŞİM'] = 0  # Değişim yok varsayıyoruz
                if 'KİŞİ_BAŞINA_TÜKETİM' in feature_names:
                    new_row['KİŞİ_BAŞINA_TÜKETİM'] = prev_values[-1] / last_people
                if 'KİŞİ_BAŞINA_TÜKETİM_DEĞİŞİM' in feature_names:
                    new_row['KİŞİ_BAŞINA_TÜKETİM_DEĞİŞİM'] = 0
            else:
                # Önceki tahminleri kullan
                prev_predictions = [row['TAHMİN_TÜKETİM'] for row in future_data]
                all_values = historical_data['TÜKETİM'].tolist() + prev_predictions
                
                # Geçmiş değerleri ekle
                for lag in [1, 2, 3, 6, 12]:
                    if f'TÜKETİM_{lag}AY_ÖNCE' in feature_names:
                        new_row[f'TÜKETİM_{lag}AY_ÖNCE'] = all_values[-lag]
                
                # Hareketli ortalamalar
                for window in [3, 6, 12]:
                    if f'ROLLING_MEAN_{window}' in feature_names:
                        new_row[f'ROLLING_MEAN_{window}'] = np.mean(all_values[-window:])
                
                # Değişim oranları
                if 'AYLARA_GÖRE_DEĞİŞİM' in feature_names:
                    new_row['AYLARA_GÖRE_DEĞİŞİM'] = (all_values[-1] - all_values[-2]) / all_values[-2] if all_values[-2] != 0 else 0
                if 'YILLIK_DEĞİŞİM' in feature_names:
                    new_row['YILLIK_DEĞİŞİM'] = (all_values[-1] - all_values[-12]) / all_values[-12] if len(all_values) >= 12 and all_values[-12] != 0 else 0
                
                # Kişi değişim ve kişi başı değerler
                if 'KİŞİ_DEĞİŞİM' in feature_names:
                    new_row['KİŞİ_DEĞİŞİM'] = 0  # Değişim yok varsayıyoruz
                if 'KİŞİ_BAŞINA_TÜKETİM' in feature_names:
                    new_row['KİŞİ_BAŞINA_TÜKETİM'] = all_values[-1] / last_people
                if 'KİŞİ_BAŞINA_TÜKETİM_DEĞİŞİM' in feature_names:
                    prev_per_capita = future_data[-1]['KİŞİ_BAŞINA_TÜKETİM'] if 'KİŞİ_BAŞINA_TÜKETİM' in future_data[-1] else all_values[-2] / last_people
                    curr_per_capita = new_row['KİŞİ_BAŞINA_TÜKETİM']
                    new_row['KİŞİ_BAŞINA_TÜKETİM_DEĞİŞİM'] = (curr_per_capita - prev_per_capita) / prev_per_capita if prev_per_capita != 0 else 0

            # Tahmin için gerekli özellikleri al
            X_pred = {}
            for feature in feature_names:
                if feature in new_row:
                    X_pred[feature] = new_row[feature]
                else:
                    X_pred[feature] = 0

            # DataFrame'e çevir
            X_pred_df = pd.DataFrame([X_pred], columns=feature_names)

            # Tahmin yap
            prediction = model.predict(X_pred_df)[0]

            # Sonuçlar
            new_row['TAHMİN_TÜKETİM'] = int(prediction)
            future_data.append(new_row)

        # Sonuçları DataFrame'e dönüştür
        future_df = pd.DataFrame(future_data)
        future_df['TARİH'] = future_df.apply(lambda row: f"{row['YIL']}-{month_names[row['AY_NO']]}", axis=1)

        return future_df[['TARİH', 'TAHMİN_TÜKETİM']]