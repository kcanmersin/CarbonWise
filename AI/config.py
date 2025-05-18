import os
from pydantic import BaseSettings
from typing import Optional, Dict, Any
import logging
from functools import lru_cache

class Settings(BaseSettings):
    """Uygulama ayarları"""
    
    # API yapılandırması
    APP_NAME: str = "AI Resource Prediction API"
    APP_VERSION: str = "1.0.0"
    API_PREFIX: str = "/api"
    DEBUG: bool = True
    
    # CORS yapılandırması
    CORS_ORIGINS: str = "*"  # Tüm kökenlere izin ver
    CORS_HEADERS: str = "*"  # Tüm başlıklara izin ver
    
    # Dosya yükleme yapılandırması
    UPLOAD_DIR: str = "uploads"
    MAX_UPLOAD_SIZE: int = 1024 * 1024 * 50  # 50 MB
    
    # Model yapılandırması
    MODELS_DIR: str = "models/saved"
    DEFAULT_MODEL_TYPE: str = "ensemble"
    
    # Loglama yapılandırması
    LOG_LEVEL: str = "INFO"
    LOG_FILE: str = "app.log"
    
    # Veritabanı yapılandırması (ileride kullanılabilir)
    DATABASE_URL: Optional[str] = None
    
    # Diğer ayarlar
    DEFAULT_MONTHS_TO_PREDICT: int = 12
    
    class Config:
        env_file = ".env"
        env_file_encoding = 'utf-8'
        case_sensitive = True

@lru_cache()
def get_settings() -> Settings:
    """Önbelleğe alınmış ayarları döndürür"""
    settings = Settings()
    
    # Gerekli dizinleri oluştur
    os.makedirs(settings.MODELS_DIR, exist_ok=True)
    os.makedirs(settings.UPLOAD_DIR, exist_ok=True)
    
    # Loglama yapılandırması
    logging_level = getattr(logging, settings.LOG_LEVEL.upper())
    logging.basicConfig(
        filename=settings.LOG_FILE,
        level=logging_level,
        format='%(asctime)s:%(levelname)s:%(message)s'
    )
    
    return settings

# Global ayarlar nesnesi
settings = get_settings()