import pandas as pd
import numpy as np
from .data_utils import get_month_number

def preprocess_water_data(df):
    """Su verileri için önişleme"""
    # Virgülden sonrasını yoksay (TÜKETİM için)
    if df['TÜKETİM'].dtype == object:
        # String verileri sayısal değerlere dönüştür
        df['TÜKETİM'] = df['TÜKETİM'].apply(
            lambda x: int(str(x).replace('.', '').split(',')[0]) if isinstance(x, str) else int(x)
        )
    else:
        # Zaten sayısal ise tamsayıya çevir
        df['TÜKETİM'] = df['TÜKETİM'].astype(int)

    # KİŞİ SAYISI sütununu sayısal hale getir
    df['KİŞİ SAYISI'] = df['KİŞİ SAYISI'].astype(int)

    # Ay ve yıl bilgilerini ayırma
    df['YIL'] = df['AY'].apply(lambda x: int(str(x).split('-')[0]))

    try:
        df['AY_NO'] = df['AY'].apply(lambda x: get_month_number(str(x).split('-')[1]))
    except Exception as e:
        print(f"Ay adı işlenirken hata: {e}")
        # Alternatif çözüm olarak try-except ile her bir satırı ayrı işle
        df['AY_NO'] = df['AY'].apply(
            lambda x: get_month_number(str(x).split('-')[1]) if len(str(x).split('-')) > 1 else 1
        )

    # Tarih sütunu oluştur (zaman serisi analizleri için)
    df['TARIH'] = pd.to_datetime(df['YIL'].astype(str) + '-' + df['AY_NO'].astype(str) + '-01')
    df = df.sort_values('TARIH')  # Tarihe göre sırala

    # Tatil ayı kolonunu ekleme (Temmuz ve Ağustos)
    df['TATIL_AYI'] = df['AY_NO'].apply(lambda x: 1 if x in [7, 8] else 0)

    # Mevsim kolonunu ekle
    df['MEVSIM'] = df['AY_NO'].apply(
        lambda x: 0 if x in [12, 1, 2] else  # Kış
                  1 if x in [3, 4, 5] else   # İlkbahar
                  2 if x in [6, 7, 8] else   # Yaz
                  3                           # Sonbahar
    )

    # Fourier özellikleri (mevsimselliği daha iyi modellemek için)
    df['SIN_MONTH'] = np.sin(2 * np.pi * df['AY_NO']/12)
    df['COS_MONTH'] = np.cos(2 * np.pi * df['AY_NO']/12)

    # Geçmiş tüketim değerlerini özellik olarak ekle (lag features)
    for lag in [1, 3, 6, 12]:
        df[f'TÜKETİM_{lag}AY_ÖNCE'] = df['TÜKETİM'].shift(lag)

    # Hareketli ortalamalar
    for window in [3, 6, 12]:
        df[f'ROLLING_MEAN_{window}'] = df['TÜKETİM'].rolling(window=window, min_periods=1).mean()

    # İlk 12 ayda geçmiş veriler olmayacağı için NaN değerlerini doldur
    lag_cols = [col for col in df.columns if 'ÖNCE' in col or 'ROLLING' in col]
    for col in lag_cols:
        df[col] = df[col].fillna(df['TÜKETİM'].mean())

    # Kişi başına tüketim trendi
    df['KİŞİ_BAŞINA_TÜKETİM'] = df['TÜKETİM'] / df['KİŞİ SAYISI']

    # Gereksiz sütunları çıkar
    columns_to_drop = ['BİRİMİ', 'MALİYET', 'KİŞİ BAŞI ORTALAMA TÜKETİM', 'KİŞİ BAŞI ORTALAMA MALİYET']
    df = df.drop(columns=[col for col in columns_to_drop if col in df.columns])

    return df

def preprocess_electric_data(df):
    """Elektrik verileri için önişleme"""
    # Virgülden sonrasını yoksay (TÜKETİM için)
    if df['TÜKETİM'].dtype == object:
        # String verileri sayısal değerlere dönüştür
        df['TÜKETİM'] = df['TÜKETİM'].apply(
            lambda x: float(str(x).replace('.', '').replace(',', '.')) if isinstance(x, str) else float(x)
        )

    # MALİYET sütununu düzelt
    if df['MALİYET'].dtype == object:
        df['MALİYET'] = df['MALİYET'].apply(
            lambda x: float(str(x).replace('.', '').replace(',', '.')) if isinstance(x, str) else float(x)
        )

    # KİŞİ SAYISI sütununu sayısal hale getir
    df['KİŞİ SAYISI'] = df['KİŞİ SAYISI'].astype(int)

    # Ay ve yıl bilgilerini ayırma
    df['YIL'] = df['AY'].apply(lambda x: int(str(x).split('-')[0]))

    try:
        df['AY_NO'] = df['AY'].apply(lambda x: get_month_number(str(x).split('-')[1]))
    except Exception as e:
        print(f"Ay adı işlenirken hata: {e}")
        # Alternatif çözüm olarak try-except ile her bir satırı ayrı işle
        df['AY_NO'] = df['AY'].apply(
            lambda x: get_month_number(str(x).split('-')[1]) if len(str(x).split('-')) > 1 else 1
        )

    # Tarih sütunu oluştur (zaman serisi analizleri için)
    df['TARIH'] = pd.to_datetime(df['YIL'].astype(str) + '-' + df['AY_NO'].astype(str) + '-01')
    df = df.sort_values('TARIH')  # Tarihe göre sırala

    # Tatil ayı kolonunu ekleme (Temmuz ve Ağustos)
    df['TATIL_AYI'] = df['AY_NO'].apply(lambda x: 1 if x in [7, 8] else 0)

    # Mevsim kolonunu ekle
    df['MEVSIM'] = df['AY_NO'].apply(
        lambda x: 0 if x in [12, 1, 2] else  # Kış
                  1 if x in [3, 4, 5] else   # İlkbahar
                  2 if x in [6, 7, 8] else   # Yaz
                  3                           # Sonbahar
    )

    # Elektrik kullanımına özel etkenler
    df['ISITMA_AYLARI'] = df['AY_NO'].apply(lambda x: 1 if x in [1, 2, 3, 11, 12] else 0)  # Kış + Kasım, Mart
    df['SOGUTMA_AYLARI'] = df['AY_NO'].apply(lambda x: 1 if x in [6, 7, 8, 9] else 0)  # Yaz + Eylül

    # Elektrik fiyat etkisi (enflasyon modelleme)
    df['BIRIM_FIYAT'] = df['MALİYET'] / df['TÜKETİM']
    df['BIRIM_FIYAT_TREND'] = df['BIRIM_FIYAT'].pct_change(12).fillna(0)  # Yıllık fiyat değişimi

    # Fourier özellikleri (mevsimselliği daha iyi modellemek için)
    df['SIN_MONTH'] = np.sin(2 * np.pi * df['AY_NO']/12)
    df['COS_MONTH'] = np.cos(2 * np.pi * df['AY_NO']/12)
    df['SIN_QUARTER'] = np.sin(2 * np.pi * df['AY_NO']/3)
    df['COS_QUARTER'] = np.cos(2 * np.pi * df['AY_NO']/3)

    # Yıl trendi (uzun vadeli trendi yakalamak için)
    min_year = df['YIL'].min()
    df['YIL_TREND'] = df['YIL'] - min_year
    df['YIL_TREND_SQ'] = df['YIL_TREND'] ** 2  # Büyüme/Azalma eğrisi için

    # Geçmiş tüketim değerlerini özellik olarak ekle (lag features)
    for lag in [1, 2, 3, 6, 12]:
        df[f'TÜKETİM_{lag}AY_ÖNCE'] = df['TÜKETİM'].shift(lag)

    # Hareketli ortalamalar
    for window in [3, 6, 12]:
        df[f'ROLLING_MEAN_{window}'] = df['TÜKETİM'].rolling(window=window, min_periods=1).mean()

    # Trend ve mevsimsel değişim oranları
    df['AYLARA_GÖRE_DEĞİŞİM'] = df['TÜKETİM'].pct_change(1).fillna(0)
    df['YILLIK_DEĞİŞİM'] = df['TÜKETİM'].pct_change(12).fillna(0)

    # Kişi sayısı değişiminin etkisi
    df['KİŞİ_DEĞİŞİM'] = df['KİŞİ SAYISI'].pct_change().fillna(0)
    df['KİŞİ_BAŞINA_TÜKETİM'] = df['TÜKETİM'] / df['KİŞİ SAYISI']
    df['KİŞİ_BAŞINA_TÜKETİM_DEĞİŞİM'] = df['KİŞİ_BAŞINA_TÜKETİM'].pct_change().fillna(0)

    # Ay bazlı mevsimsel endeksler (her ayın ortalama tüketim oranı)
    monthly_avg = df.groupby('AY_NO')['TÜKETİM'].mean()
    overall_avg = df['TÜKETİM'].mean()
    month_indexes = monthly_avg / overall_avg
    df['MEVSIM_ENDEKS'] = df['AY_NO'].map(month_indexes)

    # İlk 12 ayda geçmiş veriler olmayacağı için NaN değerlerini doldur
    lag_cols = [col for col in df.columns if 'ÖNCE' in col or 'ROLLING' in col or 'DEĞİŞİM' in col]
    for col in lag_cols:
        df[col] = df[col].fillna(0)

    # Gereksiz sütunları çıkar
    columns_to_drop = ['BİRİMİ', 'MALİYET', 'KİŞİ BAŞI ORTALAMA TÜKETİM', 'KİŞİ BAŞI ORTALAMA MALİYET']
    df = df.drop(columns=[col for col in columns_to_drop if col in df.columns])

    return df