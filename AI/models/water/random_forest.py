import pandas as pd
import numpy as np
from sklearn.ensemble import RandomForestRegressor
from sklearn.preprocessing import StandardScaler
from sklearn.metrics import mean_squared_error, mean_absolute_error

from models.base_model import BasePredictionModel
from utils.data_utils import load_water_data, split_train_test, calculate_metrics
from utils.preprocessing import preprocess_water_data

class WaterRandomForestModel(BasePredictionModel):
    def __init__(self):
        self.model_name = "RandomForest-Su"
    
    def load_and_prepare_data(self, file_path):
        return load_water_data(file_path)
    
    def preprocess_data(self, df):
        return preprocess_water_data(df)
    
    def split_train_test(self, df, test_size=12):
        return split_train_test(df, test_size)
    
    def train_model(self, train_data, test_data):
        """RandomForest modeli eğitir ve değerlendirir"""
        # Özellikler ve hedef değişken
        non_feature_cols = ['AY', 'TARIH', 'TÜKETİM']
        features = [col for col in train_data.columns if col not in non_feature_cols]
        
        X_train = train_data[features]
        y_train = train_data['TÜKETİM']
        
        X_test = test_data[features]
        y_test = test_data['TÜKETİM']
        
        # Özellikleri ölçeklendir
        scaler = StandardScaler()
        X_train_scaled = scaler.fit_transform(X_train)
        X_test_scaled = scaler.transform(X_test)
        
        # RandomForest modelini oluştur ve eğit
        model = RandomForestRegressor(
            n_estimators=200,
            max_depth=12,
            min_samples_split=2,
            min_samples_leaf=1,
            random_state=42
        )
        
        model.fit(X_train, y_train)  # Ölçeklendirme olmadan daha iyi sonuç verebilir
        
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
            'pred': pred,
            'scaler': scaler
        }
    
    def predict_future(self, df, model_results, months=12):
        """Sonraki 12 ay için RandomForest modeli ile tüketim tahmini yapar"""
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
        
        # Sonraki 12 ay için tahmin yap
        future_data = []
        future_dates = pd.date_range(start=last_date + pd.DateOffset(months=1), periods=months, freq='MS')
        
        # Her ay için ayrı ayrı tahmin yap
        last_predictions = historical_data['TÜKETİM'].tail(12).tolist()
        
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
            
            # Geçmiş tüketim değerleri
            if i == 0:
                # İlk ay için son gerçek değerleri kullan
                for lag in [1, 3, 6, 12]:
                    lag_idx = min(lag, len(last_predictions))
                    new_row[f'TÜKETİM_{lag}AY_ÖNCE'] = last_predictions[-lag_idx]
            else:
                # Sonraki aylar için önceki tahminleri kullan
                new_row['TÜKETİM_1AY_ÖNCE'] = future_data[-1]['TAHMİN_TÜKETİM']
                new_row['TÜKETİM_3AY_ÖNCE'] = last_predictions[-2] if i < 3 else future_data[i-3]['TAHMİN_TÜKETİM']
                new_row['TÜKETİM_6AY_ÖNCE'] = last_predictions[-5] if i < 6 else future_data[i-6]['TAHMİN_TÜKETİM']
                new_row['TÜKETİM_12AY_ÖNCE'] = last_predictions[-11] if i < 12 else future_data[i-12]['TAHMİN_TÜKETİM']
            
            # Son 3, 6, 12 ay ortalama değerleri 
            # (Bu kısım özgün koddan basitleştirilmiştir)
            if i == 0:
                new_row['ROLLING_MEAN_3'] = np.mean(last_predictions[-3:])
                new_row['ROLLING_MEAN_6'] = np.mean(last_predictions[-6:])
                new_row['ROLLING_MEAN_12'] = np.mean(last_predictions[-12:])
            else:
                recent_values = [row['TAHMİN_TÜKETİM'] for row in future_data]
                if i < 3:
                    new_row['ROLLING_MEAN_3'] = np.mean(last_predictions[-(3-i):] + recent_values)
                else:
                    new_row['ROLLING_MEAN_3'] = np.mean(recent_values[-3:])
                
                if i < 6:
                    new_row['ROLLING_MEAN_6'] = np.mean(last_predictions[-(6-i):] + recent_values)
                else:
                    new_row['ROLLING_MEAN_6'] = np.mean(recent_values[-6:])
                
                if i < 12:
                    new_row['ROLLING_MEAN_12'] = np.mean(last_predictions[-(12-i):] + recent_values)
                else:
                    new_row['ROLLING_MEAN_12'] = np.mean(recent_values[-12:])
            
            # Kişi başına tüketim trendi
            new_row['KİŞİ_BAŞINA_TÜKETİM'] = new_row['TÜKETİM_1AY_ÖNCE'] / last_people
            
            # Tahmin için gerekli özellikleri al
            X_pred = {}
            for feature in feature_names:
                if feature in new_row:
                    X_pred[feature] = new_row[feature]
                else:
                    X_pred[feature] = 0  # Eksik özellikler için varsayılan değer
            
            # DataFrame'e çevir
            X_pred_df = pd.DataFrame([X_pred])
            
            # Tahmin yap
            prediction = model.predict(X_pred_df)[0]
            new_row['TAHMİN_TÜKETİM'] = int(prediction)
            
            # Sonuçları ekle
            future_data.append(new_row)
        
        # Sonuçları DataFrame'e dönüştür ve gerekli sütunları seç
        future_df = pd.DataFrame(future_data)
        future_df['TARİH'] = future_df.apply(lambda row: f"{row['YIL']}-{month_names[row['AY_NO']]}", axis=1)
        
        return future_df[['TARİH', 'TAHMİN_TÜKETİM']]