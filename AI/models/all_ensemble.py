import pandas as pd
import numpy as np

from models.base_model import BasePredictionModel
from models.water.ensemble import WaterEnsembleModel
from models.electric.ensemble import ElectricEnsembleModel
from utils.data_utils import calculate_metrics

class AllEnsembleModel(BasePredictionModel):
    """
    Su ve elektrik tüketim tahminlerini birleştiren üst model.
    """
    def __init__(self):
        self.model_name = "Combined-Ensemble"
        self.water_model = WaterEnsembleModel()
        self.electric_model = ElectricEnsembleModel()
    
    def load_and_prepare_data(self, file_path):
        """Su ve elektrik verilerini ayrı ayrı yükler"""
        # Bu model için özel veri yükleme gerekiyor
        water_data = self.water_model.load_and_prepare_data(file_path)
        electric_data = self.electric_model.load_and_prepare_data(file_path)
        
        return {
            'water': water_data,
            'electric': electric_data
        }
    
    def preprocess_data(self, df_dict):
        """Su ve elektrik verilerini ayrı ayrı önişler"""
        return {
            'water': self.water_model.preprocess_data(df_dict['water']),
            'electric': self.electric_model.preprocess_data(df_dict['electric'])
        }
    
    def split_train_test(self, df_dict, test_size=12):
        """Her iki veri seti için eğitim ve test verilerini ayırır"""
        return {
            'water': self.water_model.split_train_test(df_dict['water'], test_size),
            'electric': self.electric_model.split_train_test(df_dict['electric'], test_size)
        }
    
    def train_model(self, train_test_dict, test_data=None):
        """Su ve elektrik modellerini ayrı ayrı eğitir"""
        water_results = self.water_model.train_model(
            train_test_dict['water'][0], train_test_dict['water'][1]
        )
        
        electric_results = self.electric_model.train_model(
            train_test_dict['electric'][0], train_test_dict['electric'][1]
        )
        
        return {
            'water': water_results,
            'electric': electric_results,
            'metrics': {
                'water': water_results['metrics'],
                'electric': electric_results['metrics']
            },
            'test_dates': {
                'water': water_results['test_dates'],
                'electric': electric_results['test_dates']
            },
            'y_test': {
                'water': water_results['y_test'],
                'electric': electric_results['y_test']
            },
            'pred': {
                'water': water_results['pred'],
                'electric': electric_results['pred']
            }
        }
    
    def predict_future(self, df_dict, model_results, months=12):
        """Su ve elektrik için gelecek tahminleri yapar"""
        water_future = self.water_model.predict_future(
            df_dict['water'], model_results['water'], months
        )
        
        electric_future = self.electric_model.predict_future(
            df_dict['electric'], model_results['electric'], months
        )
        
        # Sonuçları birleştirelim
        merged_df = pd.DataFrame({'TARİH': water_future['TARİH']})
        merged_df['SU_TAHMİN'] = water_future['TAHMİN_TÜKETİM']
        merged_df['ELEKTRİK_TAHMİN'] = electric_future['TAHMİN_TÜKETİM']
        
        return merged_df
    
    def main_pipeline(self, file_path):
        """Tüm modelleme sürecini yönetir"""
        # Verileri yükle
        df_dict = self.load_and_prepare_data(file_path)
        
        # Önişle
        preprocessed_dict = self.preprocess_data(df_dict)
        
        # Eğitim ve test verilerini ayır
        train_test_dict = self.split_train_test(preprocessed_dict)
        
        # Modelleri eğit
        model_results = self.train_model(train_test_dict)
        
        # Gelecek tahminlerini yap
        future_predictions = self.predict_future(preprocessed_dict, model_results)
        
        return {
            'metrics': model_results['metrics'],
            'comparisons': {
                'water': self.water_model.create_comparison_table(
                    model_results['test_dates']['water'],
                    model_results['y_test']['water'],
                    model_results['pred']['water']
                ),
                'electric': self.electric_model.create_comparison_table(
                    model_results['test_dates']['electric'],
                    model_results['y_test']['electric'],
                    model_results['pred']['electric']
                )
            },
            'future_predictions': future_predictions,
            'models': {
                'water': model_results['water']['model'],
                'electric': model_results['electric']['model']
            }
        }