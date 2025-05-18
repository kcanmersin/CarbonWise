import os
import joblib
import matplotlib.pyplot as plt
import numpy as np
import pandas as pd
from datetime import datetime

def save_model(model, model_name, directory="models/saved"):
    """Model ve ilgili bileşenleri kaydeder"""
    os.makedirs(directory, exist_ok=True)
    timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
    model_path = os.path.join(directory, f"{model_name}_{timestamp}.joblib")
    joblib.dump(model, model_path)
    return model_path

def load_model(model_path):
    """Kayıtlı modeli yükler"""
    if not os.path.exists(model_path):
        raise FileNotFoundError(f"Model dosyası bulunamadı: {model_path}")
    return joblib.load(model_path)

def plot_feature_importance(model_results, top_n=15, figsize=(10, 8)):
    """Modelin özellik önemliliklerini görselleştir"""
    # Topluluk modeli ise ilk modeli kullan
    if isinstance(model_results['model'], list):
        model_result = model_results['model'][0]
    else:
        model_result = model_results
    
    # Özellik önemi yoksa (Ensemble için olabilir)
    if model_result.get('importances') is None:
        print("Bu model için özellik önemi bilgisi mevcut değil.")
        return None
    
    # Özellik önemi DataFrame'i
    importances = model_result['importances']
    features = model_result['feature_names']
    
    importance_df = pd.DataFrame({
        'Feature': features,
        'Importance': importances
    }).sort_values('Importance', ascending=False)
    
    # Top N özellikleri seç
    importance_df = importance_df.head(top_n)
    
    # Görselleştirme
    plt.figure(figsize=figsize)
    plt.barh(importance_df['Feature'][::-1], importance_df['Importance'][::-1])
    plt.xlabel('Önem')
    plt.ylabel('Özellik')
    plt.title('Özellik Önemlilikleri')
    plt.tight_layout()
    
    return plt

def plot_predictions(model_results, figsize=(12, 6)):
    """Gerçek vs tahmin grafiği oluşturur"""
    comparison_df = model_results.get('comparison', None)
    
    if comparison_df is None:
        # Test seti tahminlerinden oluştur
        test_dates = model_results['test_dates']
        y_test = model_results['y_test']
        pred = model_results['pred']
        
        comparison_df = pd.DataFrame({
            'TARİH': test_dates,
            'GERÇEK': y_test,
            'TAHMİN': pred
        })
    
    # Tarih sütunu kontrolü
    date_col = 'TARİH' if 'TARİH' in comparison_df else 'TARIH'
    actual_col = 'GERÇEK TÜKETİM' if 'GERÇEK TÜKETİM' in comparison_df else 'GERÇEK'
    pred_col = 'TAHMİN TÜKETİM' if 'TAHMİN TÜKETİM' in comparison_df else 'TAHMİN'
    
    # Görselleştirme
    plt.figure(figsize=figsize)
    plt.plot(comparison_df[date_col], comparison_df[actual_col], 'o-', label='Gerçek')
    plt.plot(comparison_df[date_col], comparison_df[pred_col], 's--', label='Tahmin')
    plt.title('Gerçek vs Tahmin Edilen Tüketim')
    plt.xlabel('Tarih')
    plt.ylabel('Tüketim')
    plt.xticks(rotation=45)
    plt.legend()
    plt.grid(True, alpha=0.3)
    plt.tight_layout()
    
    return plt

def plot_future_predictions(predictions, figsize=(12, 6)):
    """Gelecek tahminleri grafiğini oluşturur"""
    # Liste ise DataFrame'e dönüştür
    if isinstance(predictions, list):
        predictions_df = pd.DataFrame(predictions)
    else:
        predictions_df = predictions
    
    # Birleşik tahmin mi kontrol et
    if 'SU_TAHMİN' in predictions_df.columns:
        # Her iki tüketim için grafik çiz
        fig, (ax1, ax2) = plt.subplots(2, 1, figsize=figsize)
        
        date_col = 'TARİH' if 'TARİH' in predictions_df.columns else 'TARIH'
        
        # Su grafiği
        ax1.plot(predictions_df[date_col], predictions_df['SU_TAHMİN'], 'o-', color='blue')
        ax1.set_title('Su Tüketim Tahmini')
        ax1.set_ylabel('Tüketim')
        ax1.grid(True, alpha=0.3)
        
        # Elektrik grafiği
        ax2.plot(predictions_df[date_col], predictions_df['ELEKTRİK_TAHMİN'], 'o-', color='orange')
        ax2.set_title('Elektrik Tüketim Tahmini')
        ax2.set_xlabel('Tarih')
        ax2.set_ylabel('Tüketim')
        ax2.grid(True, alpha=0.3)
        
        plt.xticks(rotation=45)
        plt.tight_layout()
    else:
        # Tek tüketim grafiği
        plt.figure(figsize=figsize)
        
        date_col = 'TARİH' if 'TARİH' in predictions_df.columns else 'TARIH'
        predict_col = 'TAHMİN_TÜKETİM' if 'TAHMİN_TÜKETİM' in predictions_df.columns else 'TAHMİN'
        
        plt.plot(predictions_df[date_col], predictions_df[predict_col], 'o-')
        plt.title('Gelecek Tüketim Tahmini')
        plt.xlabel('Tarih')
        plt.ylabel('Tüketim')
        plt.xticks(rotation=45)
        plt.grid(True, alpha=0.3)
        plt.tight_layout()
    
    return plt