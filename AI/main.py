from fastapi import FastAPI, HTTPException
from fastapi.middleware.cors import CORSMiddleware
import uvicorn
from api import water_routes, electric_routes, combined_routes
import os
import logging

# Loglama yapılandırması
logging.basicConfig(
    filename='app.log',
    level=logging.INFO,
    format='%(asctime)s:%(levelname)s:%(message)s'
)

app = FastAPI(
    title="Tüketim Tahmin API",
    description="Elektrik ve su tüketimlerini tahmin etmek için makine öğrenimi API'si.",
    version="1.0.0"
)

# CORS yapılandırması
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Tüm kaynaklara izin ver (Üretimde daha kısıtlayıcı olmalı)
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Model kaydedilecek klasörleri oluştur
os.makedirs(os.path.join("models", "saved"), exist_ok=True)

# API rotalarını dahil et
app.include_router(water_routes.router)
app.include_router(electric_routes.router)
app.include_router(combined_routes.router)

@app.get("/", tags=["root"])
async def root():
    """API çalışıyor mu diye kontrol etmek için kök endpointi"""
    return {"message": "Tüketim Tahmin API'si aktif! /docs adresinden dokümantasyona erişebilirsiniz."}

@app.exception_handler(Exception)
async def global_exception_handler(request, exc):
    """Genel hata yakalayıcı"""
    error_msg = f"İstek işlenirken hata oluştu: {str(exc)}"
    logging.error(error_msg)
    return HTTPException(status_code=500, detail=error_msg)

if __name__ == "__main__":
    # API'yi başlat
    uvicorn.run(app, host="0.0.0.0", port=8000)