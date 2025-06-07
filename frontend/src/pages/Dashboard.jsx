import React, { useState, useEffect } from "react";
import Sidebar from "../components/Sidebar";
import { getCityAirQuality, getGeoAirQuality } from "../services/dashboardService";

export default function Dashboard() {
  const [airQuality, setAirQuality] = useState(null);
  const [city, setCity] = useState("Kocaeli Gebze, Turkey");
  const [geoAirQuality, setGeoAirQuality] = useState(null);
  const [geoLocation, setGeoLocation] = useState(null);
  const [cityInput, setCityInput] = useState(city);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  // Air quality color scale based on AQI value
  const getAqiColor = (aqi) => {
    if (aqi <= 50) return { bg: "#a8e05f", text: "#2a4d00", level: "Good" };
    if (aqi <= 100) return { bg: "#fdd74b", text: "#8d5e00", level: "Moderate" };
    if (aqi <= 150) return { bg: "#fe9b57", text: "#9c3400", level: "Unhealthy for Sensitive Groups" };
    if (aqi <= 200) return { bg: "#fe6a69", text: "#a11c1c", level: "Unhealthy" };
    if (aqi <= 300) return { bg: "#a97abc", text: "#4f266e", level: "Very Unhealthy" };
    return { bg: "#a87383", text: "#591035", level: "Hazardous" };
  };

  // Icons for different pollutants and weather parameters
  const getIcon = (type) => {
    switch (type) {
      case "pm10":
        return "💨";
      case "pm25":
        return "🌫️";
      case "no2":
        return "🏭";
      case "so2":
        return "🧪";
      case "co":
        return "🚗";
      case "t":
        return "🌡️";
      case "h":
        return "💧";
      case "w":
        return "💨";
      default:
        return "ℹ️";
    }
  };

  // Fetching air quality data for a city
  useEffect(() => {
    const fetchAirQuality = async () => {
      setIsLoading(true);
      setError(null);
      try {
        const data = await getCityAirQuality(city);
        setAirQuality(data.data);
      } catch (error) {
        console.error("Error fetching air quality data:", error);
        setError("Failed to load air quality data. Please try a different city.");
      } finally {
        setIsLoading(false);
      }
    };

    fetchAirQuality();
  }, [city]);

  // Handle city search
  const handleCitySearch = (e) => {
    e.preventDefault();
    if (cityInput.trim()) {
      setCity(cityInput.trim());
    }
  };

  // Get user's geolocation
  const getUserLocation = () => {
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition(
        (position) => {
          const { latitude, longitude } = position.coords;
          setGeoLocation({ lat: latitude, lng: longitude });
          
          // Now fetch air quality based on these coordinates
          fetchGeoAirQuality(latitude, longitude);
        },
        (error) => {
          console.error("Error getting location:", error);
        }
      );
    } else {
      console.error("Geolocation is not supported by this browser.");
    }
  };

  // Fetch air quality based on coordinates
  const fetchGeoAirQuality = async (lat, lng) => {
    setIsLoading(true);
    try {
      const data = await getGeoAirQuality(lat, lng);
      setGeoAirQuality(data.data);
    } catch (error) {
      console.error("Error fetching geo air quality:", error);
    } finally {
      setIsLoading(false);
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
    { key: "adminTools", name: "Admin Tools" },
    { key: "reports", name: "Reports" }
  ];

  // Render an air quality card
  const renderAirQualityCard = (data, title) => {
    if (!data) return null;
    
    const aqiInfo = getAqiColor(data.aqi);
    
    return (
      <div style={{ 
        marginTop: "2rem", 
        borderRadius: "12px", 
        overflow: "hidden",
        boxShadow: "0 10px 20px rgba(0,0,0,0.1)"
      }}>
        {/* Header */}
        <div style={{ 
          backgroundColor: "#2c3e50", 
          padding: "1rem 1.5rem",
          color: "white",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center"
        }}>
          <h2 style={{ margin: "0", fontSize: "1.3rem" }}>{title}</h2>
          <span style={{ fontSize: "0.9rem", opacity: "0.8" }}>
            Last updated: {new Date().toLocaleTimeString()}
          </span>
        </div>
        
        {/* AQI Score Section */}
        <div style={{ 
          display: "flex",
          flexDirection: "column",
          alignItems: "center",
          padding: "1.5rem",
          backgroundColor: aqiInfo.bg,
          color: aqiInfo.text
        }}>
          <div style={{ fontSize: "3rem", fontWeight: "bold" }}>{data.aqi}</div>
          <div style={{ fontSize: "1.2rem", fontWeight: "600" }}>
            Air Quality: {aqiInfo.level}
          </div>
          <div style={{ marginTop: "0.5rem", fontSize: "0.9rem" }}>
            Dominant Pollutant: {data.dominentpol.toUpperCase()}
          </div>
        </div>
        
        {/* Pollutants Grid */}
        <div style={{ 
          display: "grid", 
          gridTemplateColumns: "repeat(auto-fill, minmax(150px, 1fr))",
          gap: "1rem",
          padding: "1.5rem",
          backgroundColor: "#f8f9fa"
        }}>
          {Object.entries(data.iaqi).map(([key, value]) => (
            <div key={key} style={{ 
              backgroundColor: "white",
              padding: "1rem",
              borderRadius: "8px",
              boxShadow: "0 2px 4px rgba(0,0,0,0.05)",
              display: "flex",
              flexDirection: "column",
              alignItems: "center"
            }}>
              <div style={{ fontSize: "1.8rem", marginBottom: "0.5rem" }}>
                {getIcon(key)}
              </div>
              <div style={{ 
                textTransform: "uppercase", 
                fontWeight: "bold",
                fontSize: "0.8rem",
                color: "#7f8c8d"
              }}>
                {key === "pm25" ? "PM2.5" : key === "t" ? "TEMP" : key === "h" ? "HUMIDITY" : key.toUpperCase()}
              </div>
              <div style={{ 
                fontWeight: "bold", 
                fontSize: "1.2rem",
                marginTop: "0.3rem"
              }}>
                {value.v} 
                <span style={{ fontSize: "0.7rem", marginLeft: "2px" }}>
                  {key === "t" ? "°C" : key === "h" ? "%" : "µg/m³"}
                </span>
              </div>
            </div>
          ))}
        </div>
        
        {/* Footer with more details */}
        <div style={{ 
          padding: "1rem 1.5rem",
          backgroundColor: "#ecf0f1",
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          borderTop: "1px solid #ddd"
        }}>
          <div>
            <strong>Location:</strong> {data.city.name}
          </div>
          <a 
            href={data.city.url} 
            target="_blank" 
            rel="noopener noreferrer"
            style={{
              backgroundColor: "#3498db",
              color: "white",
              padding: "0.5rem 1rem",
              borderRadius: "4px",
              textDecoration: "none",
              fontWeight: "500"
            }}
          >
            View Details
          </a>
        </div>
      </div>
    );
  };

  return (
    <div style={{ display: "flex", height: "100vh" }}>
      <Sidebar menuItems={menuItems} />
      <div style={{ flex: 1, padding: "2rem", backgroundColor: "#f5f7fa", overflowY: "auto" }}>
        <h1 style={{ color: "#2c3e50" }}>Dashboard</h1>

        {/* Resource Cards */}
        <div style={{ marginTop: "1.5rem" }}>
          <div
            style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fill, minmax(250px, 1fr))",
              gap: "1rem",
              marginTop: "1rem"
            }}
          >
            {/* Electricity card */}
            <div
              style={{
                backgroundColor: "#3498db",
                color: "white",
                padding: "1.5rem",
                borderRadius: "8px",
                cursor: "pointer",
                boxShadow: "0 4px 6px rgba(0, 0, 0, 0.1)",
                transition: "transform 0.2s, box-shadow 0.2s",
                ":hover": {
                  transform: "translateY(-5px)",
                  boxShadow: "0 6px 8px rgba(0, 0, 0, 0.15)"
                }
              }}
              onClick={() => window.location.href = "/electricity"}
            >
              <h3 style={{ margin: "0 0 0.5rem 0" }}>Electricity</h3>
              <p style={{ margin: 0 }}>Monitor and analyze electricity consumption</p>
            </div>

            {/* Water card */}
            <div
              style={{
                backgroundColor: "#2ecc71",
                color: "white",
                padding: "1.5rem",
                borderRadius: "8px",
                cursor: "pointer",
                boxShadow: "0 4px 6px rgba(0, 0, 0, 0.1)",
                transition: "transform 0.2s, box-shadow 0.2s"
              }}
              onClick={() => window.location.href = "/water"}
            >
              <h3 style={{ margin: "0 0 0.5rem 0" }}>Water</h3>
              <p style={{ margin: 0 }}>Monitor and analyze water consumption</p>
            </div>

            {/* Paper card */}
            <div
              style={{
                backgroundColor: "#f39c12",
                color: "white",
                padding: "1.5rem",
                borderRadius: "8px",
                cursor: "pointer",
                boxShadow: "0 4px 6px rgba(0, 0, 0, 0.1)",
                transition: "transform 0.2s, box-shadow 0.2s"
              }}
              onClick={() => window.location.href = "/paper"}
            >
              <h3 style={{ margin: "0 0 0.5rem 0" }}>Paper</h3>
              <p style={{ margin: 0 }}>Monitor and analyze paper consumption</p>
            </div>

            {/* Natural Gas card */}
            <div
              style={{
                backgroundColor: "#e74c3c",
                color: "white",
                padding: "1.5rem",
                borderRadius: "8px",
                cursor: "pointer",
                boxShadow: "0 4px 6px rgba(0, 0, 0, 0.1)",
                transition: "transform 0.2s, box-shadow 0.2s"
              }}
              onClick={() => window.location.href = "/natural-gas"}
            >
              <h3 style={{ margin: "0 0 0.5rem 0" }}>Natural Gas</h3>
              <p style={{ margin: 0 }}>Monitor and analyze natural gas consumption</p>
            </div>
          </div>
        </div>

        {/* Air Quality Section */}
        <div style={{ marginTop: "3rem" }}>
          <div style={{ 
            display: "flex", 
            justifyContent: "space-between", 
            alignItems: "center",
            marginBottom: "1rem"
          }}>
            <h2 style={{ color: "#2c3e50", margin: 0 }}>Air Quality Monitoring</h2>
            
            <div style={{ display: "flex", gap: "1rem" }}>
              <form onSubmit={handleCitySearch} style={{ display: "flex" }}>
                <input
                  type="text"
                  value={cityInput}
                  onChange={(e) => setCityInput(e.target.value)}
                  placeholder="Enter city name"
                  style={{
                    padding: "0.6rem 1rem",
                    borderRadius: "4px 0 0 4px",
                    border: "1px solid #ddd",
                    borderRight: "none",
                    outline: "none"
                  }}
                />
                <button
                  type="submit"
                  style={{
                    backgroundColor: "#3498db",
                    color: "white",
                    border: "none",
                    borderRadius: "0 4px 4px 0",
                    padding: "0 1rem",
                    cursor: "pointer"
                  }}
                >
                  Search
                </button>
              </form>
              
              <button
                onClick={getUserLocation}
                style={{
                  backgroundColor: "#2ecc71",
                  color: "white",
                  border: "none",
                  borderRadius: "4px",
                  padding: "0.6rem 1rem",
                  cursor: "pointer",
                  display: "flex",
                  alignItems: "center",
                  gap: "0.5rem"
                }}
              >
                <span>📍</span> My Location
              </button>
            </div>
          </div>

          {/* Loading state */}
          {isLoading && (
            <div style={{ 
              textAlign: "center", 
              padding: "2rem", 
              backgroundColor: "white",
              borderRadius: "8px",
              marginTop: "1rem"
            }}>
              <div style={{
                display: "inline-block",
                width: "40px",
                height: "40px",
                border: "4px solid #f3f3f3",
                borderTop: "4px solid #3498db",
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
              <p style={{ marginTop: "1rem" }}>Loading air quality data...</p>
            </div>
          )}

          {/* Error state */}
          {error && (
            <div style={{
              backgroundColor: "#f8d7da",
              color: "#721c24",
              padding: "1rem",
              borderRadius: "4px",
              marginTop: "1rem"
            }}>
              <strong>Error:</strong> {error}
            </div>
          )}

          {/* City Air Quality Card */}
          {renderAirQualityCard(airQuality, `Air Quality in ${airQuality?.city?.name || city}`)}

          {/* Geo Air Quality Card (if available) */}
          {renderAirQualityCard(geoAirQuality, "Air Quality at Your Location")}
        </div>
      </div>
    </div>
  );
}