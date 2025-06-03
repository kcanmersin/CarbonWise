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
from sklearn.preprocessing import StandardScaler, RobustScaler
from sklearn.metrics import mean_squared_error, mean_absolute_error, r2_score
import logging
from pathlib import Path
import asyncio
from contextlib import asynccontextmanager
from dotenv import load_dotenv
import math
import warnings
import re
warnings.filterwarnings('ignore')

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

# Utility functions
def safe_float_conversion(value):
    """Safely convert values to JSON-serializable floats"""
    if value is None:
        return None
    if isinstance(value, (int, float)):
        if math.isnan(value) or math.isinf(value):
            return 0.0
        return float(value)
    return 0.0

def safe_dict_conversion(data_dict):
    """Safely convert dictionary values for JSON serialization"""
    safe_dict = {}
    for key, value in data_dict.items():
        if isinstance(value, dict):
            safe_dict[key] = safe_dict_conversion(value)
        elif isinstance(value, (list, tuple)):
            safe_dict[key] = [safe_float_conversion(v) if isinstance(v, (int, float)) else v for v in value]
        else:
            safe_dict[key] = safe_float_conversion(value) if isinstance(value, (int, float)) else value
    return safe_dict

def clean_feature_names(feature_names):
    """Clean feature names for XGBoost compatibility"""
    cleaned_names = []
    for name in feature_names:
        # Convert to string and remove/replace problematic characters
        clean_name = str(name)
        clean_name = re.sub(r'[^\w]', '_', clean_name)  # Replace non-alphanumeric with underscore
        clean_name = re.sub(r'_+', '_', clean_name)     # Replace multiple underscores with single
        clean_name = clean_name.strip('_')               # Remove leading/trailing underscores
        
        # Ensure it doesn't start with a number
        if clean_name[0].isdigit():
            clean_name = 'f_' + clean_name
        
        cleaned_names.append(clean_name)
    
    return cleaned_names

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
        """Create enhanced features specific to resource type"""
        df = df.copy()
        
        # Basic time features
        df['Year'] = df['Date'].dt.year
        df['Month'] = df['Date'].dt.month
        df['Quarter'] = df['Date'].dt.quarter
        df['YearMonth'] = df['Date'].dt.strftime('%Y-%m')
        
        # Enhanced seasonal features based on resource type
        if resource_type == 'electricity':
            df['IsHeatingMonth'] = df['Month'].apply(lambda x: 1 if x in [1, 2, 3, 11, 12] else 0)
            df['IsCoolingMonth'] = df['Month'].apply(lambda x: 1 if x in [6, 7, 8, 9] else 0)
            df['IsHolidayMonth'] = df['Month'].apply(lambda x: 1 if x in [7, 8] else 0)
            df['IsPeakMonth'] = df['Month'].apply(lambda x: 1 if x in [1, 2, 7, 8, 12] else 0)
        elif resource_type == 'naturalgas':
            df['IsHeatingMonth'] = df['Month'].apply(lambda x: 1 if x in [1, 2, 3, 10, 11, 12] else 0)
            df['IsNonHeatingMonth'] = df['Month'].apply(lambda x: 1 if x in [5, 6, 7, 8, 9] else 0)
            df['IsTransitionMonth'] = df['Month'].apply(lambda x: 1 if x in [4, 10] else 0)
            df['IsPeakHeatingMonth'] = df['Month'].apply(lambda x: 1 if x in [1, 2, 12] else 0)
        elif resource_type == 'paper':
            df['IsAcademicMonth'] = df['Month'].apply(lambda x: 1 if x in [9, 10, 11, 12, 1, 2, 3, 4, 5] else 0)
            df['IsHolidayMonth'] = df['Month'].apply(lambda x: 1 if x in [6, 7, 8] else 0)
            df['IsExamMonth'] = df['Month'].apply(lambda x: 1 if x in [1, 5, 6] else 0)
            df['IsStartSemester'] = df['Month'].apply(lambda x: 1 if x in [9, 2] else 0)
        elif resource_type == 'water':
            df['IsHolidayMonth'] = df['Month'].apply(lambda x: 1 if x in [7, 8] else 0)
            df['IsSummerMonth'] = df['Month'].apply(lambda x: 1 if x in [6, 7, 8, 9] else 0)
        
        # Enhanced common features
        df['Season'] = df['Month'].apply(
            lambda x: 0 if x in [12, 1, 2] else 1 if x in [3, 4, 5] else 2 if x in [6, 7, 8] else 3
        )
        
        # Enhanced Fourier features for better seasonality capture
        df['SinMonth'] = np.sin(2 * np.pi * df['Month'] / 12)
        df['CosMonth'] = np.cos(2 * np.pi * df['Month'] / 12)
        df['SinQuarter'] = np.sin(2 * np.pi * df['Quarter'] / 4)
        df['CosQuarter'] = np.cos(2 * np.pi * df['Quarter'] / 4)
        df['SinSemester'] = np.sin(2 * np.pi * df['Month'] / 6)
        df['CosSemester'] = np.cos(2 * np.pi * df['Month'] / 6)
        
        # Enhanced trend features
        min_year = df['Year'].min()
        df['YearTrend'] = df['Year'] - min_year
        df['YearTrendSq'] = df['YearTrend'] ** 2
        df['MonthsFromStart'] = (df['Year'] - min_year) * 12 + df['Month'] - df['Month'].iloc[0]
        
        # Only create lag and rolling features if Usage column has values
        if 'Usage' in df.columns and not df['Usage'].isna().all():
            # Enhanced lag features - more variety
            for lag in [1, 2, 3, 6, 12, 24]:
                if len(df) > lag:
                    df[f'Usage_Lag{lag}'] = df['Usage'].shift(lag)
            
            # Enhanced rolling statistics
            for window in [3, 6, 12]:
                if len(df) >= window:
                    df[f'RollingMean{window}'] = df['Usage'].rolling(window=window, min_periods=1).mean()
                    df[f'RollingStd{window}'] = df['Usage'].rolling(window=window, min_periods=1).std()
                    df[f'RollingMin{window}'] = df['Usage'].rolling(window=window, min_periods=1).min()
                    df[f'RollingMax{window}'] = df['Usage'].rolling(window=window, min_periods=1).max()
            
            # Enhanced change rates
            df['MonthlyChange'] = df['Usage'].pct_change(1).fillna(0)
            df['YearlyChange'] = df['Usage'].pct_change(12).fillna(0)
            df['QuarterlyChange'] = df['Usage'].pct_change(3).fillna(0)
            
            # Seasonal decomposition features
            monthly_avg = df.groupby('Month')['Usage'].mean()
            overall_avg = df['Usage'].mean()
            if overall_avg > 0:
                month_indexes = monthly_avg / overall_avg
                df['SeasonalIndex'] = df['Month'].map(month_indexes)
                
                # Seasonal strength indicator
                df['SeasonalStrength'] = np.abs(df['SeasonalIndex'] - 1.0)
            else:
                df['SeasonalIndex'] = 1.0
                df['SeasonalStrength'] = 0.0
            
            # Moving averages for trend detection
            df['MA_Short'] = df['Usage'].rolling(window=3, min_periods=1).mean()
            df['MA_Long'] = df['Usage'].rolling(window=12, min_periods=1).mean()
            df['TrendIndicator'] = (df['MA_Short'] - df['MA_Long']) / df['MA_Long'].replace(0, 1)
            
            # Fill NaN values in lag and rolling features
            numeric_cols = df.select_dtypes(include=[np.number]).columns
            for col in numeric_cols:
                if col != 'Usage':
                    df[col] = df[col].fillna(method='ffill').fillna(method='bfill').fillna(0)
        else:
            # Initialize features with zeros for future predictions
            for lag in [1, 2, 3, 6, 12, 24]:
                df[f'Usage_Lag{lag}'] = 0
            
            for window in [3, 6, 12]:
                df[f'RollingMean{window}'] = 0
                df[f'RollingStd{window}'] = 0
                df[f'RollingMin{window}'] = 0
                df[f'RollingMax{window}'] = 0
            
            df['MonthlyChange'] = 0
            df['YearlyChange'] = 0
            df['QuarterlyChange'] = 0
            df['SeasonalIndex'] = 1.0
            df['SeasonalStrength'] = 0.0
            df['MA_Short'] = 0
            df['MA_Long'] = 0
            df['TrendIndicator'] = 0
        
        # Ensure no NaN values remain
        df = df.fillna(0)
        
        return df
    
    @staticmethod
    def get_feature_columns(df: pd.DataFrame, resource_type: str) -> List[str]:
        """Get relevant feature columns for the resource type"""
        base_features = ['Month', 'Year', 'Quarter', 'Season', 'SinMonth', 'CosMonth',
                        'SinQuarter', 'CosQuarter', 'SinSemester', 'CosSemester',
                        'YearTrend', 'YearTrendSq', 'MonthsFromStart', 'SeasonalIndex', 
                        'SeasonalStrength', 'TrendIndicator']
        
        # Add resource-specific features
        if resource_type == 'electricity':
            base_features.extend(['IsHeatingMonth', 'IsCoolingMonth', 'IsHolidayMonth', 'IsPeakMonth'])
        elif resource_type == 'naturalgas':
            base_features.extend(['IsHeatingMonth', 'IsNonHeatingMonth', 'IsTransitionMonth', 'IsPeakHeatingMonth'])
        elif resource_type == 'paper':
            base_features.extend(['IsAcademicMonth', 'IsHolidayMonth', 'IsExamMonth', 'IsStartSemester'])
        elif resource_type == 'water':
            base_features.extend(['IsHolidayMonth', 'IsSummerMonth'])
        
        # Add lag and rolling features that exist in the dataframe
        for col in df.columns:
            if any(pattern in col for pattern in ['Lag', 'Rolling', 'Change', 'MA_']):
                if col not in base_features:
                    base_features.append(col)
        
        # Return only features that actually exist in the dataframe
        return [col for col in base_features if col in df.columns]
    
    @staticmethod
    def get_legacy_features(resource_type: str) -> List[str]:
        """Get legacy feature set for backward compatibility"""
        base_features = ['Month', 'Year', 'Season', 'SinMonth', 'CosMonth',
                        'SinQuarter', 'CosQuarter', 'YearTrend', 'YearTrendSq', 'SeasonalIndex']
        
        # Add resource-specific features (legacy only)
        if resource_type == 'electricity':
            base_features.extend(['IsHeatingMonth', 'IsCoolingMonth', 'IsHolidayMonth'])
        elif resource_type == 'naturalgas':
            base_features.extend(['IsHeatingMonth', 'IsNonHeatingMonth', 'IsTransitionMonth'])
        elif resource_type == 'paper':
            base_features.extend(['IsAcademicMonth', 'IsHolidayMonth', 'IsExamMonth'])
        elif resource_type == 'water':
            base_features.extend(['IsHolidayMonth'])
        
        # Add common legacy lag and rolling features
        legacy_lag_rolling = [
            'Usage_Lag1', 'Usage_Lag2', 'Usage_Lag3', 'Usage_Lag6', 'Usage_Lag12',
            'RollingMean3', 'RollingMean6', 'RollingMean12',
            'MonthlyChange', 'YearlyChange'
        ]
        base_features.extend(legacy_lag_rolling)
        
        return base_features

# Data loader with improved error handling
class DataLoader:
    @staticmethod
    def load_data(resource_type: str, building_id: str = "0") -> pd.DataFrame:
        """Load data from database with improved error handling"""
        table_name = RESOURCE_MAPPING.get(resource_type)
        if not table_name:
            raise ValueError(f"Invalid resource type: {resource_type}")
        
        connection = get_db_connection()
        
        try:
            cursor = connection.cursor()
            
            if building_id == "0":
                # All buildings - FIXED SQL query with proper GROUP BY
                query = f"""
                    SELECT
                        YEAR(`Date`) as Year,
                        MONTH(`Date`) as Month,
                        SUM(`Usage`) as `Usage`
                    FROM {table_name}
                    WHERE `Usage` > 0 
                        AND `Date` IS NOT NULL
                        AND `Usage` IS NOT NULL
                    GROUP BY YEAR(`Date`), MONTH(`Date`)
                    HAVING SUM(`Usage`) > 0
                    ORDER BY YEAR(`Date`), MONTH(`Date`)
                """
                logger.info(f"Executing aggregation query for all buildings: {query}")
                cursor.execute(query)
                logger.info(f"Loading aggregated data for all buildings from {table_name}")
            else:
                # Specific building - FIXED SQL query with proper GROUP BY
                query = f"""
                    SELECT 
                        BuildingId,
                        YEAR(`Date`) as Year,
                        MONTH(`Date`) as Month,
                        SUM(`Usage`) as `Usage`
                    FROM {table_name}
                    WHERE BuildingId = %s 
                        AND `Usage` > 0
                        AND `Date` IS NOT NULL
                        AND `Usage` IS NOT NULL
                    GROUP BY BuildingId, YEAR(`Date`), MONTH(`Date`)
                    HAVING SUM(`Usage`) > 0
                    ORDER BY YEAR(`Date`), MONTH(`Date`)
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
            
            if not data:
                logger.error(f"No data found for resource_type: {resource_type}, building_id: {building_id}")
                raise ValueError(f"No data found for resource_type: {resource_type}, building_id: {building_id}")
            
            df = pd.DataFrame(data)
            logger.info(f"DataFrame created with {len(df)} rows and columns: {df.columns.tolist()}")
            
            # Data cleaning and conversion
            df['Usage'] = pd.to_numeric(df['Usage'], errors='coerce')
            
            # Create Date column from Year and Month
            df['Date'] = pd.to_datetime(df[['Year', 'Month']].assign(day=1))
            
            # Remove invalid data
            df = df.dropna(subset=['Usage', 'Date'])
            df = df[df['Usage'] > 0]  # Remove zero or negative usage
            
            logger.info(f"After cleaning: {len(df)} records")
            
            if df.empty:
                raise ValueError(f"No valid data after cleaning for resource_type: {resource_type}, building_id: {building_id}")
            
            # Sort by date
            df = df.sort_values('Date').reset_index(drop=True)
            
            logger.info(f"Loaded {len(df)} monthly records")
            
            # Debug: Show date range and usage range
            logger.info(f"Date range: {df['Date'].min()} to {df['Date'].max()}")
            logger.info(f"Usage range: {df['Usage'].min()} to {df['Usage'].max()}")
            logger.info(f"Sample data:\n{df.head()}")
            
            return df
            
        except Exception as e:
            logger.error(f"Error in load_data: {e}")
            raise
        finally:
            connection.close()

# Model trainer with improved error handling and better hyperparameters
class ModelTrainer:
    def __init__(self):
        self.scalers = {}
        self.feature_importance = {}
        self.xgb_feature_names = {}  # Store cleaned feature names for XGBoost
    
    def train_single_model(self, X_train: pd.DataFrame, y_train: pd.Series,
                          X_test: pd.DataFrame, y_test: pd.Series,
                          model_type: str) -> tuple:
        """Train a single model with improved hyperparameters and error handling"""
        try:
            # Ensure all features are numeric and finite
            X_train = X_train.astype(float)
            X_test = X_test.astype(float)
            y_train = y_train.astype(float)
            y_test = y_test.astype(float)
            
            # Replace any remaining infinite values
            X_train = X_train.replace([np.inf, -np.inf], 0)
            X_test = X_test.replace([np.inf, -np.inf], 0)
            
            # Additional data validation for XGBoost
            if model_type == 'xgb':
                # Check for any remaining NaN or inf values
                if X_train.isnull().any().any() or X_test.isnull().any().any():
                    logger.warning("Found NaN values in XGBoost data, filling with 0")
                    X_train = X_train.fillna(0)
                    X_test = X_test.fillna(0)
                
                if y_train.isnull().any() or y_test.isnull().any():
                    logger.warning("Found NaN values in target, dropping affected rows")
                    train_mask = ~y_train.isnull()
                    test_mask = ~y_test.isnull()
                    X_train = X_train[train_mask]
                    y_train = y_train[train_mask]
                    X_test = X_test[test_mask]
                    y_test = y_test[test_mask]
                
                # Ensure positive values only (energy consumption can't be negative)
                y_train = y_train.clip(lower=0.001)  # Prevent exactly zero values
                y_test = y_test.clip(lower=0.001)
            
            if model_type == 'rf':
                model = RandomForestRegressor(
                    n_estimators=300,  # Increased
                    max_depth=15,      # Increased
                    min_samples_split=2,
                    min_samples_leaf=1,
                    max_features='sqrt',  # Better feature selection
                    random_state=42,
                    n_jobs=-1
                )
                model.fit(X_train, y_train)
                y_pred = model.predict(X_test)
                self.feature_importance[model_type] = dict(zip(X_train.columns, model.feature_importances_))
                
            elif model_type == 'xgb':
                # CRITICAL FIX: Clean feature names for XGBoost
                original_columns = X_train.columns.tolist()
                cleaned_columns = clean_feature_names(original_columns)
                
                # Store mapping for later use
                self.xgb_feature_names[model_type] = dict(zip(original_columns, cleaned_columns))
                
                # Create cleaned DataFrames
                X_train_clean = X_train.copy()
                X_test_clean = X_test.copy()
                X_train_clean.columns = cleaned_columns
                X_test_clean.columns = cleaned_columns
                
                # CRITICAL FIX: Improved XGBoost parameters
                model = xgb.XGBRegressor(
                    n_estimators=200,      # Reduced for stability
                    learning_rate=0.15,    # Increased learning rate
                    max_depth=4,           # Reduced to prevent overfitting
                    subsample=0.9,         
                    colsample_bytree=0.9,  
                    colsample_bylevel=0.9,
                    reg_alpha=0.001,       # REDUCED L1 regularization (was causing zero predictions)
                    reg_lambda=0.01,       # REDUCED L2 regularization
                    gamma=0,               # No gamma (minimum split loss)
                    min_child_weight=1,    # Lower minimum child weight
                    objective='reg:squarederror',
                    tree_method='hist',
                    random_state=42,
                    n_jobs=-1,
                    verbosity=1,
                    enable_categorical=False,
                    validate_parameters=True,
                    # CRITICAL: Add scaling factor if values are very small
                    scale_pos_weight=1.0
                )
                
                # CRITICAL FIX: Scale data for XGBoost if values are too small
                y_mean = y_train.mean()
                if y_mean < 1:
                    # Scale up small values
                    scale_factor = 1000
                    y_train_scaled = y_train * scale_factor
                    y_test_scaled = y_test * scale_factor
                    logger.info(f"Scaling XGBoost target by {scale_factor} (original mean: {y_mean:.6f})")
                else:
                    scale_factor = 1
                    y_train_scaled = y_train
                    y_test_scaled = y_test
                
                # Log data statistics for debugging
                logger.info(f"XGBoost training data shape: {X_train_clean.shape}")
                logger.info(f"XGBoost target range: {y_train_scaled.min():.3f} - {y_train_scaled.max():.3f}")
                logger.info(f"XGBoost target mean: {y_train_scaled.mean():.3f}")
                logger.info(f"XGBoost feature names (first 5): {cleaned_columns[:5]}")
                
                # Fit with evaluation set but less aggressive early stopping
                model.fit(
                    X_train_clean, y_train_scaled,
                    eval_set=[(X_train_clean, y_train_scaled), (X_test_clean, y_test_scaled)],
                    eval_metric='rmse',
                    early_stopping_rounds=50,  # Less aggressive early stopping
                    verbose=False
                )
                
                # Make predictions and scale back if needed
                y_pred_scaled = model.predict(X_test_clean)
                y_pred = y_pred_scaled / scale_factor
                
                # Debug predictions
                logger.info(f"XGBoost raw predictions range: {y_pred_scaled.min():.3f} - {y_pred_scaled.max():.3f}")
                logger.info(f"XGBoost final predictions range: {y_pred.min():.3f} - {y_pred.max():.3f}")
                logger.info(f"XGBoost predictions mean: {y_pred.mean():.3f}")
                logger.info(f"XGBoost target mean: {y_test.mean():.3f}")
                
                # Get feature importance safely
                try:
                    self.feature_importance[model_type] = dict(zip(cleaned_columns, model.feature_importances_))
                except:
                    self.feature_importance[model_type] = {}
                
            elif model_type == 'gb':
                # Use RobustScaler instead of StandardScaler for better outlier handling
                scaler = RobustScaler()
                X_train_scaled = scaler.fit_transform(X_train)
                X_test_scaled = scaler.transform(X_test)
                self.scalers[model_type] = scaler
                
                model = GradientBoostingRegressor(
                    n_estimators=400,      # Increased
                    learning_rate=0.08,    # Slightly increased
                    max_depth=6,           # Increased
                    subsample=0.9,
                    max_features='sqrt',   # Better feature selection
                    random_state=42,
                    validation_fraction=0.2,
                    n_iter_no_change=30,   # Early stopping
                    tol=1e-6
                )
                model.fit(X_train_scaled, y_train)
                y_pred = model.predict(X_test_scaled)
                self.feature_importance[model_type] = dict(zip(X_train.columns, model.feature_importances_))
                
            else:
                raise ValueError(f"Unknown model type: {model_type}")
            
            # Ensure predictions are positive (energy consumption cannot be negative)
            y_pred = np.maximum(y_pred, 0)
            
            # Additional validation for XGBoost predictions
            if model_type == 'xgb':
                if np.all(y_pred == 0):
                    logger.error("XGBoost produced all zero predictions!")
                    # Try to understand why
                    logger.error(f"Model best iteration: {getattr(model, 'best_iteration', 'N/A')}")
                    logger.error(f"Model best score: {getattr(model, 'best_score', 'N/A')}")
                    
                    # Check if model was actually trained
                    if hasattr(model, 'feature_importances_') and len(model.feature_importances_) > 0:
                        top_features = sorted(zip(cleaned_columns, model.feature_importances_), 
                                            key=lambda x: x[1], reverse=True)[:5]
                        logger.error(f"Top 5 features: {top_features}")
                    
                    # Return a simple prediction based on mean
                    logger.warning("Falling back to mean-based predictions for XGBoost")
                    y_pred = np.full_like(y_test, y_train.mean())
            
            # Calculate metrics with safe conversion
            mse = mean_squared_error(y_test, y_pred)
            rmse = np.sqrt(mse)
            mae = mean_absolute_error(y_test, y_pred)
            
            # Safe MAPE calculation
            mask = y_test != 0
            if mask.sum() > 0:
                mape = np.mean(np.abs((y_test[mask] - y_pred[mask]) / y_test[mask])) * 100
            else:
                mape = 0.0
            
            r2 = r2_score(y_test, y_pred)
            
            metrics = {
                'MSE': safe_float_conversion(mse),
                'RMSE': safe_float_conversion(rmse),
                'MAE': safe_float_conversion(mae),
                'MAPE': safe_float_conversion(mape),
                'R2': safe_float_conversion(r2)
            }
            
            logger.info(f"{model_type} model trained - R2: {r2:.3f}, RMSE: {rmse:.3f}, MAPE: {mape:.1f}%")
            
            return model, metrics, y_pred
            
        except Exception as e:
            logger.error(f"Error training {model_type} model: {e}")
            raise
    
    def create_ensemble(self, models: Dict, X_test: pd.DataFrame, y_test: pd.Series,
                       ensemble_type: str) -> tuple:
        """Create ensemble predictions with improved error handling"""
        try:
            predictions = []
            weights = []
            
            if ensemble_type == 'rf_gb':
                model_types = ['rf', 'gb']
                # Weight based on model performance (R2 scores)
                type_weights = [0.6, 0.4]  # RF typically performs better
            elif ensemble_type == 'rf_xgb':
                model_types = ['rf', 'xgb']
                type_weights = [0.5, 0.5]
            elif ensemble_type == 'gb_xgb':
                model_types = ['gb', 'xgb']
                type_weights = [0.4, 0.6]  # XGB typically performs better
            elif ensemble_type == 'rf_gb_xgb':
                model_types = ['rf', 'gb', 'xgb']
                type_weights = [0.4, 0.3, 0.3]
            else:
                raise ValueError(f"Unknown ensemble type: {ensemble_type}")
            
            for i, model_type in enumerate(model_types):
                if model_type in models:
                    if model_type == 'gb' and model_type in self.scalers:
                        X_test_scaled = self.scalers[model_type].transform(X_test)
                        pred = models[model_type].predict(X_test_scaled)
                    elif model_type == 'xgb' and model_type in self.xgb_feature_names:
                        # Handle XGBoost with cleaned feature names
                        X_test_clean = X_test.copy()
                        original_cols = X_test.columns.tolist()
                        cleaned_cols = [self.xgb_feature_names[model_type].get(col, col) for col in original_cols]
                        X_test_clean.columns = cleaned_cols
                        pred = models[model_type].predict(X_test_clean)
                    else:
                        pred = models[model_type].predict(X_test)
                    
                    # Ensure predictions are positive
                    pred = np.maximum(pred, 0)
                    predictions.append(pred)
                    weights.append(type_weights[i])
            
            if not predictions:
                raise ValueError(f"No models available for ensemble {ensemble_type}")
            
            # Weighted average predictions
            weights = np.array(weights) / np.sum(weights)  # Normalize weights
            ensemble_pred = np.average(predictions, axis=0, weights=weights)
            
            # Calculate metrics with safe conversion
            mse = mean_squared_error(y_test, ensemble_pred)
            rmse = np.sqrt(mse)
            mae = mean_absolute_error(y_test, ensemble_pred)
            
            # Safe MAPE calculation
            mask = y_test != 0
            if mask.sum() > 0:
                mape = np.mean(np.abs((y_test[mask] - ensemble_pred[mask]) / y_test[mask])) * 100
            else:
                mape = 0.0
            
            r2 = r2_score(y_test, ensemble_pred)
            
            metrics = {
                'MSE': safe_float_conversion(mse),
                'RMSE': safe_float_conversion(rmse),
                'MAE': safe_float_conversion(mae),
                'MAPE': safe_float_conversion(mape),
                'R2': safe_float_conversion(r2)
            }
            
            logger.info(f"{ensemble_type} ensemble created - R2: {r2:.3f}, RMSE: {rmse:.3f}, MAPE: {mape:.1f}%")
            
            return ensemble_pred, metrics
            
        except Exception as e:
            logger.error(f"Error creating ensemble {ensemble_type}: {e}")
            raise

# Model manager
class ModelManager:
    @staticmethod
    def save_model(model: Any, model_path: Path, metadata: Dict) -> None:
        """Save model and metadata"""
        model_path.parent.mkdir(parents=True, exist_ok=True)
        
        # Save model
        with open(model_path, 'wb') as f:
            pickle.dump(model, f)
        
        # Safe metadata conversion
        safe_metadata = safe_dict_conversion(metadata)
        
        # Save metadata
        metadata_path = model_path.parent / 'metadata.json'
        with open(metadata_path, 'w') as f:
            json.dump(safe_metadata, f, indent=2, default=str)
    
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
        """Create features for future months with improved handling"""
        try:
            future_dates = []
            last_date = last_data['Date'].max()
            
            for i in range(1, months_ahead + 1):
                # Calculate next month properly
                next_month = last_date.month + i
                next_year = last_date.year
                
                while next_month > 12:
                    next_month -= 12
                    next_year += 1
                
                future_dates.append(pd.Timestamp(year=next_year, month=next_month, day=1))
            
            # Create future dataframe
            future_df = pd.DataFrame({'Date': future_dates})
            
            # Add basic features (this will initialize lag features appropriately)
            future_df = FeatureEngineer.create_features(future_df, resource_type)
            
            # Get historical statistics for better feature filling
            if len(last_data) > 0:
                last_usage_values = last_data['Usage'].tail(24).values  # Last 2 years
                
                # Calculate historical seasonal patterns
                historical_monthly_avg = last_data.groupby(last_data['Date'].dt.month)['Usage'].mean()
                historical_overall_avg = last_data['Usage'].mean()
                
                if historical_overall_avg > 0:
                    seasonal_indexes = historical_monthly_avg / historical_overall_avg
                else:
                    seasonal_indexes = pd.Series(index=range(1, 13), data=1.0)
                
                # Calculate trend from recent data
                if len(last_data) >= 12:
                    recent_trend = (last_data['Usage'].tail(6).mean() - last_data['Usage'].head(6).mean()) / len(last_data)
                else:
                    recent_trend = 0
                
                # Fill future features with meaningful values
                for i, row_idx in enumerate(future_df.index):
                    current_month = future_df.loc[row_idx, 'Month']
                    
                    # Lag features - use most recent values with seasonal adjustment
                    base_seasonal = seasonal_indexes.get(current_month, 1.0)
                    
                    for lag in [1, 2, 3, 6, 12, 24]:
                        if lag <= len(last_usage_values):
                            # Apply seasonal adjustment to historical lag values
                            lag_value = last_usage_values[-lag] * base_seasonal
                            future_df.loc[row_idx, f'Usage_Lag{lag}'] = max(0, lag_value)
                        else:
                            # Use seasonal adjusted average for longer lags
                            avg_value = np.mean(last_usage_values) if len(last_usage_values) > 0 else 0
                            future_df.loc[row_idx, f'Usage_Lag{lag}'] = max(0, avg_value * base_seasonal)
                    
                    # Rolling means with seasonal adjustment
                    for window in [3, 6, 12]:
                        if len(last_usage_values) >= window:
                            rolling_base = np.mean(last_usage_values[-window:])
                            future_df.loc[row_idx, f'RollingMean{window}'] = max(0, rolling_base * base_seasonal)
                            future_df.loc[row_idx, f'RollingStd{window}'] = np.std(last_usage_values[-window:])
                            future_df.loc[row_idx, f'RollingMin{window}'] = np.min(last_usage_values[-window:]) * base_seasonal
                            future_df.loc[row_idx, f'RollingMax{window}'] = np.max(last_usage_values[-window:]) * base_seasonal
                        else:
                            avg_value = np.mean(last_usage_values) if len(last_usage_values) > 0 else 0
                            future_df.loc[row_idx, f'RollingMean{window}'] = max(0, avg_value * base_seasonal)
                            future_df.loc[row_idx, f'RollingStd{window}'] = np.std(last_usage_values) if len(last_usage_values) > 0 else 0
                            future_df.loc[row_idx, f'RollingMin{window}'] = max(0, avg_value * base_seasonal * 0.8)
                            future_df.loc[row_idx, f'RollingMax{window}'] = max(0, avg_value * base_seasonal * 1.2)
                    
                    # Seasonal index
                    future_df.loc[row_idx, 'SeasonalIndex'] = float(base_seasonal)
                    future_df.loc[row_idx, 'SeasonalStrength'] = abs(base_seasonal - 1.0)
                    
                    # Moving averages with trend
                    base_ma = np.mean(last_usage_values[-3:]) if len(last_usage_values) >= 3 else historical_overall_avg
                    future_df.loc[row_idx, 'MA_Short'] = max(0, base_ma * base_seasonal + recent_trend * i)
                    
                    base_ma_long = np.mean(last_usage_values[-12:]) if len(last_usage_values) >= 12 else historical_overall_avg
                    future_df.loc[row_idx, 'MA_Long'] = max(0, base_ma_long * base_seasonal)
                    
                    # Trend indicator
                    if future_df.loc[row_idx, 'MA_Long'] > 0:
                        future_df.loc[row_idx, 'TrendIndicator'] = (future_df.loc[row_idx, 'MA_Short'] - future_df.loc[row_idx, 'MA_Long']) / future_df.loc[row_idx, 'MA_Long']
                    else:
                        future_df.loc[row_idx, 'TrendIndicator'] = 0.0
                    
                    # Change rates (conservative estimates)
                    if len(last_usage_values) >= 2:
                        recent_monthly_change = (last_usage_values[-1] - last_usage_values[-2]) / max(last_usage_values[-2], 1)
                        future_df.loc[row_idx, 'MonthlyChange'] = max(-0.5, min(0.5, recent_monthly_change))  # Cap changes
                    else:
                        future_df.loc[row_idx, 'MonthlyChange'] = 0.0
                    
                    if len(last_usage_values) >= 12:
                        recent_yearly_change = (last_usage_values[-1] - last_usage_values[-12]) / max(last_usage_values[-12], 1)
                        future_df.loc[row_idx, 'YearlyChange'] = max(-0.3, min(0.3, recent_yearly_change))  # Cap changes
                    else:
                        future_df.loc[row_idx, 'YearlyChange'] = 0.0
                    
                    if len(last_usage_values) >= 3:
                        recent_quarterly_change = (last_usage_values[-1] - last_usage_values[-3]) / max(last_usage_values[-3], 1)
                        future_df.loc[row_idx, 'QuarterlyChange'] = max(-0.4, min(0.4, recent_quarterly_change))  # Cap changes
                    else:
                        future_df.loc[row_idx, 'QuarterlyChange'] = 0.0
            
            # Final cleanup - ensure no NaN or infinite values
            future_df = future_df.replace([np.inf, -np.inf], 0)
            future_df = future_df.fillna(0)
            
            # Ensure all values are reasonable (non-negative for certain features)
            usage_related_cols = [col for col in future_df.columns if 'Usage' in col or 'Rolling' in col or 'MA_' in col]
            for col in usage_related_cols:
                if col in future_df.columns:
                    future_df[col] = future_df[col].clip(lower=0)
            
            logger.info(f"Created future features for {len(future_df)} months")
            logger.info(f"Feature sample:\n{future_df.head(3)}")
            
            return future_df
            
        except Exception as e:
            logger.error(f"Error creating future features: {e}")
            raise
    
    @staticmethod
    def predict_with_model(model_path: Path, X_test: pd.DataFrame,
                          model_type: str, scaler: Any = None, 
                          xgb_feature_mapping: Dict = None) -> np.ndarray:
        """Make predictions with a single model"""
        try:
            model, metadata = ModelManager.load_model(model_path)
            
            # Get the feature columns that were used during training
            trained_features = metadata.get('feature_columns', [])
            
            if trained_features:
                # Check for missing features and add them with default values
                missing_features = [col for col in trained_features if col not in X_test.columns]
                if missing_features:
                    logger.warning(f"Missing features for {model_type}: {missing_features}")
                    for col in missing_features:
                        # Add missing features with appropriate default values
                        if 'Lag' in col or 'Rolling' in col or 'MA_' in col:
                            X_test[col] = X_test.get('Usage_Lag1', 0)  # Use lag1 as fallback
                        elif 'Change' in col:
                            X_test[col] = 0.0
                        elif 'Seasonal' in col:
                            X_test[col] = 1.0
                        elif 'Trend' in col:
                            X_test[col] = 0.0
                        elif 'Is' in col:
                            X_test[col] = 0.0
                        elif 'Sin' in col or 'Cos' in col:
                            # Calculate based on Month if available
                            if 'Month' in X_test.columns:
                                if 'Semester' in col:
                                    if 'Sin' in col:
                                        X_test[col] = np.sin(2 * np.pi * X_test['Month'] / 6)
                                    else:
                                        X_test[col] = np.cos(2 * np.pi * X_test['Month'] / 6)
                                else:
                                    X_test[col] = 0.0
                            else:
                                X_test[col] = 0.0
                        elif 'MonthsFromStart' in col:
                            # Calculate based on Year and Month if available
                            if 'Year' in X_test.columns and 'Month' in X_test.columns:
                                min_year = X_test['Year'].min()
                                min_month = X_test['Month'].min()
                                X_test[col] = (X_test['Year'] - min_year) * 12 + X_test['Month'] - min_month
                            else:
                                X_test[col] = 0.0
                        else:
                            X_test[col] = 0.0
                
                # Remove extra features that weren't in training
                extra_features = [col for col in X_test.columns if col not in trained_features]
                if extra_features:
                    logger.warning(f"Removing extra features for {model_type}: {extra_features}")
                    X_test = X_test.drop(columns=extra_features)
                
                # Ensure correct column order
                X_test = X_test[trained_features]
            
            # Ensure features are properly formatted
            X_test = X_test.astype(float)
            X_test = X_test.replace([np.inf, -np.inf], 0)
            X_test = X_test.fillna(0)
            
            # Special handling for XGBoost to avoid feature name issues
            if model_type == 'xgb':
                # Apply feature name mapping if available
                if xgb_feature_mapping:
                    original_cols = X_test.columns.tolist()
                    cleaned_cols = [xgb_feature_mapping.get(col, clean_feature_names([col])[0]) for col in original_cols]
                else:
                    # Clean feature names for XGBoost
                    cleaned_cols = clean_feature_names(X_test.columns.tolist())
                
                X_test_clean = X_test.copy()
                X_test_clean.columns = cleaned_cols
                
                logger.info(f"XGBoost prediction input shape: {X_test_clean.shape}")
                logger.info(f"XGBoost feature sample: {X_test_clean.iloc[0].head()}")
                
                predictions = model.predict(X_test_clean)
                
                # Check for scaling factor in metadata (if used during training)
                scale_factor = metadata.get('scale_factor', 1)
                if scale_factor != 1:
                    predictions = predictions / scale_factor
                    logger.info(f"Scaled XGBoost predictions back by factor {scale_factor}")
                
            else:
                logger.info(f"Using {len(X_test.columns)} features for {model_type} prediction")
                
                if scaler is not None:
                    X_test_scaled = scaler.transform(X_test)
                    predictions = model.predict(X_test_scaled)
                else:
                    predictions = model.predict(X_test)
            
            # Ensure predictions are non-negative
            predictions = np.maximum(predictions, 0)
            
            logger.info(f"Made predictions with {model_type}: min={predictions.min():.2f}, max={predictions.max():.2f}, mean={predictions.mean():.2f}")
            
            return predictions
            
        except Exception as e:
            logger.error(f"Error making predictions with {model_type}: {e}")
            raise
        
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
        
        logger.info(f"Model types to train: {request.model_types}")
        logger.info(f"Ensemble types to train: {request.ensemble_types}")
        
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
        
        logger.info(f"Created {len(feature_cols)} features: {feature_cols}")
        
        # Improved data splitting strategy
        if len(df) >= 36:  # 3+ years of data
            # Use last 12 months for testing
            train_data = df.iloc[:-12].copy()
            test_data = df.iloc[-12:].copy()
        elif len(df) >= 24:  # 2+ years of data
            # Use last 6 months for testing
            train_data = df.iloc[:-6].copy()
            test_data = df.iloc[-6:].copy()
        else:
            # Use 20% for testing but ensure at least 1 record
            test_size = max(1, min(6, len(df) // 5))
            train_data = df.iloc[:-test_size].copy()
            test_data = df.iloc[-test_size:].copy()
        
        X_train = train_data[feature_cols]
        y_train = train_data['Usage']
        X_test = test_data[feature_cols]
        y_test = test_data['Usage']
        
        logger.info(f"Training data: {len(train_data)} records")
        logger.info(f"Test data: {len(test_data)} records")
        logger.info(f"Features shape: {X_train.shape}")
        
        # Train models
        trainer = ModelTrainer()
        models_trained = []
        all_metrics = {}
        trained_models = {}
        
        # Train individual models
        for model_type in request.model_types:
            if model_type in MODEL_TYPES:
                try:
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
                        'feature_columns': feature_cols,
                        'train_size': len(train_data),
                        'test_size': len(test_data)
                    }
                    
                    # Also save scaler if it exists
                    if model_type in trainer.scalers:
                        scaler_path = model_path.parent / f"{model_type}_scaler.pkl"
                        scaler_path.parent.mkdir(parents=True, exist_ok=True)
                        with open(scaler_path, 'wb') as f:
                            pickle.dump(trainer.scalers[model_type], f)
                        metadata['has_scaler'] = True
                    
                    ModelManager.save_model(model, model_path, metadata)
                    models_trained.append(model_type)
                    all_metrics[model_type] = metrics
                    trained_models[model_type] = model
                    
                    logger.info(f"Successfully trained {model_type} model with R2: {metrics['R2']:.3f}")
                    
                except Exception as e:
                    logger.error(f"Failed to train {model_type} model: {e}")
                    continue
        
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
                        'feature_columns': feature_cols,
                        'train_size': len(train_data),
                        'test_size': len(test_data)
                    }
                    
                    # Save ensemble metadata (no actual model file needed for ensemble)
                    ensemble_path.parent.mkdir(parents=True, exist_ok=True)
                    metadata_path = ensemble_path.parent / f'{ensemble_type}_metadata.json'
                    with open(metadata_path, 'w') as f:
                        json.dump(safe_dict_conversion(ensemble_metadata), f, indent=2, default=str)
                    
                    models_trained.append(ensemble_type)
                    all_metrics[ensemble_type] = ensemble_metrics
                    
                    logger.info(f"Successfully created {ensemble_type} ensemble with R2: {ensemble_metrics['R2']:.3f}")
                    
                except Exception as e:
                    logger.warning(f"Failed to create {ensemble_type} ensemble: {e}")
        
        if not models_trained:
            raise HTTPException(status_code=500, detail="No models were successfully trained")
        
        return TrainResponse(
            success=True,
            message=f"Successfully trained {len(models_trained)} models",
            models_trained=models_trained,
            metrics=safe_dict_conversion(all_metrics),
            data_info=safe_dict_conversion({
                'total_records': len(df),
                'training_records': len(train_data),
                'test_records': len(test_data),
                'features_count': len(feature_cols),
                'date_range': f"{df['Date'].min()} to {df['Date'].max()}"
            })
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
        
        # Create future features
        future_df = Predictor.create_future_features(df, request.months_ahead, request.resource_type)
        
        # Handle feature compatibility for existing models
        if request.model_type in ENSEMBLE_TYPES:
            # For ensembles, check component model features
            metadata_path = model_path.parent / f'{request.model_type}_metadata.json'
            with open(metadata_path, 'r') as f:
                ensemble_metadata = json.load(f)
            
            # Use features from the first available component model
            component_models = ensemble_metadata['ensemble_components']
            trained_features = ensemble_metadata.get('feature_columns', [])
            
            if not trained_features:
                # Fallback: try to get features from a component model
                for component in component_models:
                    component_path = ModelManager.get_model_path(
                        request.resource_type, request.building_id, component
                    )
                    if component_path.exists():
                        _, comp_metadata = ModelManager.load_model(component_path)
                        trained_features = comp_metadata.get('feature_columns', [])
                        if trained_features:
                            break
        else:
            # For individual models, get features from metadata
            _, metadata = ModelManager.load_model(model_path)
            trained_features = metadata.get('feature_columns', [])
        
        # If no trained features found, use legacy feature set
        if not trained_features:
            logger.warning("No trained features found in metadata, using legacy feature set")
            trained_features = FeatureEngineer.get_legacy_features(request.resource_type)
            # Filter to only features that exist in the future dataframe
            trained_features = [f for f in trained_features if f in future_df.columns]
        
        # Prepare features for prediction
        X_future = future_df[trained_features].copy() if all(col in future_df.columns for col in trained_features) else future_df
        
        logger.info(f"Using {len(trained_features)} features for prediction: {trained_features[:10]}...")
        
        # Verify feature consistency and cleanup
        missing_features = [col for col in trained_features if col not in X_future.columns]
        if missing_features:
            logger.warning(f"Missing features in future data: {missing_features}")
            for col in missing_features:
                X_future[col] = 0
        
        # Remove extra features not in training
        extra_features = [col for col in X_future.columns if col not in trained_features]
        if extra_features:
            logger.warning(f"Removing extra features: {extra_features}")
            X_future = X_future.drop(columns=extra_features)
        
        # Ensure correct column order
        X_future = X_future[trained_features]
        
        # Final verification and cleanup
        X_future = X_future.replace([np.inf, -np.inf], 0)
        X_future = X_future.fillna(0)
        X_future = X_future.astype(float)
        
        logger.info(f"Future features shape: {X_future.shape}")
        logger.info(f"Features being used: {list(X_future.columns)}")

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
                    logger.info(f"Component {component} predictions: {component_pred[:3]}...")
            
            if not ensemble_predictions:
                raise HTTPException(
                    status_code=404,
                    detail=f"No component models found for ensemble {request.model_type}"
                )
            
            # Weighted average ensemble predictions
            weights = [0.4, 0.3, 0.3] if len(ensemble_predictions) == 3 else [0.5, 0.5]
            weights = weights[:len(ensemble_predictions)]
            weights = np.array(weights) / np.sum(weights)
            
            final_predictions = np.average(ensemble_predictions, axis=0, weights=weights)
            model_info = ensemble_metadata
            
            logger.info(f"Ensemble predictions: {final_predictions[:3]}...")
            
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
        
        # Ensure all predictions are reasonable
        final_predictions = np.maximum(final_predictions, 0)  # No negative consumption
        
        # Format predictions with safe conversion
        for i, pred in enumerate(final_predictions):
            future_date = future_df.iloc[i]['Date']
            predictions.append({
                'date': future_date.strftime('%Y-%m'),
                'predicted_usage': safe_float_conversion(pred),
                'month': int(future_date.month),
                'year': int(future_date.year)
            })
        
        logger.info(f"Generated {len(predictions)} predictions successfully")
        
        return PredictResponse(
            success=True,
            predictions=predictions,
            model_info=safe_dict_conversion({
                'model_type': request.model_type,
                'resource_type': request.resource_type,
                'building_id': request.building_id,
                'trained_at': model_info.get('trained_at', 'Unknown'),
                'metrics': model_info.get('metrics', {}),
                'months_predicted': request.months_ahead,
                'feature_count': len(trained_features),
                'features_used': trained_features[:10]  # Show first 10 features
            })
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
                                metrics=safe_dict_conversion(metadata.get('metrics', {})),
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
                                metrics=safe_dict_conversion(metadata.get('metrics', {})),
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
                    'metrics': safe_dict_conversion(metadata.get('metrics', {})),
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
                    'metrics': safe_dict_conversion(metadata.get('metrics', {})),
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
        
        # Safe conversion for response
        usage_stats = {
            'min': safe_float_conversion(df['Usage'].min()),
            'max': safe_float_conversion(df['Usage'].max()),
            'mean': safe_float_conversion(df['Usage'].mean()),
            'std': safe_float_conversion(df['Usage'].std())
        }
        
        # Create monthly data safely
        monthly_data = {}
        for date, usage in df.groupby(df['Date'].dt.strftime('%Y-%m'))['Usage'].sum().items():
            monthly_data[date] = safe_float_conversion(usage)
        
        return {
            'resource_type': resource_type,
            'building_id': building_id,
            'total_records': len(df),
            'date_range': {
                'start': df['Date'].min().strftime('%Y-%m-%d'),
                'end': df['Date'].max().strftime('%Y-%m-%d')
            },
            'usage_stats': usage_stats,
            'sufficient_for_training': len(df) >= 13,
            'monthly_data': monthly_data
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
                COUNT(DISTINCT YEAR(`Date`), MONTH(`Date`)) as monthly_records,
                SUM(CASE WHEN `Usage` > 0 THEN 1 ELSE 0 END) as non_zero_records,
                AVG(CASE WHEN `Usage` > 0 THEN `Usage` ELSE NULL END) as avg_usage,
                MIN(`Date`) as min_date,
                MAX(`Date`) as max_date
            FROM {table_name}
            WHERE `Usage` > 0 AND `Date` IS NOT NULL
            GROUP BY BuildingId
            HAVING COUNT(DISTINCT YEAR(`Date`), MONTH(`Date`)) >= 13
            ORDER BY COUNT(DISTINCT YEAR(`Date`), MONTH(`Date`)) DESC
        """
        cursor.execute(query)
        buildings = cursor.fetchall()
        
        # Safe conversion for buildings data
        safe_buildings = []
        for building in buildings:
            safe_building = {}
            for key, value in building.items():
                if isinstance(value, (int, float)):
                    safe_building[key] = safe_float_conversion(value)
                else:
                    safe_building[key] = value
            safe_buildings.append(safe_building)
        
        return {
            'resource_type': resource_type,
            'available_buildings': safe_buildings,
            'total_buildings': len(safe_buildings)
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
