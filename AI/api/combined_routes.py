from fastapi import APIRouter, UploadFile, File, HTTPException, Query
from services.combined_service import CombinedPredictionService
import tempfile
import os

router = APIRouter(
    prefix="/api/combined",
    tags=["combined"],
    responses={404: {"description": "Not found"}},
)

@router.post("/train")
async def train_combined_models(
    file: UploadFile = File(...),
):
    """Su ve elektrik tüketim modellerini birlikte eğitir"""
    service = CombinedPredictionService()
    
    # Geçici dosya oluştur
    with tempfile.NamedTemporaryFile(delete=False, suffix='.xlsx') as tmp:
        tmp_path = tmp.name
        # Yüklenen dosyayı geçici dosyaya yaz
        content = await file.read()
        tmp.write(content)
    
    try:
        # Birleşik modeli eğit
        results = service.train(tmp_path)
        return results
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        # Geçici dosyayı temizle
        os.unlink(tmp_path)

@router.get("/predict")
async def predict_combined_consumption(
    months: int = Query(12, description="Tahmin yapılacak ay sayısı"),
):
    """Gelecek aylar için hem su hem elektrik tüketim tahmini yapar"""
    service = CombinedPredictionService()
    
    try:
        predictions = service.predict(months)
        return {"predictions": predictions}
    except FileNotFoundError as e:
        raise HTTPException(status_code=404, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/evaluate")
async def evaluate_combined_models():
    """Her iki model için de performans metriklerini döndürür"""
    service = CombinedPredictionService()
    
    try:
        evaluation = service.evaluate()
        return evaluation
    except FileNotFoundError as e:
        raise HTTPException(status_code=404, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))