import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import Sidebar from "../components/Sidebar";
import { 
  XAxis, YAxis, CartesianGrid, 
  Tooltip, Legend, ResponsiveContainer, AreaChart, Area,
  BarChart, Bar, PieChart, Pie, Cell
} from 'recharts';
import { getMonthlyAirQuality } from "../services/dashboardService";
import { getStats } from "../services/carbonFootprintTestService";

export default function Dashboard({ user, onLogout }) {
  const navigate = useNavigate();
  const [city, setCity] = useState("Kocaeli Gebze, Turkey");
  const [cityInput, setCityInput] = useState(city);
  const [monthlyData, setMonthlyData] = useState([]);
  const [isLoadingMonthly, setIsLoadingMonthly] = useState(false);
  const [monthlyError, setMonthlyError] = useState(null);
  
  // New states for sustainability stats
  const [stats, setStats] = useState(null);
  const [isLoadingStats, setIsLoadingStats] = useState(false);
  const [statsError, setStatsError] = useState(null);

  // Air quality color scale based on AQI value
  const getAqiColor = (aqi) => {
    if (aqi <= 50) return { bg: "#a8e05f", text: "#2a4d00", level: "Good" };
    if (aqi <= 100) return { bg: "#fdd74b", text: "#8d5e00", level: "Moderate" };
    if (aqi <= 150) return { bg: "#fe9b57", text: "#9c3400", level: "Unhealthy for Sensitive Groups" };
    if (aqi <= 200) return { bg: "#fe6a69", text: "#a11c1c", level: "Unhealthy" };
    if (aqi <= 300) return { bg: "#a97abc", text: "#4f266e", level: "Very Unhealthy" };
    return { bg: "#a87383", text: "#591035", level: "Hazardous" };
  };

  // Fetch sustainability stats
  useEffect(() => {
    const fetchStats = async () => {
      setIsLoadingStats(true);
      setStatsError(null);
      try {
        const data = await getStats();
        console.log("Stats data:", data);
        setStats(data);
      } catch (error) {
        console.error("Error fetching sustainability stats:", error);
        setStatsError("Failed to load sustainability statistics. Please try again.");
      } finally {
        setIsLoadingStats(false);
      }
    };

    fetchStats();
  }, []);

  // Fetch monthly air quality data
  useEffect(() => {
    const fetchMonthlyAirQuality = async () => {
      setIsLoadingMonthly(true);
      setMonthlyError(null);
      try {
        const data = await getMonthlyAirQuality(city);
        console.log("Monthly air quality data:", data);
        
        // Process the data for the chart - using actual API response structure
        const processedData = data.map(item => ({
          date: new Date(item.recordDate).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }),
          fullDate: item.recordDate,
          aqi: item.aqi,
          pm25: item.pM25 || 0,
          pm10: item.pM10 || 0,
          no2: item.nO2 || 0,
          so2: item.sO2 || 0,
          co: item.co || 0,
          ozone: item.ozone || 0,
          temperature: item.temperature || 0,
          humidity: item.humidity || 0,
          pressure: item.pressure || 0,
          windSpeed: item.windSpeed || 0,
          dominentpol: item.dominentPollutant,
          city: item.city
        })).sort((a, b) => new Date(a.fullDate) - new Date(b.fullDate));
        
        setMonthlyData(processedData);
      } catch (error) {
        console.error("Error fetching monthly air quality data:", error);
        setMonthlyError("Failed to load monthly air quality data.");
      } finally {
        setIsLoadingMonthly(false);
      }
    };

    fetchMonthlyAirQuality();
  }, [city]);

  // Handle city search
  const handleCitySearch = (e) => {
    e.preventDefault();
    if (cityInput.trim()) {
      setCity(cityInput.trim());
    }
  };

  // Menu items
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

  // Custom tooltip for the air quality chart
  const CustomTooltip = ({ active, payload, label }) => {
    if (active && payload && payload.length) {
      const data = payload[0].payload;
      const aqiInfo = getAqiColor(data.aqi);
      
      return (
        <div style={{
          backgroundColor: "white",
          padding: "1rem",
          border: "1px solid #ddd",
          borderRadius: "8px",
          boxShadow: "0 4px 6px rgba(0,0,0,0.1)",
          minWidth: "250px"
        }}>
          <p style={{ margin: "0 0 0.5rem 0", fontWeight: "bold" }}>{label}</p>
          <div style={{
            padding: "0.5rem",
            backgroundColor: aqiInfo.bg,
            color: aqiInfo.text,
            borderRadius: "4px",
            marginBottom: "0.5rem"
          }}>
            <p style={{ margin: 0, fontWeight: "bold" }}>AQI: {data.aqi} ({aqiInfo.level})</p>
          </div>
          <div style={{ fontSize: "0.9rem" }}>
            <p style={{ margin: "0.25rem 0" }}>🏭 Dominant: {data.dominentpol?.toUpperCase()}</p>
            <p style={{ margin: "0.25rem 0" }}>🌫️ PM2.5: {data.pm25} µg/m³</p>
            <p style={{ margin: "0.25rem 0" }}>💨 PM10: {data.pm10} µg/m³</p>
            <p style={{ margin: "0.25rem 0" }}>🧪 SO2: {data.so2} µg/m³</p>
            <p style={{ margin: "0.25rem 0" }}>🏭 NO2: {data.no2} µg/m³</p>
            <p style={{ margin: "0.25rem 0" }}>🚗 CO: {data.co} µg/m³</p>
            {data.ozone > 0 && <p style={{ margin: "0.25rem 0" }}>☀️ Ozone: {data.ozone} µg/m³</p>}
            <hr style={{ margin: "0.5rem 0", border: "none", borderTop: "1px solid #eee" }} />
            <p style={{ margin: "0.25rem 0" }}>🌡️ Temperature: {data.temperature}°C</p>
            <p style={{ margin: "0.25rem 0" }}>💧 Humidity: {data.humidity}%</p>
            <p style={{ margin: "0.25rem 0" }}>💨 Wind: {data.windSpeed} m/s</p>
            <p style={{ margin: "0.25rem 0" }}>📊 Pressure: {data.pressure} hPa</p>
          </div>
        </div>
      );
    }
    return null;
  };

  // Render sustainability stats section
  const renderSustainabilityStats = () => {
    if (isLoadingStats) {
      return (
        <div style={{ 
          textAlign: "center", 
          padding: "3rem",
          backgroundColor: "white",
          borderRadius: "12px",
          boxShadow: "0 4px 6px rgba(0,0,0,0.1)"
        }}>
          <div style={{
            display: "inline-block",
            width: "50px",
            height: "50px",
            border: "5px solid #f3f3f3",
            borderTop: "5px solid #27ae60",
            borderRadius: "50%",
            animation: "spin 1s linear infinite"
          }}></div>
          <style>
            {`
              @keyframes spin {
                0% { transform: rotate(0deg); }
                100% { transform: rotate(360deg); }
              }
            `}
          </style>
          <p style={{ marginTop: "1rem", fontSize: "1.1rem", color: "#7f8c8d" }}>Loading sustainability statistics...</p>
        </div>
      );
    }

    if (statsError) {
      return (
        <div style={{
          backgroundColor: "#fff5f5",
          border: "1px solid #feb2b2",
          color: "#c53030",
          padding: "2rem",
          borderRadius: "12px",
          textAlign: "center"
        }}>
          <div style={{ fontSize: "2rem", marginBottom: "1rem" }}>⚠️</div>
          <strong>Error:</strong> {statsError}
          <button 
            onClick={() => window.location.reload()} 
            style={{
              marginTop: "1rem",
              padding: "0.5rem 1rem",
              backgroundColor: "#c53030",
              color: "white",
              border: "none",
              borderRadius: "6px",
              cursor: "pointer",
              display: "block",
              margin: "1rem auto 0"
            }}
          >
            Retry
          </button>
        </div>
      );
    }

    if (!stats) return null;

    // Prepare data for charts
    const userDistributionData = [
      { 
        name: "With Sustainability Points", 
        value: stats.usersWithSustainabilityPoints, 
        color: "#27ae60",
        percentage: ((stats.usersWithSustainabilityPoints / stats.totalUsers) * 100).toFixed(1)
      },
      { 
        name: "Without Sustainability Points", 
        value: stats.usersWithoutSustainabilityPoints, 
        color: "#e74c3c",
        percentage: ((stats.usersWithoutSustainabilityPoints / stats.totalUsers) * 100).toFixed(1)
      }
    ];

    const pointsComparisonData = [
      { 
        category: "Average", 
        value: parseFloat(stats.averageSustainabilityPoints.toFixed(1)),
        color: "#3498db"
      },
      { 
        category: "Highest", 
        value: stats.highestSustainabilityPoints,
        color: "#27ae60"
      },
      { 
        category: "Lowest", 
        value: stats.lowestSustainabilityPoints,
        color: "#95a5a6"
      }
    ];

    return (
      <div style={{
        backgroundColor: "white",
        padding: "2rem",
        borderRadius: "16px",
        boxShadow: "0 8px 25px rgba(0,0,0,0.1)",
        marginBottom: "2rem"
      }}>
        {/* Header */}
        <div style={{
          textAlign: "center",
          marginBottom: "2rem"
        }}>
          <h2 style={{ 
            margin: "0 0 0.5rem 0", 
            color: "#2c3e50",
            fontSize: "2rem",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            gap: "0.5rem"
          }}>
            🌱 Sustainability Analytics Dashboard
          </h2>
          <p style={{ 
            color: "#7f8c8d", 
            margin: 0,
            fontSize: "1.1rem"
          }}>
            Overview of user engagement and sustainability points distribution
          </p>
        </div>

        {/* Key Metrics Cards */}
        <div style={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))",
          gap: "1.5rem",
          marginBottom: "3rem"
        }}>
          <div style={{
            background: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
            color: "white",
            padding: "2rem",
            borderRadius: "12px",
            textAlign: "center",
            boxShadow: "0 4px 15px rgba(102, 126, 234, 0.4)"
          }}>
            <div style={{ fontSize: "3rem", fontWeight: "bold", marginBottom: "0.5rem" }}>
              {stats.totalUsers}
            </div>
            <div style={{ fontSize: "1.1rem", opacity: "0.9" }}>Total Users</div>
          </div>
          
          <div style={{
            background: "linear-gradient(135deg, #f093fb 0%, #f5576c 100%)",
            color: "white",
            padding: "2rem",
            borderRadius: "12px",
            textAlign: "center",
            boxShadow: "0 4px 15px rgba(240, 147, 251, 0.4)"
          }}>
            <div style={{ fontSize: "3rem", fontWeight: "bold", marginBottom: "0.5rem" }}>
              {stats.totalSustainabilityPoints}
            </div>
            <div style={{ fontSize: "1.1rem", opacity: "0.9" }}>Total Points</div>
          </div>
          
          <div style={{
            background: "linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)",
            color: "white",
            padding: "2rem",
            borderRadius: "12px",
            textAlign: "center",
            boxShadow: "0 4px 15px rgba(79, 172, 254, 0.4)"
          }}>
            <div style={{ fontSize: "3rem", fontWeight: "bold", marginBottom: "0.5rem" }}>
              {stats.averageSustainabilityPoints.toFixed(1)}
            </div>
            <div style={{ fontSize: "1.1rem", opacity: "0.9" }}>Average Points</div>
          </div>
          
          <div style={{
            background: "linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)",
            color: "white",
            padding: "2rem",
            borderRadius: "12px",
            textAlign: "center",
            boxShadow: "0 4px 15px rgba(67, 233, 123, 0.4)"
          }}>
            <div style={{ fontSize: "3rem", fontWeight: "bold", marginBottom: "0.5rem" }}>
              {stats.highestSustainabilityPoints}
            </div>
            <div style={{ fontSize: "1.1rem", opacity: "0.9" }}>Highest Score</div>
          </div>
        </div>

        {/* Charts Section */}
        <div style={{
          display: "grid",
          gridTemplateColumns: "1fr 1fr",
          gap: "3rem",
          marginBottom: "2rem"
        }}>
          {/* User Distribution Pie Chart */}
          <div style={{
            backgroundColor: "#f8f9fa",
            padding: "1.5rem",
            borderRadius: "12px",
            border: "1px solid #e9ecef"
          }}>
            <h3 style={{ 
              textAlign: "center", 
              marginBottom: "1.5rem", 
              color: "#2c3e50",
              fontSize: "1.3rem"
            }}>
              👥 User Engagement Distribution
            </h3>
            <div style={{ height: "300px" }}>
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={userDistributionData}
                    cx="50%"
                    cy="50%"
                    labelLine={false}
                    label={({ name, percentage }) => `${percentage}%`}
                    outerRadius={100}
                    fill="#8884d8"
                    dataKey="value"
                    strokeWidth={3}
                    stroke="#fff"
                  >
                    {userDistributionData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Pie>
                  <Tooltip 
                    formatter={(value, name) => [
                      `${value} users (${userDistributionData.find(d => d.name === name)?.percentage}%)`, 
                      name
                    ]}
                  />
                  <Legend 
                    verticalAlign="bottom" 
                    height={36}
                    formatter={(value) => <span style={{ color: "#2c3e50" }}>{value}</span>}
                  />
                </PieChart>
              </ResponsiveContainer>
            </div>
          </div>

          {/* Points Comparison Bar Chart */}
          <div style={{
            backgroundColor: "#f8f9fa",
            padding: "1.5rem",
            borderRadius: "12px",
            border: "1px solid #e9ecef"
          }}>
            <h3 style={{ 
              textAlign: "center", 
              marginBottom: "1.5rem", 
              color: "#2c3e50",
              fontSize: "1.3rem"
            }}>
              📊 Points Distribution Analysis
            </h3>
            <div style={{ height: "300px" }}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={pointsComparisonData} margin={{ top: 20, right: 30, left: 20, bottom: 5 }}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e9ecef" />
                  <XAxis 
                    dataKey="category" 
                    tick={{ fill: "#2c3e50", fontSize: 12 }}
                    axisLine={{ stroke: "#dee2e6" }}
                  />
                  <YAxis 
                    tick={{ fill: "#2c3e50", fontSize: 12 }}
                    axisLine={{ stroke: "#dee2e6" }}
                    label={{ value: 'Points', angle: -90, position: 'insideLeft', style: { textAnchor: 'middle', fill: '#2c3e50' } }}
                  />
                  <Tooltip 
                    formatter={(value) => [`${value} points`, 'Sustainability Points']}
                    labelStyle={{ color: "#2c3e50" }}
                    contentStyle={{ 
                      backgroundColor: "white", 
                      border: "1px solid #dee2e6",
                      borderRadius: "8px"
                    }}
                  />
                  <Bar 
                    dataKey="value" 
                    fill="#8884d8" 
                    radius={[4, 4, 0, 0]}
                    stroke="#fff"
                    strokeWidth={2}
                  >
                    {pointsComparisonData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={entry.color} />
                    ))}
                  </Bar>
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>
        </div>

        {/* Detailed Statistics */}
        <div style={{
          backgroundColor: "#f8f9fa",
          padding: "2rem",
          borderRadius: "12px",
          border: "1px solid #e9ecef"
        }}>
          <h3 style={{ 
            margin: "0 0 1.5rem 0", 
            color: "#2c3e50",
            textAlign: "center",
            fontSize: "1.3rem"
          }}>
            📈 Detailed Analytics
          </h3>
          
          <div style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(250px, 1fr))",
            gap: "2rem"
          }}>
            {/* Participation Rate */}
            <div>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "1rem" }}>
                <strong style={{ color: "#2c3e50" }}>Participation Rate</strong>
                <span style={{ 
                  color: "#27ae60", 
                  fontWeight: "bold",
                  fontSize: "1.1rem"
                }}>
                  {((stats.usersWithSustainabilityPoints / stats.totalUsers) * 100).toFixed(1)}%
                </span>
              </div>
              <div style={{
                width: "100%",
                backgroundColor: "#e9ecef",
                borderRadius: "25px",
                height: "30px",
                position: "relative",
                overflow: "hidden"
              }}>
                <div style={{
                  width: `${(stats.usersWithSustainabilityPoints / stats.totalUsers) * 100}%`,
                  background: "linear-gradient(90deg, #27ae60, #2ecc71)",
                  height: "100%",
                  borderRadius: "25px",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  color: "white",
                  fontWeight: "bold",
                  fontSize: "0.9rem",
                  transition: "width 1s ease-in-out"
                }}>
                  {stats.usersWithSustainabilityPoints} / {stats.totalUsers}
                </div>
              </div>
            </div>
            
            {/* Points Range */}
            <div>
              <strong style={{ color: "#2c3e50", display: "block", marginBottom: "1rem" }}>Points Range</strong>
              <div style={{ 
                display: "flex", 
                alignItems: "center", 
                gap: "1rem",
                padding: "1rem",
                backgroundColor: "white",
                borderRadius: "8px",
                border: "1px solid #dee2e6"
              }}>
                <div style={{ textAlign: "center" }}>
                  <div style={{ fontSize: "1.5rem", fontWeight: "bold", color: "#95a5a6" }}>
                    {stats.lowestSustainabilityPoints}
                  </div>
                  <div style={{ fontSize: "0.8rem", color: "#7f8c8d" }}>MIN</div>
                </div>
                <div style={{ 
                  flex: 1, 
                  height: "4px", 
                  background: "linear-gradient(90deg, #95a5a6, #3498db, #27ae60)",
                  borderRadius: "2px"
                }}></div>
                <div style={{ textAlign: "center" }}>
                  <div style={{ fontSize: "1.5rem", fontWeight: "bold", color: "#27ae60" }}>
                    {stats.highestSustainabilityPoints}
                  </div>
                  <div style={{ fontSize: "0.8rem", color: "#7f8c8d" }}>MAX</div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  };

  return (
    <div style={{ display: "flex", height: "100vh" }}>
      <Sidebar menuItems={menuItems} />
      <div style={{ flex: 1, padding: "2rem", backgroundColor: "#f5f7fa", overflowY: "auto" }}>
        <h1 style={{ color: "#2c3e50", marginBottom: "2rem" }}>Dashboard</h1>

        {/* Resource Cards */}
        <div style={{ marginBottom: "3rem" }}>
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fill, minmax(250px, 1fr))",
              gap: "1rem"
            }}
          >
            {/* Electricity card */}
            <div
              style={{
                background: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
                color: "white",
                padding: "1.5rem",
                borderRadius: "12px",
                cursor: "pointer",
                boxShadow: "0 4px 15px rgba(102, 126, 234, 0.3)",
                transition: "transform 0.3s ease, box-shadow 0.3s ease"
              }}
              onMouseEnter={(e) => {
                e.target.style.transform = "translateY(-5px)";
                e.target.style.boxShadow = "0 8px 25px rgba(102, 126, 234, 0.4)";
              }}
              onMouseLeave={(e) => {
                e.target.style.transform = "translateY(0)";
                e.target.style.boxShadow = "0 4px 15px rgba(102, 126, 234, 0.3)";
              }}
              onClick={() => navigate("/electricity")}
            >
              <h3 style={{ margin: "0 0 0.5rem 0" }}>⚡ Electricity</h3>
              <p style={{ margin: 0, opacity: 0.9 }}>Monitor and analyze electricity consumption</p>
            </div>

            {/* Water card */}
            <div
              style={{
                background: "linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)",
                color: "white",
                padding: "1.5rem",
                borderRadius: "12px",
                cursor: "pointer",
                boxShadow: "0 4px 15px rgba(79, 172, 254, 0.3)",
                transition: "transform 0.3s ease, box-shadow 0.3s ease"
              }}
              onMouseEnter={(e) => {
                e.target.style.transform = "translateY(-5px)";
                e.target.style.boxShadow = "0 8px 25px rgba(79, 172, 254, 0.4)";
              }}
              onMouseLeave={(e) => {
                e.target.style.transform = "translateY(0)";
                e.target.style.boxShadow = "0 4px 15px rgba(79, 172, 254, 0.3)";
              }}
              onClick={() => navigate("/water")}
            >
              <h3 style={{ margin: "0 0 0.5rem 0" }}>💧 Water</h3>
              <p style={{ margin: 0, opacity: 0.9 }}>Monitor and analyze water consumption</p>
            </div>

            {/* Paper card */}
            <div
              style={{
                background: "linear-gradient(135deg, #fa709a 0%, #fee140 100%)",
                color: "white",
                padding: "1.5rem",
                borderRadius: "12px",
                cursor: "pointer",
                boxShadow: "0 4px 15px rgba(250, 112, 154, 0.3)",
                transition: "transform 0.3s ease, box-shadow 0.3s ease"
              }}
              onMouseEnter={(e) => {
                e.target.style.transform = "translateY(-5px)";
                e.target.style.boxShadow = "0 8px 25px rgba(250, 112, 154, 0.4)";
              }}
              onMouseLeave={(e) => {
                e.target.style.transform = "translateY(0)";
                e.target.style.boxShadow = "0 4px 15px rgba(250, 112, 154, 0.3)";
              }}
              onClick={() => navigate("/paper")}
            >
              <h3 style={{ margin: "0 0 0.5rem 0" }}>📄 Paper</h3>
              <p style={{ margin: 0, opacity: 0.9 }}>Monitor and analyze paper consumption</p>
            </div>

            {/* Natural Gas card */}
            <div
              style={{
                background: "linear-gradient(135deg, #f093fb 0%, #f5576c 100%)",
                color: "white",
                padding: "1.5rem",
                borderRadius: "12px",
                cursor: "pointer",
                boxShadow: "0 4px 15px rgba(240, 147, 251, 0.3)",
                transition: "transform 0.3s ease, box-shadow 0.3s ease"
              }}
              onMouseEnter={(e) => {
                e.target.style.transform = "translateY(-5px)";
                e.target.style.boxShadow = "0 8px 25px rgba(240, 147, 251, 0.4)";
              }}
              onMouseLeave={(e) => {
                e.target.style.transform = "translateY(0)";
                e.target.style.boxShadow = "0 4px 15px rgba(240, 147, 251, 0.3)";
              }}
              onClick={() => navigate("/natural-gas")}
            >
              <h3 style={{ margin: "0 0 0.5rem 0" }}>🔥 Natural Gas</h3>
              <p style={{ margin: 0, opacity: 0.9 }}>Monitor and analyze natural gas consumption</p>
            </div>
          </div>
        </div>

        {/* Sustainability Statistics Section */}
        {renderSustainabilityStats()}

        {/* Monthly Air Quality Chart Only */}
        <div style={{
          backgroundColor: "white",
          padding: "2rem",
          borderRadius: "16px",
          boxShadow: "0 8px 25px rgba(0,0,0,0.1)",
          marginBottom: "2rem"
        }}>
          {/* Header with City Search */}
          <div style={{ 
            display: "flex", 
            justifyContent: "space-between", 
            alignItems: "center",
            marginBottom: "2rem",
            flexWrap: "wrap",
            gap: "1rem"
          }}>
            <h3 style={{ 
              margin: 0, 
              color: "#2c3e50",
              fontSize: "1.8rem",
              display: "flex",
              alignItems: "center",
              gap: "0.5rem"
            }}>
              🌍 Monthly Air Quality Trends
            </h3>
            
            <form onSubmit={handleCitySearch} style={{ display: "flex", gap: "0.5rem" }}>
              <input
                type="text"
                value={cityInput}
                onChange={(e) => setCityInput(e.target.value)}
                placeholder="Enter city name (e.g., Istanbul, Turkey)"
                style={{
                  padding: "0.75rem 1rem",
                  borderRadius: "8px",
                  border: "2px solid #e9ecef",
                  outline: "none",
                  fontSize: "1rem",
                  minWidth: "250px",
                  transition: "border-color 0.3s ease"
                }}
                onFocus={(e) => e.target.style.borderColor = "#3498db"}
                onBlur={(e) => e.target.style.borderColor = "#e9ecef"}
              />
              <button
                type="submit"
                style={{
                  background: "linear-gradient(135deg, #667eea 0%, #764ba2 100%)",
                  color: "white",
                  border: "none",
                  borderRadius: "8px",
                  padding: "0.75rem 1.5rem",
                  cursor: "pointer",
                  fontSize: "1rem",
                  fontWeight: "600",
                  transition: "transform 0.2s ease",
                  boxShadow: "0 4px 15px rgba(102, 126, 234, 0.3)"
                }}
                onMouseEnter={(e) => e.target.style.transform = "translateY(-2px)"}
                onMouseLeave={(e) => e.target.style.transform = "translateY(0)"}
              >
                🔍 Search
              </button>
            </form>
          </div>

          {/* Current City Display */}
          <div style={{
            backgroundColor: "#f8f9fa",
            padding: "1rem",
            borderRadius: "8px",
            marginBottom: "2rem",
            border: "1px solid #e9ecef"
          }}>
            <p style={{ 
              margin: 0, 
              color: "#495057",
              fontSize: "1.1rem"
            }}>
              📍 Showing data for: <strong>{city}</strong>
            </p>
          </div>

          {/* Monthly chart loading */}
          {isLoadingMonthly && (
            <div style={{ 
              textAlign: "center", 
              padding: "3rem"
            }}>
              <div style={{
                display: "inline-block",
                width: "50px",
                height: "50px",
                border: "5px solid #f3f3f3",
                borderTop: "5px solid #3498db",
                borderRadius: "50%",
                animation: "spin 1s linear infinite"
              }}></div>
              <style>
                {`
                  @keyframes spin {
                    0% { transform: rotate(0deg); }
                    100% { transform: rotate(360deg); }
                  }
                `}
              </style>
              <p style={{ marginTop: "1rem", fontSize: "1.1rem", color: "#7f8c8d" }}>
                Loading air quality data for {city}...
              </p>
            </div>
          )}

          {/* Monthly chart error */}
          {monthlyError && (
            <div style={{
              backgroundColor: "#fff5f5",
              border: "1px solid #feb2b2",
              color: "#c53030",
              padding: "2rem",
              borderRadius: "12px",
              textAlign: "center"
            }}>
              <div style={{ fontSize: "2rem", marginBottom: "1rem" }}>⚠️</div>
              <strong>Error:</strong> {monthlyError}
              <button 
                onClick={() => window.location.reload()} 
                style={{
                  marginTop: "1rem",
                  padding: "0.5rem 1rem",
                  backgroundColor: "#c53030",
                  color: "white",
                  border: "none",
                  borderRadius: "6px",
                  cursor: "pointer",
                  display: "block",
                  margin: "1rem auto 0"
                }}
              >
                Retry
              </button>
            </div>
          )}

          {/* Monthly chart */}
          {!isLoadingMonthly && !monthlyError && monthlyData.length > 0 && (
            <>
              <div style={{ height: "450px", marginBottom: "2rem" }}>
                <ResponsiveContainer width="100%" height="100%">
                  <AreaChart
                    data={monthlyData}
                    margin={{ top: 20, right: 30, left: 20, bottom: 60 }}
                  >
                    <defs>
                      <linearGradient id="aqiGradient" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="5%" stopColor="#667eea" stopOpacity={0.8}/>
                        <stop offset="95%" stopColor="#667eea" stopOpacity={0.1}/>
                      </linearGradient>
                    </defs>
                    <CartesianGrid strokeDasharray="3 3" stroke="#e9ecef" />
                    <XAxis 
                      dataKey="date" 
                      angle={-45}
                      textAnchor="end"
                      height={80}
                      fontSize={12}
                      tick={{ fill: "#495057" }}
                      axisLine={{ stroke: "#dee2e6" }}
                    />
                    <YAxis 
                      label={{ 
                        value: 'Air Quality Index (AQI)', 
                        angle: -90, 
                        position: 'insideLeft',
                        style: { textAnchor: 'middle', fill: '#495057' }
                      }}
                      tick={{ fill: "#495057" }}
                      axisLine={{ stroke: "#dee2e6" }}
                    />
                    <Tooltip content={<CustomTooltip />} />
                    <Legend />
                    <Area
                      type="monotone"
                      dataKey="aqi"
                      stroke="#667eea"
                      strokeWidth={3}
                      fillOpacity={1}
                      fill="url(#aqiGradient)"
                      name="Air Quality Index (AQI)"
                    />
                  </AreaChart>
                </ResponsiveContainer>
              </div>

              {/* AQI Legend */}
              <div style={{
                display: "flex",
                flexWrap: "wrap",
                gap: "1rem",
                justifyContent: "center",
                padding: "1.5rem",
                backgroundColor: "#f8f9fa",
                borderRadius: "12px",
                border: "1px solid #e9ecef"
              }}>
                <h4 style={{ 
                  width: "100%", 
                  textAlign: "center", 
                  margin: "0 0 1rem 0",
                  color: "#2c3e50",
                  fontSize: "1.1rem"
                }}>
                  🎯 AQI Scale Reference
                </h4>
                {[
                  { color: "#a8e05f", label: "Good", range: "0-50" },
                  { color: "#fdd74b", label: "Moderate", range: "51-100" },
                  { color: "#fe9b57", label: "Unhealthy for Sensitive", range: "101-150" },
                  { color: "#fe6a69", label: "Unhealthy", range: "151-200" },
                  { color: "#a97abc", label: "Very Unhealthy", range: "201-300" },
                  { color: "#a87383", label: "Hazardous", range: "300+" }
                ].map((item, index) => (
                  <div key={index} style={{ 
                    display: "flex", 
                    alignItems: "center", 
                    gap: "0.5rem",
                    padding: "0.5rem 1rem",
                    backgroundColor: "white",
                    borderRadius: "20px",
                    border: "1px solid #dee2e6",
                    fontSize: "0.9rem"
                  }}>
                    <div style={{ 
                      width: "20px", 
                      height: "20px", 
                      backgroundColor: item.color,
                      borderRadius: "50%",
                      border: "2px solid white",
                      boxShadow: "0 2px 4px rgba(0,0,0,0.1)"
                    }}></div>
                    <span style={{ fontWeight: "600", color: "#2c3e50" }}>
                      {item.label}
                    </span>
                    <span style={{ fontSize: "0.8rem", color: "#6c757d" }}>
                      ({item.range})
                    </span>
                  </div>
                ))}
              </div>
            </>
          )}

          {/* No data message */}
          {!isLoadingMonthly && !monthlyError && monthlyData.length === 0 && (
            <div style={{
              textAlign: "center",
              padding: "3rem",
              color: "#6c757d"
            }}>
              <div style={{ fontSize: "4rem", marginBottom: "1rem" }}>📊</div>
              <h3 style={{ color: "#495057", marginBottom: "1rem" }}>No Air Quality Data Available</h3>
              <p style={{ fontSize: "1.1rem", margin: 0 }}>
                No monthly air quality data found for <strong>{city}</strong>.
                <br />
                Try searching for a different city or check back later.
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}