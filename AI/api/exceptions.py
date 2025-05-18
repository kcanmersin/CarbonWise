from enum import Enum
from fastapi import HTTPException

class ExceptionType(str, Enum):
    """Özel istisna türleri"""
    NOT_FOUND = "not_found"
    BAD_REQUEST = "bad_request"
    UNAUTHORIZED = "unauthorized"
    FORBIDDEN = "forbidden"
    INTERNAL_SERVER_ERROR = "internal_server_error"
    VALIDATION_ERROR = "validation_error"
    MODEL_ERROR = "model_error"
    FILE_ERROR = "file_error"

class CustomException(HTTPException):
    """API için özelleştirilmiş istisna sınıfı"""
    
    def __init__(
        self,
        status_code: int,
        detail: str,
        exception_type: ExceptionType = ExceptionType.INTERNAL_SERVER_ERROR,
        headers: dict = None
    ):
        super().__init__(status_code=status_code, detail=detail, headers=headers)
        self.exception_type = exception_type

# Yaygın kullanılan istisna oluşturucular
def file_not_found_exception(filename: str) -> CustomException:
    """Dosya bulunamadığında fırlatılacak istisna"""
    return CustomException(
        status_code=404,
        detail=f"Dosya bulunamadı: {filename}",
        exception_type=ExceptionType.FILE_ERROR
    )

def model_not_found_exception(model_name: str) -> CustomException:
    """Model bulunamadığında fırlatılacak istisna"""
    return CustomException(
        status_code=404,
        detail=f"Model bulunamadı: {model_name}. Önce modeli eğitin.",
        exception_type=ExceptionType.MODEL_ERROR
    )

def invalid_file_format_exception() -> CustomException:
    """Dosya formatı geçersiz olduğunda fırlatılacak istisna"""
    return CustomException(
        status_code=400,
        detail="Geçersiz dosya formatı. Lütfen Excel (.xlsx) dosyası yükleyin.",
        exception_type=ExceptionType.FILE_ERROR
    )

def validation_error_exception(field: str, message: str) -> CustomException:
    """Doğrulama hatası olduğunda fırlatılacak istisna"""
    return CustomException(
        status_code=400,
        detail=f"Doğrulama hatası ({field}): {message}",
        exception_type=ExceptionType.VALIDATION_ERROR
    )

def model_training_error_exception(model_type: str, error_message: str) -> CustomException:
    """Model eğitimi sırasında hata olduğunda fırlatılacak istisna"""
    return CustomException(
        status_code=500,
        detail=f"{model_type} modeli eğitilirken hata oluştu: {error_message}",
        exception_type=ExceptionType.MODEL_ERROR
    )