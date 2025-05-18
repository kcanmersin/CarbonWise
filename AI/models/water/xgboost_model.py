import pandas as pd
import numpy as np
import xgboost as xgb
from sklearn.preprocessing import StandardScaler

from models.base_model import BasePredictionModel
from utils.data_utils import load_water_data, split_train_test, calculate_metrics
from utils.preprocessing import preprocess_water_data

class WaterXGBoostModel(BasePredictionModel):
    def __init__(self):
        self.model_name = "XGBoost-Su"
    
    def load_and_prepare_data(self, file_path):
        return load_water_data(file_path)
    
    def preprocess_data(self, df):
        return preprocess_water_data(df)
    
    def split_train_test(self, df, test_size=12):
        return split_train_test(df, test_size)
    
    def train_model(self, train_data, test_data):
        """XGBoost modeli eğitir ve değerlendirir"""
        # Özellikler ve hedef değişken
        non_feature_cols = ['AY', 'TARIH', 'TÜKETİM']
        features = [col for col in train_data.columns if col not in non_feature_cols]

        X_train = train_data[features]
        y_train = train_data['TÜKETİM']

        X_test = test_data[features]
        y_test = test_data['TÜKETİM']

        # XGBoost modelini oluştur ve eğit
        model = xgb.XGBRegressor(
            n_estimators=200,
            learning_rate=0.1,
            max_depth=5,
            subsample=0.8,
            colsample_bytree=0.8,
            objective='reg:squarederror',
            tree_method='hist',
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
        """Sonraki aylar için XGBoost modeli ile tüketim tahmini yapar"""
        # Son tarih bilgisi
        last_row = df.iloc[-1]
        last_date = last_row['TARIH']
        last_people = last_row['KİŞİ SAYISI']

        # Model ve özellikler
        model = model_results['model']
        feature_names = model_results['feature_names']

        # Ay isimlerini tanımla
        month_names = {
            1: 'OCAK', 2: 'ŞUBAT', 3: 'MART', 4: 'NİSAN',
            5: 'MAYIS', 6: 'HAZİRAN', 7: 'TEMMUZ', 8: 'AĞUSTOS',
            9: 'EYLÜL', 10: 'EKİM', 11: 'KASIM', 12: 'ARALIK'
        }

        # Son 24 ay verilerini al (lag feature'lar için)
        historical_data = df.iloc[-24:].copy()
        
        # Min yıl hesapla (YIL_TREND için)
        min_year = df['YIL'].min() if 'YIL_TREND' in feature_names else 0

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

            # Mevsimsellik ve tatil bilgileri
            new_row['TATIL_AYI'] = 1 if future_month in [7, 8] else 0
            new_row['MEVSIM'] = 0 if future_month in [12, 1, 2] else \
                              1 if future_month in [3, 4, 5] else \
                              2 if future_month in [6, 7, 8] else 3

            # Fourier özellikleri
            new_row['SIN_MONTH'] = np.sin(2 * np.pi * future_month/12)
            new_row['COS_MONTH'] = np.cos(2 * np.pi * future_month/12)

            # Yıl trend - eğer özellik listesinde varsa
            if 'YIL_TREND' in feature_names:
                new_row['YIL_TREND'] = future_year - min_year
            
            # Geçmiş tüketim değerleri ve hareketli ortalamalar için
            # Gerekli verileri hazırla
            if i == 0:
                # İlk ay için son gerçek değerleri kullan
                recent_values = historical_data['TÜKETİM'].tolist()
                
                for lag in [1, 3, 6, 12]:
                    if f'TÜKETİM_{lag}AY_ÖNCE' in feature_names:
                        lag_idx = min(lag, len(recent_values))
                        new_row[f'TÜKETİM_{lag}AY_ÖNCE'] = recent_values[-lag_idx]
                
                # Hareketli ortalamalar için özellikleri ekle
                if 'ROLLING_MEAN_3' in feature_names:
                    new_row['ROLLING_MEAN_3'] = np.mean(recent_values[-3:])
                if 'ROLLING_MEAN_6' in feature_names:
                    new_row['ROLLING_MEAN_6'] = np.mean(recent_values[-6:])
                if 'ROLLING_MEAN_12' in feature_names:
                    new_row['ROLLING_MEAN_12'] = np.mean(recent_values[-12:])
                
                # Değişim oranları
                if 'AYLARA_GÖRE_DEĞİŞİM' in feature_names:
                    new_row['AYLARA_GÖRE_DEĞİŞİM'] = (recent_values[-1] - recent_values[-2]) / recent_values[-2] if len(recent_values) >= 2 and recent_values[-2] != 0 else 0
                if 'YILLIK_DEĞİŞİM' in feature_names:
                    new_row['YILLIK_DEĞİŞİM'] = (recent_values[-1] - recent_values[-12]) / recent_values[-12] if len(recent_values) >= 12 and recent_values[-12] != 0 else 0
            else:
                # Önceki tahminleri kullan
                prev_predictions = [row['TAHMİN_TÜKETİM'] for row in future_data]
                recent_values = historical_data['TÜKETİM'].tolist() + prev_predictions
                
                # Son ayların tüketim değerleri
                for lag in [1, 3, 6, 12]:
                    if f'TÜKETİM_{lag}AY_ÖNCE' in feature_names:
                        lag_idx = min(lag, len(recent_values))
                        new_row[f'TÜKETİM_{lag}AY_ÖNCE'] = recent_values[-lag_idx]
                
                # Hareketli ortalamalar
                if 'ROLLING_MEAN_3' in feature_names:
                    new_row['ROLLING_MEAN_3'] = np.mean(recent_values[-3:])
                if 'ROLLING_MEAN_6' in feature_names:
                    new_row['ROLLING_MEAN_6'] = np.mean(recent_values[-6:])
                if 'ROLLING_MEAN_12' in feature_names:
                    new_row['ROLLING_MEAN_12'] = np.mean(recent_values[-12:])
                
                # Değişim oranları
                if 'AYLARA_GÖRE_DEĞİŞİM' in feature_names:
                    new_row['AYLARA_GÖRE_DEĞİŞİM'] = (recent_values[-1] - recent_values[-2]) / recent_values[-2] if recent_values[-2] != 0 else 0
                if 'YILLIK_DEĞİŞİM' in feature_names:
                    new_row['YILLIK_DEĞİŞİM'] = (recent_values[-1] - recent_values[-12]) / recent_values[-12] if len(recent_values) >= 12 and recent_values[-12] != 0 else 0

            # Kişi başına tüketim
            if 'KİŞİ_BAŞINA_TÜKETİM' in feature_names:
                new_row['KİŞİ_BAŞINA_TÜKETİM'] = recent_values[-1] / last_people

            # Tahmin için gerekli özellikleri eksiksiz hazırla
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
            new_row['TAHMİN_TÜKETİM'] = int(prediction)

            # Sonuçları ekle
            future_data.append(new_row)

        # Sonuçları DataFrame'e dönüştür
        future_df = pd.DataFrame(future_data)
        future_df['TARİH'] = future_df.apply(lambda row: f"{row['YIL']}-{month_names[row['AY_NO']]}", axis=1)

        return future_df[['TARİH', 'TAHMİN_TÜKETİM']]