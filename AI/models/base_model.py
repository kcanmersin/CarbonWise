from abc import ABC, abstractmethod
import pandas as pd
import numpy as np

class BasePredictionModel(ABC):
    """
    Bütün tahmin modelleri için temel arayüz.
    """
    @abstractmethod
    def preprocess_data(self, df):
        """Veriyi model için hazırlar"""
        pass
    
    @abstractmethod
    def split_train_test(self, df, test_size=12):
        """Eğitim ve test verisini ayırır"""
        pass
    
    @abstractmethod
    def train_model(self, train_data, test_data):
        """Modeli eğitir ve performans metriklerini döndürür"""
        pass
    
    @abstractmethod
    def predict_future(self, df, model_results, months=12):
        """Sonraki aylar için tahmin yapar"""
        pass
    
    @abstractmethod
    def load_and_prepare_data(self, file_path):
        """Excel dosyasından veriyi yükler ve hazırlar"""
        pass
    
    def main_pipeline(self, file_path):
        """
        Veri yükleme, model eğitimi ve tahmin akışını yönetir
        """
        df_raw = self.load_and_prepare_data(file_path)
        df = self.preprocess_data(df_raw)
        train_data, test_data = self.split_train_test(df)
        model_results = self.train_model(train_data, test_data)
        future_predictions = self.predict_future(df, model_results)
        
        return {
            'metrics': model_results['metrics'],
            'comparison': self.create_comparison_table(
                model_results['test_dates'],
                model_results['y_test'],
                model_results['pred']
            ),
            'future_predictions': future_predictions,
            'model': model_results['model']
        }
    
    def create_comparison_table(self, test_dates, y_test, pred):
        """Test verileri ve tahminleri karşılaştıran bir tablo oluşturur"""
        comparison_data = []

        for i in range(len(test_dates)):
            date = test_dates.iloc[i]
            actual = int(y_test.iloc[i])
            predicted = int(pred[i])
            error = predicted - actual
            error_percent = (error / actual) * 100 if actual != 0 else 0

            comparison_data.append({
                'TARİH': date,
                'GERÇEK TÜKETİM': actual,
                'TAHMİN TÜKETİM': predicted,
                'FARK': error,
                'FARK %': f"{error_percent:.2f}%"
            })

        return pd.DataFrame(comparison_data)