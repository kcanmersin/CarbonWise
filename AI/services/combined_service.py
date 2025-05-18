import pandas as pd
import joblib
import os

from models.all_ensemble import AllEnsembleModel
from services.water_service import WaterPredictionService
from services.electric_service import ElectricPredictionService

class CombinedPredictionService:
    """Su ve elektrik tüketimlerini birlikte tahmin eden servis"""
    
    def __init__(self):
        """Su ve elektrik için topluluk modellerini kullanan birleşik servis oluşturur"""
        self.model = AllEnsembleModel()
        self.water_service = WaterPredictionService(model_type="ensemble")
        self.electric_service = ElectricPredictionService(model_type="ensemble")
        
        # Model dosya yolları
        self.models_dir = os.path.join("models", "saved")
        os.makedirs(self.models_dir, exist_ok=True)
        self.model_path = os.path.join(self.models_dir, "combined_model.joblib")
    
    def train(self, file_path):
        """Excel dosyasından birleşik modeli eğitir ve kaydeder"""
        # Su ve elektrik modellerini ayrı ayrı eğit
        water_results = self.water_service.train(file_path)
        electric_results = self.electric_service.train(file_path)
        
        # Birleşik sonuçları oluştur
        combined_results = {
            'water': water_results,
            'electric': electric_results,
            'combined_future_predictions': self._merge_predictions(
                water_results['future_predictions'],
                electric_results['future_predictions']
            )
        }
        
        # Birleşik modeli kaydet
        joblib.dump(combined_results, self.model_path)
        
        return combined_results
    
    def predict(self, months=12):
        """Gelecek aylar için hem su hem elektrik tüketim tahmini yapar"""
        if not os.path.exists(self.model_path):
            # Eğer birleşik model yoksa, ayrı ayrı servisleri kullanmayı dene
            try:
                water_predictions = self.water_service.predict(months)
                electric_predictions = self.electric_service.predict(months)
                
                # Tahminleri birleştir
                return self._merge_predictions(water_predictions, electric_predictions)
            except FileNotFoundError:
                raise FileNotFoundError("Hiçbir model eğitilmemiş. Önce train metodu ile modelleri eğitin.")
        
        # Kaydedilmiş birleşik modeli yükle
        combined_results = joblib.load(self.model_path)
        
        # En son birleşik tahminleri döndür
        return combined_results['combined_future_predictions']
    
    def evaluate(self):
        """Su ve elektrik modelleri için performans metriklerini döndürür"""
        if not os.path.exists(self.model_path):
            # Ayrı ayrı servisleri değerlendirmeyi dene
            try:
                water_eval = self.water_service.evaluate()
                electric_eval = self.electric_service.evaluate()
                
                return {
                    'water': water_eval,
                    'electric': electric_eval
                }
            except FileNotFoundError:
                raise FileNotFoundError("Hiçbir model eğitilmemiş. Önce train metodu ile modelleri eğitin.")
        
        # Kaydedilmiş birleşik modeli yükle
        combined_results = joblib.load(self.model_path)
        
        return {
            'water': {
                'metrics': combined_results['water']['metrics'],
                'comparison': combined_results['water']['comparison']
            },
            'electric': {
                'metrics': combined_results['electric']['metrics'],
                'comparison': combined_results['electric']['comparison']
            }
        }
    
    def _merge_predictions(self, water_predictions, electric_predictions):
        """Su ve elektrik tahminlerini tek bir yapıda birleştirir"""
        # Liste halindeki tahminlerden DataFramelere dönüştürelim
        if isinstance(water_predictions, list):
            water_df = pd.DataFrame(water_predictions)
        else:
            water_df = water_predictions
            
        if isinstance(electric_predictions, list):
            electric_df = pd.DataFrame(electric_predictions)
        else:
            electric_df = electric_predictions
        
        # Ortak tarih bilgisine göre birleştir
        date_col = 'TARİH' if 'TARİH' in water_df.columns else 'TARIH'
        
        # Eğer tarihler farklı formatta ise standardize et
        if date_col not in water_df.columns or date_col not in electric_df.columns:
            # Alternatif tarih sütunlarını ara
            if 'TARİH' in water_df.columns:
                water_df['TARIH'] = water_df['TARİH']
            if 'TARİH' in electric_df.columns:
                electric_df['TARIH'] = electric_df['TARİH']
            date_col = 'TARIH'
        
        # Birleştirme işlemi
        merged_df = pd.merge(
            water_df[[date_col, 'TAHMİN_TÜKETİM']], 
            electric_df[[date_col, 'TAHMİN_TÜKETİM']], 
            on=date_col,
            suffixes=('_SU', '_ELEKTRİK')
        )
        
        # Sütun adlarını tekrar düzenle
        merged_df = merged_df.rename(columns={
            'TAHMİN_TÜKETİM_SU': 'SU_TAHMİN',
            'TAHMİN_TÜKETİM_ELEKTRİK': 'ELEKTRİK_TAHMİN'
        })
        
        return merged_df.to_dict('records')