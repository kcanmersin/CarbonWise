import React, { useState, useEffect } from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import Sidebar from '../components/Sidebar';
import {  getBuildings } from '../services/buildingService';
import { getHealth, predict, getBuildingModels } from '../services/aiService';

const PredictionsPage = () => {
    // State management
    const [resourceType, setResourceType] = useState('');
    const [buildingId, setBuildingId] = useState('');
    const [modelType, setModelType] = useState('');
    const [monthsAhead, setMonthsAhead] = useState(12);
    const [buildings, setBuildings] = useState([]);
    const [health, setHealth] = useState(null);
    const [predictionData, setPredictionData] = useState([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState('');
    const [availableModels, setAvailableModels] = useState([]);
    const [isLoadingModels, setIsLoadingModels] = useState(false);

    // Resource types
    const resourceTypes = [
        { value: 'electricity', label: 'Electricity', icon: '⚡' },
        { value: 'water', label: 'Water', icon: '💧' },
        { value: 'naturalgas', label: 'Natural Gas', icon: '🔥' },
        { value: 'paper', label: 'Paper', icon: '📄' }
    ];

    // Fetch initial data
    useEffect(() => {
        const fetchInitialData = async () => {
            try {
                const [buildingsData, healthData] = await Promise.all([
                    getBuildings(),
                    getHealth()
                ]);

                // Add "All Buildings" option at the beginning
                const buildingsWithAll = [
                    { id: '0', name: 'All Buildings' },
                    ...buildingsData
                ];

                setBuildings(buildingsWithAll);
                setHealth(healthData);
            } catch (err) {
                setError(`Failed to load initial data: ${err.message}`);
            }
        };

        fetchInitialData();
    }, []);

    // Fetch available models when resource type and building change
    useEffect(() => {
        const fetchModels = async () => {
            if (!resourceType || !buildingId) {
                setAvailableModels([]);
                setModelType(''); // Reset selected model
                return;
            }

            setIsLoadingModels(true);
            try {
                const actualBuildingId = buildingId === '0' ? '0' : buildingId;
                const response = await getBuildingModels(resourceType, actualBuildingId);
                
                // Extract model types from the response
                const modelTypes = response.models.map(model => model.model_type);
                
                // Transform to dropdown format
                const labelMap = {
                    'rf': 'Random Forest (RF)',
                    'xgb': 'XGBoost (XGB)', 
                    'gb': 'Gradient Boosting (GB)',
                    'rf_gb_xgb': 'RF + GB + XGB Ensemble'
                };
                
                const modelOptions = modelTypes.map(modelType => ({
                    value: modelType,
                    label: labelMap[modelType] || modelType
                }));
                
                setAvailableModels(modelOptions);
                setModelType(''); // Reset selected model when models change
                console.log(`Found ${response.total_models} models for ${resourceType} in building ${buildingId}`);
            } catch (err) {
                console.error('Error fetching models:', err);
                // When 404 or other error, show empty array (no models found)
                setAvailableModels([]);
                setModelType('');
            } finally {
                setIsLoadingModels(false);
            }
        };

        fetchModels();
    }, [resourceType, buildingId]);

    // Handle prediction
    const handlePredict = async () => {
        if (!resourceType || !buildingId || !modelType) {
            setError('Please select all required fields');
            return;
        }

        setIsLoading(true);
        setError('');

        try {
            // Determine the actual building ID to send to API
            const actualBuildingId = buildingId === '0' ? '0' : buildingId;
            
            // Generate prediction
            console.log('Generating prediction...');

            console.log(`Resource Type: ${resourceType}, Building ID: ${actualBuildingId}, Model Type: ${modelType}, Months Ahead: ${monthsAhead}`);
            const result = await predict(resourceType, actualBuildingId, modelType, monthsAhead);

            console.log('Prediction result:', result);
            
            // Transform data for chart  
            const chartData = result.predictions?.map((prediction) => ({
                month: `${prediction.year}-${String(prediction.month).padStart(2, '0')}`, // "2024-10"
                predicted: prediction.predicted_usage,
                fullDate: prediction.date
            })) || [];

            setPredictionData(chartData);
        } catch (err) {
            setError(`Prediction failed: ${err.message}`);
            console.error('Prediction error:', err);
        } finally {
            setIsLoading(false);
        }
    };

    // Menu items for sidebar
    const menuItems = [
        { key: "dashboard", name: "Dashboard" },
        {
            key: "resource-monitoring",
            name: "Resource Monitoring",
            subItems: [
                { key: "electricity", name: "Electricity" },
                { key: "water", name: "Water" },
                { key: "paper", name: "Paper" },
                { key: "naturalGas", name: "Natural Gas" }
            ]
        },
        {
            key: "carbon-footprint",
            name: "Carbon Footprint",
            subItems: [
                { key: "test", name: "Test" },
                { key: "calculations", name: "Calculations" }
            ]
        },
        { key: "predictions", name: "Predictions" },
        { key: "userManagement", name: "User Management" },
        { key: "adminTools", name: "Admin Tools" },
        { key: "reports", name: "Reports" }
    ];

    return (
        <div style={{ display: "flex", height: "100vh" }}>
            <Sidebar menuItems={menuItems} />
            <div style={{ flex: 1, padding: "2rem", backgroundColor: "#f5f7fa", overflowY: "auto" }}>
                <div style={{ maxWidth: "1400px", margin: "0 auto" }}>
                {/* Header */}
                <div style={{ marginBottom: "2rem" }}>
                    <div style={{ display: "flex", alignItems: "center", marginBottom: "0.5rem" }}>
                        <span style={{ fontSize: "2rem", marginRight: "1rem" }}>🔮</span>
                        <h1 style={{ color: "#2c3e50", margin: 0, fontSize: "2.5rem", fontWeight: "bold" }}>
                            AI Prediction Dashboard
                        </h1>
                    </div>
                    <p style={{ color: "#7f8c8d", margin: 0, fontSize: "1.1rem" }}>
                        Generate accurate forecasts for resource consumption using advanced machine learning models
                    </p>
                    <div style={{ 
                        display: "flex", 
                        alignItems: "center", 
                        marginTop: "1rem",
                        gap: "0.5rem"
                    }}>
                        <div style={{
                            width: "12px",
                            height: "12px",
                            borderRadius: "50%",
                            backgroundColor: health?.status === 'healthy' ? "#27ae60" : "#e74c3c"
                        }}></div>
                        <span style={{ color: "#7f8c8d", fontSize: "0.9rem" }}>
                            AI Service: {health?.status || 'Unknown'}
                        </span>
                    </div>
                </div>

                {/* Controls */}
                <div style={{
                    backgroundColor: "white",
                    padding: "2rem",
                    borderRadius: "12px",
                    boxShadow: "0 4px 6px rgba(0,0,0,0.1)",
                    marginBottom: "2rem"
                }}>
                    <h2 style={{ 
                        color: "#2c3e50", 
                        marginBottom: "1.5rem",
                        fontSize: "1.5rem",
                        fontWeight: "600"
                    }}>
                        Prediction Parameters
                    </h2>
                    
                    <div style={{
                        display: "grid",
                        gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))",
                        gap: "1.5rem",
                        marginBottom: "2rem"
                    }}>
                        {/* Resource Type */}
                        <div>
                            <label style={{
                                display: "block",
                                fontSize: "0.9rem",
                                fontWeight: "600",
                                color: "#495057",
                                marginBottom: "0.5rem"
                            }}>
                                Resource Type
                            </label>
                            <select
                                value={resourceType}
                                onChange={(e) => setResourceType(e.target.value)}
                                style={{
                                    width: "100%",
                                    padding: "0.75rem 1rem",
                                    border: "2px solid #e9ecef",
                                    borderRadius: "8px",
                                    fontSize: "1rem",
                                    outline: "none",
                                    backgroundColor: "white",
                                    cursor: "pointer"
                                }}
                                onFocus={(e) => e.target.style.borderColor = "#3498db"}
                                onBlur={(e) => e.target.style.borderColor = "#e9ecef"}
                            >
                                <option value="">Select Resource</option>
                                {resourceTypes.map((type) => (
                                    <option key={type.value} value={type.value}>
                                        {type.icon} {type.label}
                                    </option>
                                ))}
                            </select>
                        </div>

                        {/* Building */}
                        <div>
                            <label style={{
                                display: "block",
                                fontSize: "0.9rem",
                                fontWeight: "600",
                                color: "#495057",
                                marginBottom: "0.5rem"
                            }}>
                                Building
                            </label>
                            <select
                                value={buildingId}
                                onChange={(e) => setBuildingId(e.target.value)}
                                style={{
                                    width: "100%",
                                    padding: "0.75rem 1rem",
                                    border: "2px solid #e9ecef",
                                    borderRadius: "8px",
                                    fontSize: "1rem",
                                    outline: "none",
                                    backgroundColor: "white",
                                    cursor: "pointer"
                                }}
                                onFocus={(e) => e.target.style.borderColor = "#3498db"}
                                onBlur={(e) => e.target.style.borderColor = "#e9ecef"}
                            >
                                <option value="">Select Building</option>
                                {buildings.map((building) => (
                                    <option key={building.id || building.value} value={building.id || building.value}>
                                        🏢 {building.name || building.label}
                                    </option>
                                ))}
                            </select>
                        </div>

                        {/* Model Type */}
                        <div>
                            <label style={{
                                display: "block",
                                fontSize: "0.9rem",
                                fontWeight: "600",
                                color: "#495057",
                                marginBottom: "0.5rem"
                            }}>
                                Model Type
                            </label>
                            <select
                                value={modelType}
                                onChange={(e) => setModelType(e.target.value)}
                                disabled={isLoadingModels || !resourceType || !buildingId || availableModels.length === 0}
                                style={{
                                    width: "100%",
                                    padding: "0.75rem 1rem",
                                    border: "2px solid #e9ecef",
                                    borderRadius: "8px",
                                    fontSize: "1rem",
                                    outline: "none",
                                    backgroundColor: isLoadingModels || availableModels.length === 0 ? "#f8f9fa" : "white",
                                    cursor: isLoadingModels || !resourceType || !buildingId || availableModels.length === 0 ? "not-allowed" : "pointer",
                                    opacity: isLoadingModels || availableModels.length === 0 ? 0.7 : 1
                                }}
                                onFocus={(e) => e.target.style.borderColor = "#3498db"}
                                onBlur={(e) => e.target.style.borderColor = "#e9ecef"}
                            >
                                <option value="">
                                    {isLoadingModels ? "Loading models..." : 
                                    !resourceType || !buildingId ? "Select resource & building first" :
                                    availableModels.length === 0 ? "No trained models found" :
                                    "Select Model"}
                                </option>
                                {availableModels.map((model) => (
                                    <option key={model.value} value={model.value}>
                                        🤖 {model.label}
                                    </option>
                                ))}
                            </select>
                            {!isLoadingModels && resourceType && buildingId && availableModels.length === 0 && (
                                <small style={{ color: "#e74c3c", fontSize: "0.8rem" }}>
                                    ⚠️ No trained models available for this combination.
                                </small>
                            )}
                            {isLoadingModels && (
                                <small style={{ color: "#7f8c8d", fontSize: "0.8rem" }}>
                                    🔄 Loading available models...
                                </small>
                            )}
                        </div>

                        {/* Months Ahead */}
                        <div>
                            <label style={{
                                display: "block",
                                fontSize: "0.9rem",
                                fontWeight: "600",
                                color: "#495057",
                                marginBottom: "0.5rem"
                            }}>
                                Months Ahead
                            </label>
                            <input
                                type="number"
                                value={monthsAhead}
                                onChange={(e) => setMonthsAhead(parseInt(e.target.value))}
                                min="1"
                                max="36"
                                style={{
                                    width: "100%",
                                    padding: "0.75rem 1rem",
                                    border: "2px solid #e9ecef",
                                    borderRadius: "8px",
                                    fontSize: "1rem",
                                    outline: "none"
                                }}
                                onFocus={(e) => e.target.style.borderColor = "#3498db"}
                                onBlur={(e) => e.target.style.borderColor = "#e9ecef"}
                            />
                        </div>
                    </div>

                    {/* Action Button */}
                    <div style={{ display: "flex", justifyContent: "center" }}>
                        <button
                            onClick={handlePredict}
                            disabled={isLoading || !resourceType || !buildingId || !modelType || availableModels.length === 0}
                            style={{
                                display: "flex",
                                alignItems: "center",
                                gap: "0.5rem",
                                padding: "1rem 2rem",
                                background: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
                                color: "white",
                                border: "none",
                                borderRadius: "8px",
                                fontSize: "1rem",
                                fontWeight: "600",
                                cursor: isLoading || !resourceType || !buildingId || !modelType ? "not-allowed" : "pointer",
                                opacity: isLoading || !resourceType || !buildingId || !modelType || availableModels.length === 0 ? 0.6 : 1,
                                transition: "all 0.3s ease",
                                boxShadow: "0 4px 15px rgba(102, 126, 234, 0.3)"
                            }}
                        >
                            {isLoading ? (
                                <>
                                    <div style={{
                                        width: "20px",
                                        height: "20px",
                                        border: "2px solid rgba(255,255,255,0.3)",
                                        borderTop: "2px solid white",
                                        borderRadius: "50%",
                                        animation: "spin 1s linear infinite"
                                    }}></div>
                                    Generating Prediction...
                                </>
                            ) : (
                                <>
                                    <span style={{ fontSize: "1.2rem" }}>🔮</span>
                                    Generate Prediction
                                </>
                            )}
                        </button>
                    </div>

                    {/* CSS Animation */}
                    <style>
                        {`
                            @keyframes spin {
                                0% { transform: rotate(0deg); }
                                100% { transform: rotate(360deg); }
                            }
                        `}
                    </style>

                    {/* Error Message */}
                    {error && (
                        <div style={{
                            marginTop: "1.5rem",
                            padding: "1rem 1.5rem",
                            backgroundColor: "#fff5f5",
                            border: "1px solid #feb2b2",
                            borderRadius: "8px",
                            display: "flex",
                            alignItems: "center",
                            gap: "0.75rem"
                        }}>
                            <span style={{ fontSize: "1.2rem" }}>⚠️</span>
                            <span style={{ color: "#c53030", fontWeight: "500" }}>{error}</span>
                        </div>
                    )}
                </div>

                {/* Chart */}
                {predictionData.length > 0 && (
                    <div style={{
                        backgroundColor: "white",
                        borderRadius: "12px",
                        boxShadow: "0 4px 6px rgba(0,0,0,0.1)",
                        padding: "2rem"
                    }}>
                        <h2 style={{
                            color: "#2c3e50",
                            marginBottom: "1.5rem",
                            fontSize: "1.5rem",
                            fontWeight: "600"
                        }}>
                            📊 Prediction Results: {resourceType} - {
                                buildingId === '0' 
                                    ? 'All Buildings' 
                                    : buildings.find(b => (b.id || b.value) === buildingId)?.name || buildings.find(b => (b.id || b.value) === buildingId)?.label
                            }
                        </h2>
                        
                        <ResponsiveContainer width="100%" height={450}>
                            <LineChart data={predictionData} margin={{ top: 20, right: 30, left: 20, bottom: 20 }}>
                                <CartesianGrid strokeDasharray="3 3" stroke="#f1f3f4" />
                                <XAxis 
                                    dataKey="month" 
                                    stroke="#7f8c8d"
                                    fontSize={12}
                                    fontWeight="500"
                                />
                                <YAxis 
                                    stroke="#7f8c8d"
                                    fontSize={12}
                                    fontWeight="500"
                                />
                                <Tooltip 
                                    contentStyle={{
                                        backgroundColor: "white",
                                        border: "1px solid #e9ecef",
                                        borderRadius: "8px",
                                        boxShadow: "0 4px 15px rgba(0,0,0,0.1)",
                                        color: "#2c3e50",
                                        fontWeight: "500"
                                    }}
                                    labelStyle={{ color: "#2c3e50", fontWeight: "600" }}
                                />
                                <Legend 
                                    wrapperStyle={{ paddingTop: "20px" }}
                                    iconType="line"
                                />
                                <Line 
                                    type="monotone" 
                                    dataKey="predicted" 
                                    stroke="#667eea" 
                                    strokeWidth={3}
                                    name="AI Prediction"
                                    dot={{ fill: "#667eea", strokeWidth: 2, r: 4 }}
                                />
                            </LineChart>
                        </ResponsiveContainer>
                        
                        <div style={{
                            marginTop: "1.5rem",
                            padding: "1rem 1.5rem",
                            background: "linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%)",
                            borderRadius: "8px",
                            border: "1px solid #dee2e6"
                        }}>
                            <p style={{ 
                                color: "#495057", 
                                margin: 0, 
                                fontSize: "0.9rem",
                                fontWeight: "500"
                            }}>
                                <strong>Model:</strong> {availableModels.find(m => m.value === modelType)?.label || modelType} |
                                <strong> Resource:</strong> {resourceType} | 
                                <strong> Building:</strong> {
                                    buildingId === '0' 
                                        ? 'All Buildings' 
                                        : buildings.find(b => (b.id || b.value) === buildingId)?.name || buildings.find(b => (b.id || b.value) === buildingId)?.label
                                } | 
                                <strong> Forecast Period:</strong> {monthsAhead} months
                            </p>
                        </div>
                    </div>
                )}

                {/* Empty State */}
                {predictionData.length === 0 && !isLoading && (
                    <div style={{
                        backgroundColor: "white",
                        borderRadius: "12px",
                        boxShadow: "0 4px 6px rgba(0,0,0,0.1)",
                        padding: "3rem",
                        textAlign: "center"
                    }}>
                        <div style={{ fontSize: "4rem", marginBottom: "1rem" }}>🔮</div>
                        <h3 style={{ 
                            color: "#2c3e50", 
                            marginBottom: "1rem",
                            fontSize: "1.4rem",
                            fontWeight: "600"
                        }}>
                            Ready to Generate Predictions
                        </h3>
                        <p style={{ 
                            color: "#7f8c8d",
                            margin: 0,
                            fontSize: "1rem",
                            lineHeight: "1.6"
                        }}>
                            Select your parameters above and click "Generate Prediction" to see AI-powered forecasting results.<br/>
                            Our advanced machine learning models will provide accurate consumption predictions.<br/>
                            <strong>Note:</strong> Select "All Buildings" to get aggregate predictions across all buildings.
                        </p>
                    </div>
                )}
            </div>
        </div>
            </div>
    );
};

export default PredictionsPage;