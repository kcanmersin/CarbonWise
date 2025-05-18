import pandas as pd
import numpy as np

def get_month_number(month_name):
    """Türkçe ay adını sayıya dönüştürür"""
    # Önce metni temizle - boşluk karakterleri ve istenmeyen karakterleri kaldır
    month_name = month_name.strip()

    month_dict = {
        'OCAK': 1, 'ŞUBAT': 2, 'MART': 3, 'NİSAN': 4,
        'MAYIS': 5, 'HAZİRAN': 6, 'TEMMUZ': 7, 'AĞUSTOS': 8,
        'EYLÜL': 9, 'EKİM': 10, 'KASIM': 11, 'ARALIK': 12
    }

    try:
        return month_dict[month_name]
    except KeyError:
        # Sözlükte bulunmazsa benzer bir eşleşme arayalım
        for key in month_dict.keys():
            if key in month_name:
                return month_dict[key]
        # Hala bulunamazsa varsayılan değer
        print(f"Uyarı: '{month_name}' ay adı tanınamadı. Varsayılan değer 1 kullanılıyor.")
        return 1

def load_water_data(file_path):
    """Su tüketim verilerini yükler"""
    try:
        df = pd.read_excel(file_path, sheet_name="Su")
    except Exception as e:
        raise FileNotFoundError("'Su' tabı bulunamadı veya dosya hatalı: " + str(e))

    # Sütun adlarını temizle
    df.columns = df.columns.str.strip()
    return df

def load_electric_data(file_path):
    """Elektrik tüketim verilerini yükler"""
    try:
        df = pd.read_excel(file_path, sheet_name="Elektirik")
    except Exception as e:
        raise FileNotFoundError("'Elektrik' tabı bulunamadı veya dosya hatalı: " + str(e))

    # Sütun adlarını temizle
    df.columns = df.columns.str.strip()
    return df

def split_train_test(df, test_size=12):
    """Son 12 ayı test verisi olarak ayırır"""
    if len(df) <= test_size:
        raise ValueError(f"Veri seti çok küçük, en az {test_size+1} satır olmalı")

    train_data = df.iloc[:-test_size].copy()
    test_data = df.iloc[-test_size:].copy()

    return train_data, test_data

def calculate_metrics(y_true, y_pred):
    """Model performans metriklerini hesaplar"""
    from sklearn.metrics import mean_squared_error, mean_absolute_error
    
    mse = mean_squared_error(y_true, y_pred)
    mae = mean_absolute_error(y_true, y_pred)
    rmse = np.sqrt(mse)
    mape = np.mean(np.abs((y_true - y_pred) / y_true)) * 100

    return {
        'MSE': mse,
        'MAE': mae,
        'RMSE': rmse,
        'MAPE': mape
    }