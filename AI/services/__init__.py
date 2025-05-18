# Tahmin servisleri paketi
from .water_service import WaterPredictionService
from .electric_service import ElectricPredictionService
from .combined_service import CombinedPredictionService

__all__ = [
    'WaterPredictionService',
    'ElectricPredictionService',
    'CombinedPredictionService'
]