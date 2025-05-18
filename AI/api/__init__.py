# API paketleri
from .water_routes import router as water_router
from .electric_routes import router as electric_router
from .combined_routes import router as combined_router
from .exceptions import CustomException, ExceptionType

__all__ = [
    'water_router',
    'electric_router',
    'combined_router',
    'CustomException',
    'ExceptionType'
]