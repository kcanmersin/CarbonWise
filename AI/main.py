from fastapi import FastAPI, HTTPException, BackgroundTasks
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel, Field
from typing import Optional, List, Dict, Any
import pandas as pd
import numpy as np
import pymysql
import pickle
import os
import json
from datetime import datetime, timedelta
import xgboost as xgb
from sklearn.ensemble import RandomForestRegressor, GradientBoostingRegressor
from sklearn.preprocessing import StandardScaler
from sklearn.metrics import mean_squared_error, mean_absolute_error, r2_score
import logging
from pathlib import Path
import asyncio
from contextlib import asynccontextmanager
from dotenv import load_dotenv

# Load environment variables
load_dotenv()

# Logging setup
logging.basicConfig(
    level=logging.INFO if os.getenv('DEBUG', 'false').lower() == 'true' else logging.WARNING,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# FastAPI app initialization
app = FastAPI(
    title="Energy Consumption Prediction API",
    description="Machine Learning API for predicting energy consumption (Electricity, Water, Natural Gas, Paper)",
    version="1.0.0"
)

# CORS middleware
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# Database connection config from environment variables
DB_CONFIG = {
    'host': os.getenv('DB_HOST', 'localhost'),
    'port': int(os.getenv('DB_PORT', 3306)),
    'user': os.getenv('DB_USER', 'root'),
    'password': os.getenv('DB_PASSWORD', ''),
    'database': os.getenv('DB_NAME', 'ENVIROMENT'),
    'ssl': {'ssl_mode': os.getenv('DB_SSL_MODE', 'DISABLED')},
    'cursorclass': pymysql.cursors.DictCursor
}

# Model storage paths from environment
MODELS_DIR = Path(os.getenv('MODELS_DIR', 'models'))
MODELS_DIR.mkdir(exist_ok=True)

LOGS_DIR = Path(os.getenv('LOGS_DIR', 'logs'))
LOGS_DIR.mkdir(exist_ok=True)

# Environment info
ENVIRONMENT = os.getenv('ENVIRONMENT', 'development')
DEBUG = os.getenv('DEBUG', 'false').lower() == 'true'

logger.info(f"Starting API in {ENVIRONMENT} mode, Debug: {DEBUG}")
logger.info(f"Database Host: {DB_CONFIG['host']}:{DB_CONFIG['port']}")
logger.info(f"Models Directory: {MODELS_DIR}")

# Resource types and their database tables
RESOURCE_MAPPING = {
    'electricity': 'Electrics',
    'water': 'Waters',
    'naturalgas': 'NaturalGas',
    'paper': 'Papers'
}

# Model types
MODEL_TYPES = ['rf', 'xgb', 'gb']
ENSEMBLE_TYPES = ['rf_gb', 'rf_xgb', 'gb_xgb', 'rf_gb_xgb']

# Pydantic models with fixed namespace conflicts
class TrainRequest(BaseModel):
    model_config = {"protected_namespaces": ()}
    
    resource_type: str = Field(..., description="Resource type: electricity, water, naturalgas, paper")
    building_id: Optional[str] = Field("0", description="Building ID (0 for all buildings, or UUID string)")
    model_types: Optional[List[str]] = Field(default_factory=lambda: MODEL_TYPES, 
                                           description="Model types to train")
    ensemble_types: Optional[List[str]] = Field(default_factory=lambda: ENSEMBLE_TYPES,
                                              description="Ensemble types to create")

class PredictRequest(BaseModel):
    model_config = {"protected_namespaces": ()}
    
    resource_type: str = Field(..., description="Resource type")
    building_id: Optional[str] = Field("0", description="Building ID (0 for all buildings, or UUID string)")
    model_type: str = Field(..., description="Model type or ensemble type")
    months_ahead: Optional[int] = Field(12, description="Number of months to predict")

class TrainResponse(BaseModel):
    model_config = {"protected_namespaces": ()}
    
    success: bool
    message: str
    models_trained: List[str]
    metrics: Dict[str, Any]
    data_info: Dict[str, Any]

class PredictResponse(BaseModel):
    model_config = {"protected_namespaces": ()}
    
    success: bool
    predictions: List[Dict[str, Any]]
    model_info: Dict[str, Any]

class ModelInfo(BaseModel):
    model_config = {"protected_namespaces": ()}
    
    resource_type: str
    building_id: str
    model_type: str
    trained_at: str
    metrics: Dict[str, float]
    data_points: int

# Database connection helper
def get_db_connection():
    try:
        connection = pymysql.connect(**DB_CONFIG)
        return connection
    except Exception as e:
        logger.error(f"Database connection error: {e}")
        raise HTTPException(status_code=500, detail="Database connection failed")

# Feature engineering functions
class FeatureEngineer:
    @staticmethod
    def create_features(df: pd.DataFrame, resource_type: str) -> pd.DataFrame:
        """Create features specific to resource type"""
        df = df.copy()
        
        # Basic time features
        df['Year'] = df['Date'].dt.year
        df['Month'] = df['Date'].dt.month
        df['YearMonth'] = df['Date'].dt.strftime('%Y-%m')
        
        # Seasonal features based on resource type
        if resource_type == 'electricity':
            df['IsHeatingMonth'] = df['Month'].apply(lambda x: 1 if x in [1, 2, 3, 11, 12] else 0)
            df['IsCoolingMonth'] = df['Month'].apply(lambda x: 1 if x in [6, 7, 8, 9] else 0)
            df['IsHolidayMonth'] = df['Month'].apply(lambda x: 1 if x in [7, 8] else 0)
        
        elif resource_type == 'naturalgas':
            df['IsHeatingMonth'] = df['Month'].apply(lambda x: 1 if x in [1, 2, 3, 10, 11, 12] else 0)
            df['IsNonHeatingMonth'] = df['Month'].apply(lambda x: 1 if x in [5, 6, 7, 8, 9] else 0)
            df['IsTransitionMonth'] = df['Month'].apply(lambda x: 1 if x in [4, 10] else 0)
        
        elif resource_type == 'paper':
            df['IsAcademicMonth'] = df['Month'].apply(lambda x: 1 if x in [9, 10, 11, 12, 1, 2, 3, 4, 5] else 0)
            df['IsHolidayMonth'] = df['Month'].apply(lambda x: 1 if x in [6, 7, 8] else 0)
            df['IsExamMonth'] = df['Month'].apply(lambda x: 1 if x in [1, 5, 6] else 0)
        
        elif resource_type == 'water':
            df['IsHolidayMonth'] = df['Month'].apply(lambda x: 1 if x in [7, 8] else 0)
        
        # Common features
        df['Season'] = df['Month'].apply(
            lambda x: 0 if x in [12, 1, 2] else 1 if x in [3, 4, 5] else 2 if x in [6, 7, 8] else 3
        )
        
        # Fourier features
        df['SinMonth'] = np.sin(2 * np.pi * df['Month'] / 12)
        df['CosMonth'] = np.cos(2 * np.pi * df['Month'] / 12)
        df['SinQuarter'] = np.sin(2 * np.pi * df['Month'] / 3)
        df['CosQuarter'] = np.cos(2 * np.pi * df['Month'] / 3)
        
        # Trend features
        min_year = df['Year'].min()
        df['YearTrend'] = df['Year'] - min_year
        df['YearTrendSq'] = df['YearTrend'] ** 2
        
        # Lag features
        for lag in [1, 2, 3, 6, 12]:
            df[f'Usage_Lag{lag}'] = df['Usage'].shift(lag)
        
        # Rolling means
        for window in [3, 6, 12]:
            df[f'RollingMean{window}'] = df['Usage'].rolling(window=window, min_periods=1).mean()
        
        # Change rates
        df['MonthlyChange'] = df['Usage'].pct_change(1).fillna(0)
        df['YearlyChange'] = df['Usage'].pct_change(12).fillna(0)
        
        # Seasonal index
        monthly_avg = df.groupby('Month')['Usage'].mean()
        overall_avg = df['Usage'].mean()
        month_indexes = monthly_avg / overall_avg
        df['SeasonalIndex'] = df['Month'].map(month_indexes)
        
        # Fill NaN values
        lag_cols = [col for col in df.columns if 'Lag' in col or 'Rolling' in col or 'Change' in col]
        for col in lag_cols:
            df[col] = df[col].fillna(0)
        
        return df
    
    @staticmethod
    def get_feature_columns(df: pd.DataFrame, resource_type: str) -> List[str]:
        """Get relevant feature columns for the resource type"""
        base_features = ['Month', 'Year', 'Season', 'SinMonth', 'CosMonth', 
                        'SinQuarter', 'CosQuarter', 'YearTrend', 'YearTrendSq', 'SeasonalIndex']
        
        # Add resource-specific features
        if resource_type == 'electricity':
            base_features.extend(['IsHeatingMonth', 'IsCoolingMonth', 'IsHolidayMonth'])
        elif resource_type == 'naturalgas':
            base_features.extend(['IsHeatingMonth', 'IsNonHeatingMonth', 'IsTransitionMonth'])
        elif resource_type == 'paper':
            base_features.extend(['IsAcademicMonth', 'IsHolidayMonth', 'IsExamMonth'])
        elif resource_type == 'water':
            base_features.extend(['IsHolidayMonth'])
        
        # Add lag and rolling features
        lag_rolling_features = [col for col in df.columns if 'Lag' in col or 'Rolling' in col or 'Change' in col]
        base_features.extend(lag_rolling_features)
        
        return base_features

# Data loader
class DataLoader:
    @staticmethod
    def load_data(resource_type: str, building_id: str = "0") -> pd.DataFrame:
        """Load data from database"""
        table_name = RESOURCE_MAPPING.get(resource_type)
        if not table_name:
            raise ValueError(f"Invalid resource type: {resource_type}")
        
        connection = get_db_connection()
        try:
            cursor = connection.cursor()
            
            if building_id == "0":
                # All buildings - monthly aggregation
                query = f"""
                SELECT
                    DATE_FORMAT(`Date`, '%Y-%m') as YearMonth,
                    SUM(`Usage`) as `Usage`,
                    MIN(`Date`) as `Date`
                FROM {table_name}
                WHERE `Usage` > 0
                GROUP BY DATE_FORMAT(`Date`, '%Y-%m')
                HAVING SUM(`Usage`) > 0
                ORDER BY MIN(`Date`)
                """
                logger.info(f"Executing aggregation query for all buildings: {query}")
                cursor.execute(query)
                logger.info(f"Loading aggregated data for all buildings from {table_name}")
            else:
                # Specific building
                query = f"""
                SELECT BuildingId, `Usage`, `Date`
                FROM {table_name}
                WHERE BuildingId = %s AND `Usage` > 0
                ORDER BY `Date`
                """
                logger.info(f"Executing building-specific query: {query} with building_id: {building_id}")
                cursor.execute(query, (building_id,))
                logger.info(f"Loading data for building {building_id} from {table_name}")
            
            data = cursor.fetchall()
            logger.info(f"Raw data fetched: {len(data)} records")
            
            # Debug: Print first few records
            if data:
                logger.info(f"First record: {data[0]}")
                if len(data) > 1:
                    logger.info(f"Last record: {data[-1]}")
            
            df = pd.DataFrame(data)
            
            if df.empty:
                logger.error(f"No data found for resource_type: {resource_type}, building_id: {building_id}")
                raise ValueError(f"No data found for resource_type: {resource_type}, building_id: {building_id}")
            
            logger.info(f"DataFrame created with {len(df)} rows and columns: {df.columns.tolist()}")
            
            df['Usage'] = pd.to_numeric(df['Usage'], errors='coerce')
            df['Date'] = pd.to_datetime(df['Date'])
            df = df.dropna()
            
            logger.info(f"After cleaning: {len(df)} records")
            
            # If specific building, aggregate to monthly
            if building_id != "0":
                df['Year'] = df['Date'].dt.year
                df['Month'] = df['Date'].dt.month
                monthly_data = df.groupby(['Year', 'Month']).agg({
                    'Usage': 'sum',
                    'Date': 'first'
                }).reset_index()
                df = monthly_data
                df['Date'] = pd.to_datetime(df[['Year', 'Month']].assign(day=1))
                df = df.sort_values('Date').reset_index(drop=True)
                logger.info(f"Aggregated to {len(df)} monthly records for building {building_id}")
            else:
                logger.info(f"Loaded {len(df)} monthly aggregated records for all buildings")
            
            # Debug: Show date range and usage range
            if not df.empty:
                logger.info(f"Date range: {df['Date'].min()} to {df['Date'].max()}")
                logger.info(f"Usage range: {df['Usage'].min()} to {df['Usage'].max()}")
                logger.info(f"Sample data:\n{df.head()}")
            
            return df
            
        except Exception as e:
            logger.error(f"Error in load_data: {e}")
            raise
        finally:
            connection.close()

# Model trainer
class ModelTrainer:
    def __init__(self):
        self.scalers = {}
    
    def train_single_model(self, X_train: pd.DataFrame, y_train: pd.Series, 
                          X_test: pd.DataFrame, y_test: pd.Series, 
                          model_type: str) -> tuple:
        """Train a single model"""
        if model_type == 'rf':
            model = RandomForestRegressor(
                n_estimators=200,
                max_depth=12,
                min_samples_split=2,
                min_samples_leaf=1,
                random_state=42
            )
            model.fit(X_train, y_train)
        
        elif model_type == 'xgb':
            model = xgb.XGBRegressor(
                n_estimators=200,
                learning_rate=0.1,
                max_depth=5,
                subsample=0.8,
                colsample_bytree=0.8,
                objective='reg:squarederror',
                tree_method='hist',
                random_state=42
            )
            model.fit(X_train, y_train)
        
        elif model_type == 'gb':
            # Scale data for GB
            scaler = StandardScaler()
            X_train_scaled = scaler.fit_transform(X_train)
            X_test_scaled = scaler.transform(X_test)
            self.scalers[model_type] = scaler
            
            model = GradientBoostingRegressor(
                n_estimators=300,
                learning_rate=0.05,
                max_depth=3,
                subsample=1.0,
                random_state=42
            )
            model.fit(X_train_scaled, y_train)
            
            # Use scaled data for prediction
            y_pred = model.predict(X_test_scaled)
        else:
            raise ValueError(f"Unknown model type: {model_type}")
        
        # Make predictions (handle scaling for GB)
        if model_type == 'gb':
            pass  # Already predicted above
        else:
            y_pred = model.predict(X_test)
        
        # Calculate metrics
        metrics = {
            'MSE': mean_squared_error(y_test, y_pred),
            'RMSE': np.sqrt(mean_squared_error(y_test, y_pred)),
            'MAE': mean_absolute_error(y_test, y_pred),
            'MAPE': np.mean(np.abs((y_test - y_pred) / y_test)) * 100,
            'R2': r2_score(y_test, y_pred)
        }
        
        return model, metrics, y_pred
    
    def create_ensemble(self, models: Dict, X_test: pd.DataFrame, y_test: pd.Series, 
                       ensemble_type: str) -> tuple:
        """Create ensemble predictions"""
        predictions = []
        
        if ensemble_type == 'rf_gb':
            model_types = ['rf', 'gb']
        elif ensemble_type == 'rf_xgb':
            model_types = ['rf', 'xgb']
        elif ensemble_type == 'gb_xgb':
            model_types = ['gb', 'xgb']
        elif ensemble_type == 'rf_gb_xgb':
            model_types = ['rf', 'gb', 'xgb']
        else:
            raise ValueError(f"Unknown ensemble type: {ensemble_type}")
        
        for model_type in model_types:
            if model_type in models:
                if model_type == 'gb' and model_type in self.scalers:
                    X_test_scaled = self.scalers[model_type].transform(X_test)
                    pred = models[model_type].predict(X_test_scaled)
                else:
                    pred = models[model_type].predict(X_test)
                predictions.append(pred)
        
        if not predictions:
            raise ValueError(f"No models available for ensemble {ensemble_type}")
        
        # Average predictions
        ensemble_pred = np.mean(predictions, axis=0)
        
        # Calculate metrics
        metrics = {
            'MSE': mean_squared_error(y_test, ensemble_pred),
            'RMSE': np.sqrt(mean_squared_error(y_test, ensemble_pred)),
            'MAE': mean_absolute_error(y_test, ensemble_pred),
            'MAPE': np.mean(np.abs((y_test - ensemble_pred) / y_test)) * 100,
            'R2': r2_score(y_test, ensemble_pred)
        }
        
        return ensemble_pred, metrics

# Model manager
class ModelManager:
    @staticmethod
    def save_model(model: Any, model_path: Path, metadata: Dict) -> None:
        """Save model and metadata"""
        model_path.parent.mkdir(parents=True, exist_ok=True)
        
        # Save model
        with open(model_path, 'wb') as f:
            pickle.dump(model, f)
        
        # Save metadata
        metadata_path = model_path.parent / 'metadata.json'
        with open(metadata_path, 'w') as f:
            json.dump(metadata, f, indent=2, default=str)
    
    @staticmethod
    def load_model(model_path: Path) -> tuple:
        """Load model and metadata"""
        if not model_path.exists():
            raise FileNotFoundError(f"Model not found: {model_path}")
        
        # Load model
        with open(model_path, 'rb') as f:
            model = pickle.load(f)
        
        # Load metadata
        metadata_path = model_path.parent / 'metadata.json'
        metadata = {}
        if metadata_path.exists():
            with open(metadata_path, 'r') as f:
                metadata = json.load(f)
        
        return model, metadata
    
    @staticmethod
    def get_model_path(resource_type: str, building_id: str, model_type: str) -> Path:
        """Get model file path"""
        # Sanitize building_id for file path (replace invalid characters)
        safe_building_id = building_id.replace("-", "_") if building_id != "0" else "0"
        return MODELS_DIR / resource_type / f"building_{safe_building_id}" / f"{model_type}_model.pkl"

# Prediction helper
class Predictor:
    @staticmethod
    def create_future_features(last_data: pd.DataFrame, months_ahead: int, 
                             resource_type: str) -> pd.DataFrame:
        """Create features for future months"""
        future_dates = []
        last_date = last_data['Date'].max()
        
        for i in range(1, months_ahead + 1):
            # Calculate next month
            if last_date.month == 12:
                next_date = last_date.replace(year=last_date.year + 1, month=1)
            else:
                next_date = last_date.replace(month=last_date.month + 1)
            future_dates.append(next_date)
            last_date = next_date
        
        # Create future dataframe
        future_df = pd.DataFrame({'Date': future_dates})
        
        # Add basic features
        future_df = FeatureEngineer.create_features(future_df, resource_type)
        
        # For lag features, use last known values
        last_usage_values = last_data['Usage'].tail(12).values
        
        for i, row_idx in enumerate(future_df.index):
            for lag in [1, 2, 3, 6, 12]:
                if lag <= len(last_usage_values):
                    future_df.loc[row_idx, f'Usage_Lag{lag}'] = last_usage_values[-lag]
                else:
                    future_df.loc[row_idx, f'Usage_Lag{lag}'] = last_usage_values[0]
        
        # Rolling means based on historical data
        for window in [3, 6, 12]:
            window_values = last_usage_values[-window:] if len(last_usage_values) >= window else last_usage_values
            future_df[f'RollingMean{window}'] = np.mean(window_values)
        
        # Change rates (set to 0 for future predictions)
        future_df['MonthlyChange'] = 0
        future_df['YearlyChange'] = 0
        
        return future_df
    
    @staticmethod
    def predict_with_model(model_path: Path, X_test: pd.DataFrame, 
                          model_type: str, scaler: Any = None) -> np.ndarray:
        """Make predictions with a single model"""
        model, metadata = ModelManager.load_model(model_path)
        
        if scaler is not None:
            X_test_scaled = scaler.transform(X_test)
            predictions = model.predict(X_test_scaled)
        else:
            predictions = model.predict(X_test)
        
        return predictions

# API Routes
@app.get("/")
async def root():
    return {"message": "Energy Consumption Prediction API", "version": "1.0.0"}

@app.get("/health")
async def health_check():
    """Health check endpoint with environment info"""
    try:
        # Test database connection
        connection = get_db_connection()
        cursor = connection.cursor()
        cursor.execute("SELECT 1")
        result = cursor.fetchone()
        connection.close()
        
        return {
            "status": "healthy",
            "database": "connected",
            "environment": ENVIRONMENT,
            "debug": DEBUG,
            "database_host": DB_CONFIG['host'],
            "database_name": DB_CONFIG['database'],
            "models_directory": str(MODELS_DIR),
            "timestamp": datetime.now().isoformat()
        }
    except Exception as e:
        logger.error(f"Health check failed: {e}")
        return {
            "status": "unhealthy",
            "database": "disconnected",
            "error": str(e),
            "environment": ENVIRONMENT,
            "timestamp": datetime.now().isoformat()
        }

@app.post("/train", response_model=TrainResponse)
async def train_models(request: TrainRequest, background_tasks: BackgroundTasks):
    """Train models for specified resource type and building"""
    try:
        # Validate inputs
        if request.resource_type not in RESOURCE_MAPPING:
            raise HTTPException(status_code=400, detail=f"Invalid resource type: {request.resource_type}")
        
        # Load data
        logger.info(f"Loading data for {request.resource_type}, building {request.building_id}")
        df = DataLoader.load_data(request.resource_type, request.building_id)
        
        # Check minimum data requirement
        if len(df) < 13:
            raise HTTPException(
                status_code=400, 
                detail=f"Insufficient data. Found {len(df)} records, minimum 13 required."
            )
        
        # Feature engineering
        logger.info("Creating features...")
        df = FeatureEngineer.create_features(df, request.resource_type)
        feature_cols = FeatureEngineer.get_feature_columns(df, request.resource_type)
        
        # Split data
        train_data = df.iloc[:-12].copy()
        test_data = df.iloc[-12:].copy()
        
        X_train = train_data[feature_cols]
        y_train = train_data['Usage']
        X_test = test_data[feature_cols]
        y_test = test_data['Usage']
        
        # Train models
        trainer = ModelTrainer()
        models_trained = []
        all_metrics = {}
        trained_models = {}
        
        # Train individual models
        for model_type in request.model_types:
            if model_type in MODEL_TYPES:
                logger.info(f"Training {model_type} model...")
                model, metrics, y_pred = trainer.train_single_model(
                    X_train, y_train, X_test, y_test, model_type
                )
                
                # Save model
                model_path = ModelManager.get_model_path(
                    request.resource_type, request.building_id, model_type
                )
                metadata = {
                    'resource_type': request.resource_type,
                    'building_id': request.building_id,
                    'model_type': model_type,
                    'trained_at': datetime.now().isoformat(),
                    'metrics': metrics,
                    'data_points': len(df),
                    'feature_columns': feature_cols
                }
                
                # Also save scaler if it exists
                if model_type in trainer.scalers:
                    scaler_path = model_path.parent / f"{model_type}_scaler.pkl"
                    with open(scaler_path, 'wb') as f:
                        pickle.dump(trainer.scalers[model_type], f)
                    metadata['has_scaler'] = True
                
                ModelManager.save_model(model, model_path, metadata)
                
                models_trained.append(model_type)
                all_metrics[model_type] = metrics
                trained_models[model_type] = model
        
        # Create ensembles
        for ensemble_type in request.ensemble_types:
            if ensemble_type in ENSEMBLE_TYPES:
                try:
                    logger.info(f"Creating {ensemble_type} ensemble...")
                    ensemble_pred, ensemble_metrics = trainer.create_ensemble(
                        trained_models, X_test, y_test, ensemble_type
                    )
                    
                    # Save ensemble metadata
                    ensemble_path = ModelManager.get_model_path(
                        request.resource_type, request.building_id, ensemble_type
                    )
                    ensemble_metadata = {
                        'resource_type': request.resource_type,
                        'building_id': request.building_id,
                        'model_type': ensemble_type,
                        'ensemble_components': ensemble_type.split('_'),
                        'trained_at': datetime.now().isoformat(),
                        'metrics': ensemble_metrics,
                        'data_points': len(df),
                        'feature_columns': feature_cols
                    }
                    
                    # Save ensemble metadata (no actual model file needed for ensemble)
                    ensemble_path.parent.mkdir(parents=True, exist_ok=True)
                    metadata_path = ensemble_path.parent / f'{ensemble_type}_metadata.json'
                    with open(metadata_path, 'w') as f:
                        json.dump(ensemble_metadata, f, indent=2, default=str)
                    
                    models_trained.append(ensemble_type)
                    all_metrics[ensemble_type] = ensemble_metrics
                    
                except Exception as e:
                    logger.warning(f"Failed to create {ensemble_type} ensemble: {e}")
        
        return TrainResponse(
            success=True,
            message=f"Successfully trained {len(models_trained)} models",
            models_trained=models_trained,
            metrics=all_metrics,
            data_info={
                'total_records': len(df),
                'training_records': len(train_data),
                'test_records': len(test_data),
                'date_range': f"{df['Date'].min()} to {df['Date'].max()}"
            }
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Training error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.post("/predict", response_model=PredictResponse)
async def predict_consumption(request: PredictRequest):
    """Predict future consumption using trained models"""
    try:
        # Validate inputs
        if request.resource_type not in RESOURCE_MAPPING:
            raise HTTPException(status_code=400, detail=f"Invalid resource type: {request.resource_type}")
        
        # Check if model exists
        model_path = ModelManager.get_model_path(
            request.resource_type, request.building_id, request.model_type
        )
        
        # For ensemble models, check metadata file
        if request.model_type in ENSEMBLE_TYPES:
            metadata_path = model_path.parent / f'{request.model_type}_metadata.json'
            if not metadata_path.exists():
                raise HTTPException(
                    status_code=404, 
                    detail=f"Ensemble model {request.model_type} not found for {request.resource_type}, building {request.building_id}"
                )
        else:
            if not model_path.exists():
                raise HTTPException(
                    status_code=404, 
                    detail=f"Model {request.model_type} not found for {request.resource_type}, building {request.building_id}"
                )
        
        # Load historical data for feature creation
        df = DataLoader.load_data(request.resource_type, request.building_id)
        if len(df) < 13:
            raise HTTPException(
                status_code=400, 
                detail=f"Insufficient historical data for predictions. Found {len(df)} records, minimum 13 required."
            )
        
        # Create features for historical data
        df = FeatureEngineer.create_features(df, request.resource_type)
        feature_cols = FeatureEngineer.get_feature_columns(df, request.resource_type)
        
        # Create future features
        future_df = Predictor.create_future_features(df, request.months_ahead, request.resource_type)
        X_future = future_df[feature_cols]
        
        predictions = []
        model_info = {}
        
        if request.model_type in ENSEMBLE_TYPES:
            # Handle ensemble predictions
            metadata_path = model_path.parent / f'{request.model_type}_metadata.json'
            with open(metadata_path, 'r') as f:
                ensemble_metadata = json.load(f)
            
            ensemble_predictions = []
            component_models = ensemble_metadata['ensemble_components']
            
            for component in component_models:
                component_path = ModelManager.get_model_path(
                    request.resource_type, request.building_id, component
                )
                
                if component_path.exists():
                    # Load scaler if exists
                    scaler = None
                    if component == 'gb':
                        scaler_path = component_path.parent / f"{component}_scaler.pkl"
                        if scaler_path.exists():
                            with open(scaler_path, 'rb') as f:
                                scaler = pickle.load(f)
                    
                    component_pred = Predictor.predict_with_model(
                        component_path, X_future, component, scaler
                    )
                    ensemble_predictions.append(component_pred)
            
            if not ensemble_predictions:
                raise HTTPException(
                    status_code=404, 
                    detail=f"No component models found for ensemble {request.model_type}"
                )
            
            # Average ensemble predictions
            final_predictions = np.mean(ensemble_predictions, axis=0)
            model_info = ensemble_metadata
            
        else:
            # Handle single model predictions
            # Load scaler if exists
            scaler = None
            if request.model_type == 'gb':
                scaler_path = model_path.parent / f"{request.model_type}_scaler.pkl"
                if scaler_path.exists():
                    with open(scaler_path, 'rb') as f:
                        scaler = pickle.load(f)
            
            final_predictions = Predictor.predict_with_model(
                model_path, X_future, request.model_type, scaler
            )
            
            # Load model metadata
            _, metadata = ModelManager.load_model(model_path)
            model_info = metadata
        
        # Format predictions
        for i, pred in enumerate(final_predictions):
            future_date = future_df.iloc[i]['Date']
            predictions.append({
                'date': future_date.strftime('%Y-%m'),
                'predicted_usage': float(pred),
                'month': int(future_date.month),
                'year': int(future_date.year)
            })
        
        return PredictResponse(
            success=True,
            predictions=predictions,
            model_info={
                'model_type': request.model_type,
                'resource_type': request.resource_type,
                'building_id': request.building_id,
                'trained_at': model_info.get('trained_at', 'Unknown'),
                'metrics': model_info.get('metrics', {}),
                'months_predicted': request.months_ahead
            }
        )
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Prediction error: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/models", response_model=List[ModelInfo])
async def list_models():
    """List all trained models"""
    models = []
    
    try:
        for resource_type in RESOURCE_MAPPING.keys():
            resource_dir = MODELS_DIR / resource_type
            if not resource_dir.exists():
                continue
            
            for building_dir in resource_dir.iterdir():
                if not building_dir.is_dir():
                    continue
                
                # Extract building_id from directory name (handle both UUID and "0")
                dir_name = building_dir.name
                if dir_name.startswith('building_'):
                    building_id_part = dir_name[9:]  # Remove 'building_' prefix
                    # Convert back to original format
                    if building_id_part == "0":
                        building_id = "0"
                    else:
                        # Convert underscore back to hyphens for UUID
                        building_id = building_id_part.replace("_", "-")
                else:
                    continue
                
                # Check for individual models
                for model_type in MODEL_TYPES:
                    model_path = building_dir / f"{model_type}_model.pkl"
                    if model_path.exists():
                        try:
                            _, metadata = ModelManager.load_model(model_path)
                            models.append(ModelInfo(
                                resource_type=resource_type,
                                building_id=building_id,
                                model_type=model_type,
                                trained_at=metadata.get('trained_at', 'Unknown'),
                                metrics=metadata.get('metrics', {}),
                                data_points=metadata.get('data_points', 0)
                            ))
                        except Exception as e:
                            logger.warning(f"Error loading model metadata: {e}")
                
                # Check for ensemble models
                for ensemble_type in ENSEMBLE_TYPES:
                    metadata_path = building_dir / f'{ensemble_type}_metadata.json'
                    if metadata_path.exists():
                        try:
                            with open(metadata_path, 'r') as f:
                                metadata = json.load(f)
                            models.append(ModelInfo(
                                resource_type=resource_type,
                                building_id=building_id,
                                model_type=ensemble_type,
                                trained_at=metadata.get('trained_at', 'Unknown'),
                                metrics=metadata.get('metrics', {}),
                                data_points=metadata.get('data_points', 0)
                            ))
                        except Exception as e:
                            logger.warning(f"Error loading ensemble metadata: {e}")
    
    except Exception as e:
        logger.error(f"Error listing models: {e}")
        raise HTTPException(status_code=500, detail=str(e))
    
    return models

@app.get("/models/{resource_type}/{building_id}")
async def get_building_models(resource_type: str, building_id: str):
    """Get all models for a specific resource type and building"""
    if resource_type not in RESOURCE_MAPPING:
        raise HTTPException(status_code=400, detail=f"Invalid resource type: {resource_type}")
    
    models = []
    safe_building_id = building_id.replace("-", "_") if building_id != "0" else "0"
    building_dir = MODELS_DIR / resource_type / f"building_{safe_building_id}"
    
    if not building_dir.exists():
        raise HTTPException(
            status_code=404, 
            detail=f"No models found for {resource_type}, building {building_id}"
        )
    
    try:
        # Individual models
        for model_type in MODEL_TYPES:
            model_path = building_dir / f"{model_type}_model.pkl"
            if model_path.exists():
                _, metadata = ModelManager.load_model(model_path)
                models.append({
                    'model_type': model_type,
                    'type': 'individual',
                    'trained_at': metadata.get('trained_at', 'Unknown'),
                    'metrics': metadata.get('metrics', {}),
                    'data_points': metadata.get('data_points', 0)
                })
        
        # Ensemble models
        for ensemble_type in ENSEMBLE_TYPES:
            metadata_path = building_dir / f'{ensemble_type}_metadata.json'
            if metadata_path.exists():
                with open(metadata_path, 'r') as f:
                    metadata = json.load(f)
                models.append({
                    'model_type': ensemble_type,
                    'type': 'ensemble',
                    'components': metadata.get('ensemble_components', []),
                    'trained_at': metadata.get('trained_at', 'Unknown'),
                    'metrics': metadata.get('metrics', {}),
                    'data_points': metadata.get('data_points', 0)
                })
        
        if not models:
            raise HTTPException(
                status_code=404, 
                detail=f"No trained models found for {resource_type}, building {building_id}"
            )
        
        return {
            'resource_type': resource_type,
            'building_id': building_id,
            'models': models,
            'total_models': len(models)
        }
        
    except HTTPException:
        raise
    except Exception as e:
        logger.error(f"Error getting building models: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.delete("/models/{resource_type}/{building_id}")
async def delete_building_models(resource_type: str, building_id: str):
    """Delete all models for a specific resource type and building"""
    if resource_type not in RESOURCE_MAPPING:
        raise HTTPException(status_code=400, detail=f"Invalid resource type: {resource_type}")
    
    safe_building_id = building_id.replace("-", "_") if building_id != "0" else "0"
    building_dir = MODELS_DIR / resource_type / f"building_{safe_building_id}"
    
    if not building_dir.exists():
        raise HTTPException(
            status_code=404, 
            detail=f"No models found for {resource_type}, building {building_id}"
        )
    
    try:
        import shutil
        shutil.rmtree(building_dir)
        return {
            'success': True,
            'message': f"Successfully deleted all models for {resource_type}, building {building_id}"
        }
    except Exception as e:
        logger.error(f"Error deleting models: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/data-info/{resource_type}")
async def get_data_info(resource_type: str, building_id: Optional[str] = "0"):
    """Get data information for a resource type and building"""
    if resource_type not in RESOURCE_MAPPING:
        raise HTTPException(status_code=400, detail=f"Invalid resource type: {resource_type}")
    
    try:
        df = DataLoader.load_data(resource_type, building_id)
        
        return {
            'resource_type': resource_type,
            'building_id': building_id,
            'total_records': len(df),
            'date_range': {
                'start': df['Date'].min().strftime('%Y-%m-%d'),
                'end': df['Date'].max().strftime('%Y-%m-%d')
            },
            'usage_stats': {
                'min': float(df['Usage'].min()),
                'max': float(df['Usage'].max()),
                'mean': float(df['Usage'].mean()),
                'std': float(df['Usage'].std())
            },
            'sufficient_for_training': len(df) >= 13,
            'monthly_data': df.groupby(df['Date'].dt.strftime('%Y-%m'))['Usage'].sum().to_dict()
        }
        
    except Exception as e:
        logger.error(f"Error getting data info: {e}")
        raise HTTPException(status_code=500, detail=str(e))

@app.get("/available-buildings/{resource_type}")
async def get_available_buildings(resource_type: str):
    """Get available buildings with sufficient data for training"""
    if resource_type not in RESOURCE_MAPPING:
        raise HTTPException(status_code=400, detail=f"Invalid resource type: {resource_type}")
    
    table_name = RESOURCE_MAPPING[resource_type]
    connection = get_db_connection()
    
    try:
        cursor = connection.cursor()
        query = f"""
        SELECT 
            BuildingId,
            COUNT(*) as total_records,
            SUM(CASE WHEN `Usage` > 0 THEN 1 ELSE 0 END) as non_zero_records,
            AVG(CASE WHEN `Usage` > 0 THEN `Usage` ELSE NULL END) as avg_usage,
            MIN(`Date`) as min_date,
            MAX(`Date`) as max_date
        FROM {table_name}
        GROUP BY BuildingId
        HAVING SUM(CASE WHEN `Usage` > 0 THEN 1 ELSE 0 END) >= 13
        ORDER BY SUM(CASE WHEN `Usage` > 0 THEN 1 ELSE 0 END) DESC
        """
        cursor.execute(query)
        buildings = cursor.fetchall()
        
        return {
            'resource_type': resource_type,
            'available_buildings': buildings,
            'total_buildings': len(buildings)
        }
        
    except Exception as e:
        logger.error(f"Error getting available buildings: {e}")
        raise HTTPException(status_code=500, detail=str(e))
    finally:
        connection.close()

if __name__ == "__main__":
    import uvicorn
    
    # Get configuration from environment
    host = os.getenv('API_HOST', '0.0.0.0')
    port = int(os.getenv('API_PORT', 8000))
    workers = int(os.getenv('API_WORKERS', 1))
    
    logger.info(f"Starting server on {host}:{port} with {workers} workers")
    
    if ENVIRONMENT == 'development':
        # Development mode with auto-reload
        uvicorn.run(
            "main:app", 
            host=host, 
            port=port, 
            reload=True,
            log_level="info" if DEBUG else "warning"
        )
    else:
        # Production mode
        uvicorn.run(
            app, 
            host=host, 
            port=port, 
            workers=workers,
            log_level="info" if DEBUG else "warning"
        )