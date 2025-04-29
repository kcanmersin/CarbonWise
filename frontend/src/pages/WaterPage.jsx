import React, { useState, useEffect } from "react";
import Sidebar from "../components/Sidebar";
import { 
  BarChart, Bar, XAxis, YAxis, CartesianGrid, 
  Tooltip, Legend, ResponsiveContainer 
} from 'recharts';
import { getAllWaters, filterWaters } from "../services/waterService";

const WaterPage = () => {
  // State variables
  const [monthlyData, setMonthlyData] = useState([]);
  const [selectedYear, setSelectedYear] = useState(new Date().getFullYear());
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // Available years for selection
  const availableYears = [2022, 2023, 2024, 2025];
  
  // Sidebar menu items
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

  // Fetch water data on component mount and when selected year changes
  useEffect(() => {
    fetchWaterData(selectedYear);
  }, [selectedYear]);

  // Function to fetch water data for the selected year
  const fetchWaterData = async (year) => {
    try {
      setIsLoading(true);
      setError(null);
      
      // Set date range for the selected year
      const startDate = new Date(year, 0, 1);  // January 1st of selected year
      const endDate = new Date(year, 11, 31);  // December 31st of selected year
      
      console.log("Fetching water data for year:", year);
      
      let result;
      
      // Filter by year if not current year
      if (year !== new Date().getFullYear()) {
        result = await filterWaters(startDate, endDate);
      } else {
        // Get all records if current year
        result = await getAllWaters();
        // Filter for current year in the frontend
        result = result.filter(record => new Date(record.date).getFullYear() === year);
      }
      
      console.log("Water data received:", result);
      
      // Transform the API data into the format needed for the chart
      const transformedData = processWaterData(result);
      setMonthlyData(transformedData);
    } catch (error) {
      setError("Failed to fetch water data. Please try again later.");
      console.error("Error fetching water data:", error);
    } finally {
      setIsLoading(false);
    }
  };

  // Process water data from API into chart format
  const processWaterData = (data) => {
    // Initialize array for all months
    const months = [
      "Jan", "Feb", "Mar", "Apr", "May", "Jun", 
      "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    ];
    
    const monthlyUsage = months.map((month, index) => ({
      month: month,
      usage: 0,
      monthIndex: index
    }));
    
    // If the data already has usage field, use it directly
    data.forEach(record => {
      const date = new Date(record.date);
      const monthIndex = date.getMonth();
      
      if (record.usage !== undefined) {
        // If usage is provided directly
        monthlyUsage[monthIndex].usage += record.usage;
      } else {
        // If we need to calculate usage from meter values
        const usage = record.finalMeterValue - record.initialMeterValue;
        monthlyUsage[monthIndex].usage += usage;
      }
    });
    
    // Sort by month index to ensure proper ordering
    monthlyUsage.sort((a, b) => a.monthIndex - b.monthIndex);
    
    // Remove monthIndex from final data
    return monthlyUsage.map(({ month, usage }) => ({ month, usage }));
  };

  // Handle year selection change
  const handleYearChange = (e) => {
    const year = parseInt(e.target.value);
    console.log("Changing selected year to:", year);
    setSelectedYear(year);
  };

  return (
    <div style={{ display: "flex", height: "100vh" }}>
      <Sidebar menuItems={menuItems} />
      <div style={{ flex: 1, padding: "1rem", backgroundColor: "#f5f5f5", overflowY: "auto" }}>
        {/* Page Header */}
        <div style={{ 
          backgroundColor: "#333", 
          color: "#fff", 
          padding: "0.5rem 1rem",
          marginBottom: "1rem"
        }}>
          <h2>Resource Monitoring - Water</h2>
        </div>
        
        {/* Loading Indicator */}
        {isLoading && (
          <div style={{ textAlign: "center", padding: "2rem" }}>
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
            <p>Loading data...</p>
          </div>
        )}
        
        {/* Error Display */}
        {error && (
          <div style={{ 
            backgroundColor: "#f8d7da", 
            color: "#721c24", 
            padding: "1rem", 
            borderRadius: "4px",
            marginBottom: "1rem"
          }}>
            <strong>Error:</strong> {error}
          </div>
        )}
        
        {/* Main Content */}
        {!isLoading && !error && (
          /* Monthly Usage Chart Section */
          <div style={{ 
            backgroundColor: "#fff", 
            padding: "1rem", 
            borderRadius: "4px", 
            boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
            height: "calc(100vh - 130px)"
          }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "1rem" }}>
              <h3>Water Usage Monthly Graph</h3>
              
              {/* Year Selection */}
              <div>
                <label htmlFor="yearSelect" style={{ marginRight: "0.5rem" }}>Year:</label>
                <select 
                  id="yearSelect"
                  value={selectedYear}
                  onChange={handleYearChange}
                  style={{ 
                    padding: "0.5rem", 
                    borderRadius: "4px", 
                    border: "1px solid #ccc",
                    backgroundColor: "#eee",
                    width: "100px"
                  }}
                >
                  {availableYears.map(year => (
                    <option key={year} value={year}>{year}</option>
                  ))}
                </select>
              </div>
            </div>
            
            {/* Monthly Usage Bar Chart */}
            <div style={{ height: "calc(100% - 60px)" }}>
              <ResponsiveContainer width="100%" height="100%">
                <BarChart
                  data={monthlyData}
                  margin={{ top: 20, right: 30, left: 20, bottom: 30 }}
                >
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="month" />
                  <YAxis />
                  <Tooltip formatter={(value) => `${value.toFixed(2)} m³`} />
                  <Legend />
                  <Bar 
                    dataKey="usage" 
                    fill="#1abc9c" 
                    name={`Water Usage ${selectedYear} (m³)`}
                  />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default WaterPage;