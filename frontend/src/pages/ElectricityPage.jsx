import React, { useState, useEffect } from "react";
import Sidebar from "../components/Sidebar";
import { 
  BarChart, Bar, LineChart, Line, XAxis, YAxis, CartesianGrid, 
  Tooltip, Legend, ResponsiveContainer 
} from 'recharts';
import { getBuildings } from "../services/buildingService";
import { filterElectrics } from "../services/electricityService";
import { getMonthlyTotals } from "../services/electricityService";

const ElectricityPage = () => {
  // State variables
  const [buildings, setBuildings] = useState([]);
  const [selectedBuildingId, setSelectedBuildingId] = useState("");
  const [selectedBuildingIds, setSelectedBuildingIds] = useState([]);
  
  // Date picker state variables
  const [startDate, setStartDate] = useState(new Date(new Date().getFullYear(), 0, 1)); // Jan 1 of current year
  const [endDate, setEndDate] = useState(new Date()); // Today
  
  const [monthlyData, setMonthlyData] = useState([]);
  const [compareData, setCompareData] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // Colors for the charts
  const lineColors = ["#8884d8", "#82ca9d", "#ffc658", "#ff8042", "#a4de6c", "#d0ed57"];
  
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

  // Fetch buildings on component mount
  useEffect(() => {
    fetchBuildings();
  }, []);

  // Fetch electricity data when selected building or date range changes
  useEffect(() => {
    if (selectedBuildingId === "all") {
      // Fetch total data for all buildings
      fetchAllBuildingsData();
    } else if (selectedBuildingId) {
      // Fetch data for specific building
      fetchElectricityData(selectedBuildingId);
    }
  }, [selectedBuildingId, startDate, endDate]);

  // Fetch comparison data when selected buildings or date range changes
  useEffect(() => {
    if (selectedBuildingIds.length > 0) {
      console.log("Selected building IDs for comparison:", selectedBuildingIds);
      console.log("Date range for comparison:", startDate.toDateString(), "to", endDate.toDateString());
      fetchComparisonData();
    }
  }, [selectedBuildingIds, startDate, endDate]);

  // Function to fetch all buildings
  const fetchBuildings = async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      const buildingsData = await getBuildings();
      console.log("Buildings data:", buildingsData);
      setBuildings(buildingsData);
      
      // Set first building as default selection if available
      if (buildingsData.length > 0) {
        setSelectedBuildingId(buildingsData[0].id);
        setSelectedBuildingIds([buildingsData[0].id]);
      }
    } catch (error) {
      setError("Failed to fetch buildings. Please try again later.");
      console.error("Error fetching buildings:", error);
    } finally {
      setIsLoading(false);
    }
  };

  // Function to fetch electricity data for a specific building and date range
  const fetchElectricityData = async (buildingId) => {
    try {
      setIsLoading(true);
      setError(null);
      
      console.log("Fetching electricity data with params:", {
        buildingId,
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString()
      });
      
      const result = await filterElectrics({
        buildingId: buildingId,
        startDate: startDate,
        endDate: endDate
      });
      
      console.log("Electricity data received:", result);
      
      // Transform the API data into the format needed for the chart
      const transformedData = processElectricityData(result);
      setMonthlyData(transformedData);
    } catch (error) {
      setError("Failed to fetch electricity data. Please try again later.");
      console.error("Error fetching electricity data:", error);
    } finally {
      setIsLoading(false);
    }
  };

  // Function to fetch comparison data for multiple buildings and date range
  const fetchComparisonData = async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      // Fetch data for each selected building
      const comparisonDataPromises = selectedBuildingIds.map(buildingId => 
        filterElectrics({
          buildingId: buildingId,
          startDate: startDate,
          endDate: endDate
        })
      );
      
      const results = await Promise.all(comparisonDataPromises);
      console.log("Comparison data received:", results);
      
      // Process and combine data for comparison chart
      const processedCompareData = processComparisonData(results, selectedBuildingIds);
      setCompareData(processedCompareData);
    } catch (error) {
      setError("Failed to fetch comparison data. Please try again later.");
      console.error("Error fetching comparison data:", error);
    } finally {
      setIsLoading(false);
    }
  };

  // Function to fetch electricity data for all buildings
  const fetchAllBuildingsData = async () => {
    try {
      setIsLoading(true);
      setError(null);
      
      console.log("Fetching all buildings data with date range:", {
        startDate: startDate.toISOString(),
        endDate: endDate.toISOString()
      });
      
      // Call getMonthlyTotals, NOT filterElectrics
      const result = await getMonthlyTotals(startDate, endDate);
      console.log("All buildings data received:", result);
      
      // Transform the API data into the chart format
      const transformedData = processAllBuildingsData(result);
      setMonthlyData(transformedData);
    } catch (error) {
      setError("Failed to fetch all buildings data. Please try again later.");
      console.error("Error fetching all buildings data:", error);
    } finally {
      setIsLoading(false);
    }
  };
  const handleBuildingChange = (e) => {
    const buildingId = e.target.value;
    setSelectedBuildingId(buildingId);
    
    // Reset any previous errors
    setError(null);
  };

  // Process all buildings data from API into chart format
  const processAllBuildingsData = (data) => {
    // Get all months within the date range
    const months = [];
    const startYear = startDate.getFullYear();
    const startMonth = startDate.getMonth();
    const endYear = endDate.getFullYear();
    const endMonth = endDate.getMonth();
    
    // Generate all month-year combinations within range
    for (let year = startYear; year <= endYear; year++) {
      const monthStart = (year === startYear) ? startMonth : 0;
      const monthEnd = (year === endYear) ? endMonth : 11;
      
      for (let month = monthStart; month <= monthEnd; month++) {
        months.push({
          month: getMonthName(month),
          year: year,
          monthIndex: month,
          fullMonth: `${getMonthName(month)} ${year}`
        });
      }
    }
    
    // Initialize monthly usage with generated months
    const monthlyUsage = months.map((monthData) => {
      return {
        month: monthData.fullMonth,
        monthOnly: monthData.month,
        year: monthData.year,
        fullMonth: monthData.fullMonth,
        usage: 0
      };
    });
    
    // Update with actual data based on your API response format
    data.forEach(item => {
      // month is 1-based in the API response, monthIndex is 0-based in our data
      const month = item.month - 1;
      const year = item.year;
      
      // Find matching month in our array
      const dataIndex = months.findIndex(m => 
        m.monthIndex === month && m.year === year
      );
      
      if (dataIndex !== -1) {
        // Use totalKWHValue field from the API response
        monthlyUsage[dataIndex].usage = item.totalKWHValue;
      }
    });
    
    return monthlyUsage;
  };

  // Process electricity data from API into chart format
  const processElectricityData = (data) => {
    // Get all months within the date range
    const months = [];
    const startYear = startDate.getFullYear();
    const startMonth = startDate.getMonth();
    const endYear = endDate.getFullYear();
    const endMonth = endDate.getMonth();
    
    // Generate all month-year combinations within range
    for (let year = startYear; year <= endYear; year++) {
      const monthStart = (year === startYear) ? startMonth : 0;
      const monthEnd = (year === endYear) ? endMonth : 11;
      
      for (let month = monthStart; month <= monthEnd; month++) {
        months.push({
          month: getMonthName(month),
          year: year,
          monthIndex: month,
          // Full month-year format for display
          fullMonth: `${getMonthName(month)} ${year}`
        });
      }
    }
    
    // Initialize monthly usage with generated months
    const monthlyUsage = months.map((monthData) => {
      return {
        // Always show month and year for X-axis
        month: monthData.fullMonth,
        // For custom X-axis ticks if needed
        monthOnly: monthData.month,
        year: monthData.year,
        // Full label for tooltip
        fullMonth: monthData.fullMonth,
        usage: 0
      };
    });
    
    // Update with actual data
    data.forEach(record => {
      const date = new Date(record.date);
      // Skip records outside our date range
      if (date < startDate || date > endDate) return;
      
      const year = date.getFullYear();
      const month = date.getMonth();
      
      // Find matching month in our array
      const monthIndex = months.findIndex(m => 
        m.month === getMonthName(month) && m.year === year
      );
      
      if (monthIndex !== -1) {
        // Use kwhValue field from the API response
        monthlyUsage[monthIndex].usage += record.kwhValue;
      }
    });
    
    return monthlyUsage;
  };

  // Process comparison data from API into chart format
  const processComparisonData = (results, buildingIds) => {
    // Get all months within the date range
    const months = [];
    const startYear = startDate.getFullYear();
    const startMonth = startDate.getMonth();
    const endYear = endDate.getFullYear();
    const endMonth = endDate.getMonth();
    
    // Generate all month-year combinations within range
    for (let year = startYear; year <= endYear; year++) {
      const monthStart = (year === startYear) ? startMonth : 0;
      const monthEnd = (year === endYear) ? endMonth : 11;
      
      for (let month = monthStart; month <= monthEnd; month++) {
        months.push({
          month: getMonthName(month),
          year: year,
          monthIndex: month,
          // Full month-year format for display
          fullMonth: `${getMonthName(month)} ${year}`
        });
      }
    }
    
    // Initialize monthly data with generated months
    const monthlyData = months.map((monthData) => {
      return {
        // Always show month and year for X-axis
        month: monthData.fullMonth,
        // For custom X-axis ticks if needed
        monthOnly: monthData.month,
        year: monthData.year,
        // Full label for tooltip
        fullMonth: monthData.fullMonth
      };
    });
    
    // Process data for each building
    results.forEach((data, index) => {
      const buildingId = buildingIds[index];
      const building = buildings.find(b => b.id === buildingId);
      const buildingName = building ? building.name : data[0]?.buildingName || `Building ${index + 1}`;
      
      // Initialize usage to 0 for this building for all months
      monthlyData.forEach(item => {
        item[buildingName] = 0;
      });
      
      // Update with actual data
      data.forEach(record => {
        const date = new Date(record.date);
        // Skip records outside our date range
        if (date < startDate || date > endDate) return;
        
        const year = date.getFullYear();
        const month = date.getMonth();
        
        // Find matching month in our array
        const monthIndex = months.findIndex(m => 
          m.month === getMonthName(month) && m.year === year
        );
        
        if (monthIndex !== -1) {
          // Add to the existing value (in case there are multiple records for the same month)
          monthlyData[monthIndex][buildingName] += record.kwhValue;
        }
      });
    });
    
    return monthlyData;
  };

  // Helper function to get month name
  const getMonthName = (monthIndex) => {
    const months = [
      "Jan", "Feb", "Mar", "Apr", "May", "Jun", 
      "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    ];
    return months[monthIndex];
  };

  // Format date to YYYY-MM-DD format for date input
  const formatDateForInput = (date) => {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  };

  // Handle date selection
  const handleStartDateChange = (e) => {
    const newDate = new Date(e.target.value);
    if (newDate > endDate) {
      setStartDate(newDate);
      setEndDate(newDate);
    } else {
      setStartDate(newDate);
    }
  };
  
  const handleEndDateChange = (e) => {
    const newDate = new Date(e.target.value);
    if (newDate < startDate) {
      setEndDate(newDate);
      setStartDate(newDate);
    } else {
      setEndDate(newDate);
    }
  };

  // Handle adding a building to comparison
  const handleAddBuildingToComparison = (e) => {
    const buildingId = e.target.value;
    if (buildingId && !selectedBuildingIds.includes(buildingId)) {
      console.log("Adding building to comparison:", buildingId);
      setSelectedBuildingIds([...selectedBuildingIds, buildingId]);
    }
    e.target.value = ""; // Reset the select element
  };

  // Handle removing a building from comparison
  const handleRemoveBuildingFromComparison = (buildingId) => {
    console.log("Removing building from comparison:", buildingId);
    setSelectedBuildingIds(selectedBuildingIds.filter(id => id !== buildingId));
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
          <h2>Resource Monitoring - Electricity</h2>
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
          <>
            {/* Monthly Usage Chart Section */}
            <div style={{ 
              backgroundColor: "#fff", 
              padding: "1rem", 
              borderRadius: "4px", 
              boxShadow: "0 2px 4px rgba(0,0,0,0.1)",
              marginBottom: "1rem"
            }}>
              <h3>
                {selectedBuildingId === "all" 
                  ? "Total Electricity Usage Across All Buildings" 
                  : "Electricity Consumption Graphs"}
              </h3>
              
              {/* Controls - Building Selection and Date Range */}
              <div style={{ 
                marginBottom: "1.5rem", 
                display: "flex", 
                alignItems: "flex-end", 
                gap: "1.5rem", 
                flexWrap: "wrap"
              }}>
                <div>
                  <label htmlFor="buildingSelect" style={{ 
                    display: "block", 
                    marginBottom: "0.25rem", 
                    fontWeight: "500" 
                  }}>
                    Building Name:
                  </label>
                  <select 
                    id="buildingSelect"
                    value={selectedBuildingId}
                    onChange={handleBuildingChange}
                    style={{ 
                      padding: "0.5rem", 
                      borderRadius: "4px", 
                      border: "1px solid #ccc",
                      backgroundColor: "#f5f5f5",
                      minWidth: "250px"
                    }}
                  >
                    <option value="all">TÜM BİNALAR</option>
                    {buildings.map(building => (
                      <option key={building.id} value={building.id}>{building.name}</option>
                    ))}
                  </select>
                </div>
                
                {/* Date Range Pickers - Updated to use HTML5 date inputs */}
                <div>
                  <label htmlFor="startDatePicker" style={{ 
                    display: "block", 
                    marginBottom: "0.25rem", 
                    fontWeight: "500" 
                  }}>
                    Start Date:
                  </label>
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
                  <label htmlFor="endDatePicker" style={{ 
                    display: "block", 
                    marginBottom: "0.25rem", 
                    fontWeight: "500" 
                  }}>
                    End Date:
                  </label>
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
              
              {/* Monthly Usage Bar Chart */}
              <div style={{ height: "300px" }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart
                    data={monthlyData}
                    margin={{ top: 10, right: 30, left: 0, bottom: 30 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis 
                      dataKey="fullMonth" 
                      angle={-45}
                      textAnchor="end"
                      height={80}
                    />
                    <YAxis />
                    <Tooltip 
                      formatter={(value) => `${value.toFixed(2)} kWh`}
                      labelFormatter={(label) => {
                        return label; // Full month-year is already in the label
                      }}
                    />
                    <Legend />
                    <Bar 
                      dataKey="usage" 
                      fill="#3498db" 
                      name={selectedBuildingId === "all" 
                        ? "Total Electricity Usage (kWh)" 
                        : "Electricity Usage (kWh)"}
                    />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>
            
            {/* Building Comparison Chart Section */}
            <div style={{ 
              backgroundColor: "#fff", 
              padding: "1rem", 
              borderRadius: "4px", 
              boxShadow: "0 2px 4px rgba(0,0,0,0.1)"
            }}>
              <h3>Electricity Consumption Comparison By Buildings</h3>
              
              {/* Controls - Building Selection for Comparison */}
              <div style={{ marginBottom: "1rem", display: "flex", alignItems: "center", gap: "1rem", flexWrap: "wrap" }}>
                <div>
                  <label htmlFor="compareSelect" style={{ marginRight: "0.5rem" }}>Add Building for Comparison:</label>
                  <select 
                    id="compareSelect"
                    onChange={handleAddBuildingToComparison}
                    defaultValue=""
                    style={{ 
                      padding: "0.5rem", 
                      borderRadius: "4px", 
                      border: "1px solid #ccc",
                      backgroundColor: "#f5f5f5",
                      minWidth: "200px"
                    }}
                  >
                    <option value="" disabled>Select a building to add</option>
                    {buildings
                      .filter(building => !selectedBuildingIds.includes(building.id))
                      .map(building => (
                        <option key={building.id} value={building.id}>{building.name}</option>
                      ))
                    }
                  </select>
                </div>
                
                {/* Display current date range for reference */}
                  <div style={{ 
                  backgroundColor: "#f0f9ff",
                  padding: "0.5rem",
                  borderRadius: "4px",
                  border: "1px solid #d0e3ff",
                  fontSize: "0.9rem"
                }}>
                  Date Range: {formatDateForInput(startDate)} to {formatDateForInput(endDate)}
                </div>
              </div>
              
              {/* Selected Buildings Display */}
              <div style={{ 
                display: "flex", 
                flexWrap: "wrap", 
                gap: "0.5rem", 
                marginBottom: "1rem" 
              }}>
                {selectedBuildingIds.map((buildingId, index) => {
                  const building = buildings.find(b => b.id === buildingId);
                  const buildingName = building ? building.name : 'Unknown Building';
                  
                  return (
                    <div 
                      key={buildingId}
                      style={{
                        display: "flex",
                        alignItems: "center",
                        backgroundColor: "#f0f0f0",
                        padding: "0.3rem 0.6rem",
                        borderRadius: "4px",
                        border: `1px solid ${lineColors[index % lineColors.length]}`
                      }}
                    >
                      <span style={{ 
                        display: "inline-block", 
                        width: "10px", 
                        height: "10px", 
                        backgroundColor: lineColors[index % lineColors.length],
                        marginRight: "0.5rem",
                        borderRadius: "50%"
                      }}></span>
                      {buildingName}
                      <button
                        onClick={() => handleRemoveBuildingFromComparison(buildingId)}
                        style={{
                          background: "none",
                          border: "none",
                          color: "#777",
                          cursor: "pointer",
                          marginLeft: "0.5rem",
                          fontSize: "1rem",
                          padding: "0 0.3rem"
                        }}
                      >
                        ×
                      </button>
                    </div>
                  );
                })}

                {selectedBuildingIds.length === 0 && (
                  <div style={{ color: "#666", fontSize: "0.9rem" }}>
                    No buildings selected for comparison. Please add buildings using the dropdown above.
                  </div>
                )}
              </div>

              {/* Comparison Line Chart */}
              <div style={{ height: "300px" }}>
                <ResponsiveContainer width="100%" height="100%">
                  <LineChart
                    data={compareData}
                    margin={{ top: 10, right: 30, left: 0, bottom: 30 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis 
                      dataKey="fullMonth" 
                      angle={-45}
                      textAnchor="end"
                      height={80}
                    />
                    <YAxis />
                    <Tooltip 
                      formatter={(value) => `${value.toFixed(2)} kWh`}
                      labelFormatter={(label) => {
                        return label; // Full month-year is already in the label
                      }}
                    />
                    <Legend />
                    {buildings
                      .filter(building => selectedBuildingIds.includes(building.id))
                      .map((building, index) => (
                        <Line
                          key={building.id}
                          type="monotone"
                          dataKey={building.name}
                          stroke={lineColors[index % lineColors.length]}
                          activeDot={{ r: 8 }}
                        />
                      ))}
                  </LineChart>
                </ResponsiveContainer>
              </div>
            </div>
          </>
        )}
      </div>
    </div>
  );
};

export default ElectricityPage;