import pandas as pd
import joblib
import os

from models.water.random_forest import WaterRandomForestModel
from models.water.xgboost_model import WaterXGBoostModel
from models.water.gradient_boosting import WaterGradientBoostingModel
from models.water.ensemble import WaterEnsembleModel

class WaterPredictionService:
    """Su tüketimi tahmin servisi"""
    
    def __init__(self, model_type="ensemble"):
        """
        Model tipine göre servis oluşturur
        model_type: "rf" (RandomForest), "xgb" (XGBoost), "gb" (GradientBoosting), "ensemble" (Hepsi)
        """
        self.model_type = model_type
        
        # Model seçimi
        if model_type == "rf":
            self.model = WaterRandomForestModel()
        elif model_type == "xgb":
            self.model = WaterXGBoostModel()
        elif model_type == "gb":
            self.model = WaterGradientBoostingModel()
        elif model_type == "ensemble":
            self.model = WaterEnsembleModel()
        else:
            raise ValueError(f"Geçersiz model tipi: {model_type}. Geçerli değerler: rf, xgb, gb, ensemble")
        
        # Model dosya yolları
        self.models_dir = os.path.join("models", "saved")
        os.makedirs(self.models_dir, exist_ok=True)
        self.model_path = os.path.join(self.models_dir, f"water_{model_type}.joblib")
        self.data_path = os.path.join(self.models_dir, f"water_data.joblib")
    
    def train(self, file_path):
        """Excel dosyasından modeli eğitir ve kaydeder"""
        results = self.model.main_pipeline(file_path)
        
        # Model ve veriyi kaydet
        joblib.dump(results, self.model_path)
        
        return {
            'metrics': results['metrics'],
            'comparison': results['comparison'].to_dict('records'),
            'future_predictions': results['future_predictions'].to_dict('records')
        }
    
    def predict(self, months=12):
        """Gelecek aylar için su tüketim tahmini yapar"""
        if not os.path.exists(self.model_path):
            raise FileNotFoundError(f"Model dosyası bulunamadı: {self.model_path}. Önce eğitim yapın.")
        
        # Kaydedilmiş modeli yükle
        results = joblib.load(self.model_path)
        
        # En son verileri kullanarak tahmin yap
        predictions = results['future_predictions']
        
        return predictions.to_dict('records')
    
    def evaluate(self):
        """Model performans metriklerini döndürür"""
        if not os.path.exists(self.model_path):
            raise FileNotFoundError(f"Model dosyası bulunamadı: {self.model_path}. Önce eğitim yapın.")
        
        results = joblib.load(self.model_path)
        
        return {
            'metrics': results['metrics'],
            'comparison': results['comparison'].to_dict('records')
        }