from fastapi import APIRouter, UploadFile, File, HTTPException, Depends, Query
from services.electric_service import ElectricPredictionService
import tempfile
import os

router = APIRouter(
    prefix="/api/electric",
    tags=["electric"],
    responses={404: {"description": "Not found"}},
)

async def get_electric_service(model_type: str = Query("ensemble")):
    """
    ElectricPredictionService bağımlılığını sağlar.
    Varsayılan olarak "ensemble" modelini kullanır.
    """
    try:
        return ElectricPredictionService(model_type)
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))

@router.post("/train")
async def train_electric_model(
    file: UploadFile = File(...),
    model_type: str = Query("ensemble"),
    service: ElectricPredictionService = Depends(get_electric_service)
):
    """Excel dosyasıyla elektrik tüketim modelini eğitir"""
    # Geçici dosya oluştur
    with tempfile.NamedTemporaryFile(delete=False, suffix='.xlsx') as tmp:
        tmp_path = tmp.name
        # Yüklenen dosyayı geçici dosyaya yaz
        content = await file.read()
        tmp.write(content)
    
    try:
        # Modeli eğit
        results = service.train(tmp_path)
        return results
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        # Geçici dosyayı temizle
        os.unlink(tmp_path)

@router.get("/predict")
async def predict_electric_consumption(
    months: int = Query(12, description="Tahmin yapılacak ay sayısı"),
    service: ElectricPredictionService = Depends(get_electric_service)
):
    """Gelecek aylar için elektrik tüketim tahmini yapar"""
    try:
        predictions = service.predict(months)
        return {"predictions": predictions}
    except FileNotFoundError as e:
        raise HTTPException(status_code=404, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

@router.get("/evaluate")
async def evaluate_electric_model(
    service: ElectricPredictionService = Depends(get_electric_service)
):
    """Model performans metriklerini döndürür"""
    try:
        evaluation = service.evaluate()
        return evaluation
    except FileNotFoundError as e:
        raise HTTPException(status_code=404, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))