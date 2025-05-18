import pandas as pd
import numpy as np

from models.base_model import BasePredictionModel
from models.electric.random_forest import ElectricRandomForestModel
from models.electric.xgboost_model import ElectricXGBoostModel
from models.electric.gradient_boosting import ElectricGradientBoostingModel
from utils.data_utils import calculate_metrics

class ElectricEnsembleModel(BasePredictionModel):
    """
    Elektrik tüketimi için RandomForest, XGBoost ve GradientBoosting 
    modellerini birleştiren topluluk modeli.
    """
    def __init__(self):
        self.model_name = "Ensemble-Elektrik"
        self.models = [
            ElectricRandomForestModel(),
            ElectricXGBoostModel(),
            ElectricGradientBoostingModel()
        ]
        
    def load_and_prepare_data(self, file_path):
        # İlk modelin veri yükleme fonksiyonunu kullan
        return self.models[0].load_and_prepare_data(file_path)
    
    def preprocess_data(self, df):
        # İlk modelin önişleme fonksiyonunu kullan
        return self.models[0].preprocess_data(df)
    
    def split_train_test(self, df, test_size=12):
        # İlk modelin veri ayırma fonksiyonunu kullan
        return self.models[0].split_train_test(df, test_size)
    
    def train_model(self, train_data, test_data):
        """Tüm modelleri eğitir ve topluluk tahminleri oluşturur"""
        model_results_list = []
        predictions = []
        
        # Her modeli eğit
        for model in self.models:
            result = model.train_model(train_data, test_data)
            model_results_list.append(result)
            predictions.append(result['pred'])
        
        # Topluluk tahminleri oluştur (ortalama)
        ensemble_pred = np.mean(predictions, axis=0)
        
        # Topluluk performansını değerlendir
        ensemble_metrics = calculate_metrics(test_data['TÜKETİM'], ensemble_pred)
        
        return {
            'model': model_results_list,  # Tüm modelleri sakla
            'metrics': ensemble_metrics,
            'importances': None,  # Topluluk için özellik önemi hesaplanmıyor
            'feature_names': model_results_list[0]['feature_names'],
            'test_dates': test_data['AY'],
            'y_test': test_data['TÜKETİM'],
            'pred': ensemble_pred
        }
    
    def predict_future(self, df, model_results, months=12):
        """Tüm modellerin gelecek tahminlerini birleştirir"""
        future_predictions = []
        
        # Her model için gelecek tahmini yap
        for i, model in enumerate(self.models):
            model_result = model_results['model'][i]  # İlgili modelin sonuçlarını al
            pred_df = model.predict_future(df, model_result, months)
            future_predictions.append(pred_df['TAHMİN_TÜKETİM'].values)
        
        # Tahminleri birleştir (ortalama)
        ensemble_future_pred = np.mean(future_predictions, axis=0)
        
        # Sonuç DataFrame'ini oluştur
        result_df = self.models[0].predict_future(df, model_results['model'][0], months)
        result_df['TAHMİN_TÜKETİM'] = ensemble_future_pred.astype(int)
        
        return result_df