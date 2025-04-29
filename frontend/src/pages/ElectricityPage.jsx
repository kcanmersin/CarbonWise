import React, { useState, useEffect } from "react";
import Sidebar from "../components/Sidebar";
import { 
  BarChart, Bar, LineChart, Line, XAxis, YAxis, CartesianGrid, 
  Tooltip, Legend, ResponsiveContainer 
} from 'recharts';
import { getBuildings } from "../services/buildingService";
import { filterElectrics } from "../services/electricityService";

const ElectricityPage = () => {
  // State variables
  const [buildings, setBuildings] = useState([]);
  const [selectedBuildingId, setSelectedBuildingId] = useState("");
  const [selectedBuildingIds, setSelectedBuildingIds] = useState([]);
  const [selectedYear, setSelectedYear] = useState(new Date().getFullYear());
  const [monthlyData, setMonthlyData] = useState([]);
  const [compareData, setCompareData] = useState([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);
  
  // Available years for selection
  const availableYears = [2022, 2023, 2024, 2025];
  
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

  // Fetch electricity data when selected building or year changes
  useEffect(() => {
    if (selectedBuildingId) {
      console.log("Selected building ID:", selectedBuildingId);
      console.log("Selected year:", selectedYear);
      fetchElectricityData(selectedBuildingId, selectedYear);
    }
  }, [selectedBuildingId, selectedYear]);

  // Fetch comparison data when selected buildings or year changes
  useEffect(() => {
    if (selectedBuildingIds.length > 0) {
      console.log("Selected building IDs for comparison:", selectedBuildingIds);
      console.log("Selected year for comparison:", selectedYear);
      fetchComparisonData(selectedYear);
    }
  }, [selectedBuildingIds, selectedYear]);

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

  // Function to fetch electricity data for a specific building and year
  const fetchElectricityData = async (buildingId, year) => {
    try {
      setIsLoading(true);
      setError(null);
      
      // Set date range for the selected year
      const startDate = new Date(year, 0, 1);  // January 1st of selected year
      const endDate = new Date(year, 11, 31);  // December 31st of selected year
      
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

  // Function to fetch comparison data for multiple buildings and year
  const fetchComparisonData = async (year) => {
    try {
      setIsLoading(true);
      setError(null);
      
      // Set date range for the selected year
      const startDate = new Date(year, 0, 1);  // January 1st of selected year
      const endDate = new Date(year, 11, 31);  // December 31st of selected year
      
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

  // Process electricity data from API into chart format
  const processElectricityData = (data) => {
    // Initialize array for all months
    const months = [
      "Jan", "Feb", "Mar", "Apr", "May", "Jun", 
      "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    ];
    
    const monthlyUsage = months.map(month => ({
      month: month,
      usage: 0
    }));
    
    // Update with actual data
    data.forEach(record => {
      const date = new Date(record.date);
      const monthIndex = date.getMonth();
      
      // Use kwhValue field from the API response
      monthlyUsage[monthIndex].usage += record.kwhValue;
    });
    
    return monthlyUsage;
  };

  // Process comparison data from API into chart format
  const processComparisonData = (results, buildingIds) => {
    // Initialize array for all months
    const months = [
      "Jan", "Feb", "Mar", "Apr", "May", "Jun", 
      "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    ];
    
    const monthlyData = months.map(month => ({
      month: month
    }));
    
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
        const monthIndex = date.getMonth();
        
        // Add to the existing value (in case there are multiple records for the same month)
        monthlyData[monthIndex][buildingName] += record.kwhValue;
      });
    });
    
    return monthlyData;
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
              <h3>Electricity Usage Monthly/Yearly Graphs</h3>
              
              {/* Controls - Building and Year Selection */}
              <div style={{ marginBottom: "1rem", display: "flex", alignItems: "center", gap: "1rem", flexWrap: "wrap" }}>
                <div>
                  <label htmlFor="buildingSelect" style={{ marginRight: "0.5rem" }}>Building Name:</label>
                  <select 
                    id="buildingSelect"
                    value={selectedBuildingId}
                    onChange={(e) => setSelectedBuildingId(e.target.value)}
                    style={{ 
                      padding: "0.5rem", 
                      borderRadius: "4px", 
                      border: "1px solid #ccc",
                      backgroundColor: "#eee",
                      minWidth: "200px"
                    }}
                  >
                    {buildings.map(building => (
                      <option key={building.id} value={building.id}>{building.name}</option>
                    ))}
                  </select>
                </div>
                
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
              <div style={{ height: "300px" }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart
                    data={monthlyData}
                    margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="month" />
                    <YAxis />
                    <Tooltip formatter={(value) => `${value.toFixed(2)} kWh`} />
                    <Legend />
                    <Bar 
                      dataKey="usage" 
                      fill="#3498db" 
                      name={`Electricity Usage ${selectedYear} (kWh)`}
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
              <h3>Electric Usage Comparison By Buildings</h3>
              
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
                      backgroundColor: "#eee",
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
                
                <div>
                  <label htmlFor="compareYearSelect" style={{ marginRight: "0.5rem" }}>Year:</label>
                  <select 
                    id="compareYearSelect"
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
                    margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="month" />
                    <YAxis />
                    <Tooltip formatter={(value) => `${value.toFixed(2)} kWh`} />
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