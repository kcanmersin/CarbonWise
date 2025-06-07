import React, { useState, useEffect } from "react";
import Sidebar from "../components/Sidebar";
import { 
  BarChart, Bar, XAxis, YAxis, CartesianGrid, 
  Tooltip, Legend, ResponsiveContainer 
} from 'recharts';
import { filterWaters } from "../services/waterService";

const WaterPage = () => {
  // Set default date range: Jan 1st of current year to today
  const currentYear = new Date().getFullYear();
  const firstDayOfYear = new Date(currentYear, 0, 1);
  const today = new Date();

  // State variables
  const [monthlyData, setMonthlyData] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  const [startDate, setStartDate] = useState(firstDayOfYear);
  const [endDate, setEndDate] = useState(today);
  
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

  // Fetch water data on component mount and when date range changes
  useEffect(() => {
    fetchWaterData();
  }, [startDate, endDate]);

  // Format date as YYYY-MM-DD for input fields
  const formatDateForInput = (date) => {
    if (!(date instanceof Date) || isNaN(date.getTime())) {
      console.error("Invalid date:", date);
      return "";
    }
    
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  };

  // Handle date selection changes
  const handleStartDateChange = (e) => {
    const newDate = new Date(e.target.value);
    if (!isNaN(newDate.getTime())) {
      if (newDate > endDate) {
        setStartDate(newDate);
        setEndDate(newDate);
      } else {
        setStartDate(newDate);
      }
    }
  };
  
  const handleEndDateChange = (e) => {
    const newDate = new Date(e.target.value);
    if (!isNaN(newDate.getTime())) {
      if (newDate < startDate) {
        setEndDate(newDate);
        setStartDate(newDate);
      } else {
        setEndDate(newDate);
      }
    }
  };

  // Function to fetch water data for the selected date range
  const fetchWaterData = async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      // Validate dates before proceeding
      if (!(startDate instanceof Date) || !(endDate instanceof Date)) {
        throw new Error("Invalid date objects");
      }
      
      if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) {
        throw new Error("Invalid date values");
      }
      
      console.log("Fetching water data with date range:", {
        startDate: startDate,
        endDate: endDate
      });
      
      // Pass Date objects directly to filterWaters
      const result = await filterWaters({
        startDate: startDate,
        endDate: endDate
      });
      
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
    // Create a map to aggregate usage by month
    const monthlyUsageMap = new Map();
    
    // First, generate all months in the selected date range
    const startYear = startDate.getFullYear();
    const startMonth = startDate.getMonth();
    const endYear = endDate.getFullYear();
    const endMonth = endDate.getMonth();
    
    // Generate all month-year combinations within range with 0 usage
    for (let year = startYear; year <= endYear; year++) {
      const monthStart = (year === startYear) ? startMonth : 0;
      const monthEnd = (year === endYear) ? endMonth : 11;
      
      for (let month = monthStart; month <= monthEnd; month++) {
        const monthYearKey = `${year}-${month}`;
        monthlyUsageMap.set(monthYearKey, {
          month: getMonthName(month),
          year: year,
          fullMonth: `${getMonthName(month)} ${year}`,
          usage: 0
        });
      }
    }
    
    // Process each record from the API
    data.forEach(record => {
      // Extract month and year from the date string
      const recordDate = new Date(record.date);
      const year = recordDate.getFullYear();
      const month = recordDate.getMonth();
      
      // Create a unique key for each month-year combination
      const monthYearKey = `${year}-${month}`;
      
      // Only process if this month is in our selected range
      if (monthlyUsageMap.has(monthYearKey)) {
        const currentMonthData = monthlyUsageMap.get(monthYearKey);
        
        // Add this record's usage to the month's total
        currentMonthData.usage += record.usage;
        
        // Update the map
        monthlyUsageMap.set(monthYearKey, currentMonthData);
      }
    });
    
    // Convert the map to an array and sort by date
    const monthlyUsageArray = Array.from(monthlyUsageMap.values())
      .sort((a, b) => {
        if (a.year !== b.year) return a.year - b.year;
        return getMonthIndex(a.month) - getMonthIndex(b.month);
      });
    
    return monthlyUsageArray;
  };

  // Helper function to get month name
  const getMonthName = (monthIndex) => {
    const months = [
      "Jan", "Feb", "Mar", "Apr", "May", "Jun", 
      "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    ];
    return months[monthIndex];
  };
  
  // Helper function to get month index from name
  const getMonthIndex = (monthName) => {
    const months = [
      "Jan", "Feb", "Mar", "Apr", "May", "Jun", 
      "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    ];
    return months.indexOf(monthName);
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
            <button 
              onClick={() => setError(null)} 
              style={{
                marginLeft: "10px",
                background: "none",
                border: "none",
                color: "#721c24",
                fontWeight: "bold",
                cursor: "pointer"
              }}
            >
              ×
            </button>
          </div>
        )}
        
        {/* Main Content */}
        {!isLoading && !error && (
        <div style={{ 
          backgroundColor: "#fff", 
          padding: "1rem", 
          borderRadius: "4px", 
          boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
          height: "calc(100vh - 130px)"
        }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "1rem" }}>
            <h3>Water Usage Monthly Graph</h3>
            
            {/* Date Range Selection */}
            <div style={{ display: "flex", gap: "1rem", alignItems: "center" }}>
              <div>
                <label htmlFor="startDatePicker" style={{ marginRight: "0.5rem" }}>Start Date:</label>
                <input 
                  type="date"
                  id="startDatePicker"
                  value={formatDateForInput(startDate)}
                  onChange={handleStartDateChange}
                  style={{ 
                    padding: "0.5rem", 
                    borderRadius: "4px", 
                    border: "1px solid #ccc",
                    backgroundColor: "#f5f5f5",
                    width: "160px"
                  }}
                />
              </div>
              
              <div>
                <label htmlFor="endDatePicker" style={{ marginRight: "0.5rem" }}>End Date:</label>
                <input 
                  type="date"
                  id="endDatePicker"
                  value={formatDateForInput(endDate)}
                  onChange={handleEndDateChange}
                  style={{ 
                    padding: "0.5rem", 
                    borderRadius: "4px", 
                    border: "1px solid #ccc",
                    backgroundColor: "#f5f5f5",
                    width: "160px"
                  }}
                />
              </div>
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
                <XAxis 
                  dataKey="fullMonth" 
                  angle={-45}
                  textAnchor="end"
                  height={80}
                />
                <YAxis />
                <Tooltip formatter={(value) => `${value.toFixed(2)} m³`} />
                <Legend />
                <Bar 
                  dataKey="usage" 
                  fill="#1abc9c" 
                  name={`Water Usage (m³)`}
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