import React, { useState, useEffect } from "react";
import Sidebar from "../components/Sidebar";
import { 
  PieChart, Pie, Cell, Legend, ResponsiveContainer, Tooltip,
  BarChart, Bar, XAxis, YAxis, CartesianGrid
} from 'recharts';
import { calculateCarbonFootprint, getHistoricalFootprint } from "../services/carbonFootprintService";

const CarbonFootprintCalculations = () => {
  // State variables
  const [formData, setFormData] = useState({
    year: new Date().getFullYear(),
    electricityFactor: 0.5,
    shuttleBusFactor: 0.3,
    carFactor: 0.25,
    motorcycleFactor: 0.12
  });
  
  const [resultsData, setResultsData] = useState(null);
  const [historicalData, setHistoricalData] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [isSubmitted, setIsSubmitted] = useState(false);
  
  // Colors for pie chart
  const COLORS = ['#0088FE', '#00C49F', '#FFBB28', '#FF8042', '#8884d8'];
  
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
  { key: "userManagement", name: "User Management" }, // 👈 NEW ITEM
  { key: "adminTools", name: "Admin Tools" },         // 👈 KEPT ORIGINAL
  { key: "reports", name: "Reports" }
];

  // Fetch historical data when results are submitted
  useEffect(() => {
    if (isSubmitted) {
      fetchHistoricalData();
    }
  }, [
    isSubmitted, 
    formData.electricityFactor, 
    formData.shuttleBusFactor, 
    formData.carFactor, 
    formData.motorcycleFactor
  ]);

  // Handle form input changes
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: name === 'year' ? parseInt(value) : parseFloat(value)
    });
  };

 // Fetch historical carbon footprint data
  const fetchHistoricalData = async () => {
    try {
      setIsLoading(true);
      
      const factors = {
        electricityFactor: formData.electricityFactor,
        shuttleBusFactor: formData.shuttleBusFactor,
        carFactor: formData.carFactor,
        motorcycleFactor: formData.motorcycleFactor
      };
      
      console.log("Fetching historical data with factors:", factors);
      
      // Call API to get historical data
      const historicalData = await getHistoricalFootprint(factors);
      console.log("Raw historical data from API:", historicalData);
      
      // Check if we received valid data
      if (!historicalData || !Array.isArray(historicalData)) {
        console.error("Invalid historical data format:", historicalData);
        return;
      }
      
      // Process the data for the chart
      const processedData = processHistoricalData(historicalData);
      console.log("Processed historical data for chart:", processedData);
      
      setHistoricalData(processedData);
    } catch (error) {
      console.error("Error fetching historical carbon footprint data:", error);
      // Don't show error for historical data, just log it
    } finally {
      setIsLoading(false);
    }
  };

  // Process historical data for the chart
  const processHistoricalData = (data) => {
    const currentYear = new Date().getFullYear();
    const startYear = currentYear - 4;
    
    // Group data by year
    const yearGroups = {};
    
    // Initialize year groups for the past 5 years (including current year)
    for (let year = startYear; year <= currentYear; year++) {
      yearGroups[year] = {
        year,
        electricity: 0,
        shuttleBus: 0,
        car: 0,
        motorcycle: 0,
        total: 0
      };
    }
    
    // Process the API data
    if (Array.isArray(data)) {
      data.forEach(item => {
        // Use the year field directly from the item
        const year = item.year;
        
        // Skip if the year is outside our range
        if (year < startYear || year > currentYear) return;
        
        // Add to the appropriate year group
        if (yearGroups[year]) {
          yearGroups[year].electricity = item.electricityEmission || 0;
          yearGroups[year].shuttleBus = item.shuttleBusEmission || 0;
          yearGroups[year].car = item.carEmission || 0;
          yearGroups[year].motorcycle = item.motorcycleEmission || 0;
          yearGroups[year].total = item.totalEmission || 0;
        }
      });
    }
    
    // Convert to array and sort by year
    return Object.values(yearGroups).sort((a, b) => a.year - b.year);
  };

  // Handle form submission
  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);
    
    try {
      // Prepare data for API call
      const factors = {
        electricityFactor: formData.electricityFactor,
        shuttleBusFactor: formData.shuttleBusFactor,
        carFactor: formData.carFactor,
        motorcycleFactor: formData.motorcycleFactor
      };
      
      console.log("Calculating carbon footprint with factors:", factors);
      
      // Call API to calculate emissions
      const results = await calculateCarbonFootprint(formData.year, factors);
      console.log("Calculation results:", results);
      
      // Process results for display
      setResultsData(results);
      setIsSubmitted(true);
    } catch (error) {
      setError("Failed to calculate carbon footprint. Please check your inputs and try again.");
      console.error("Error calculating carbon footprint:", error);
    } finally {
      setIsLoading(false);
    }
  };

  // Load existing emissions data for a year
  const handleLoadYear = async () => {
    setIsLoading(true);
    setError(null);
    
    try {
      console.log("Fetching carbon footprint data for year:", formData.year);
      
      // Use the same calculation endpoint with default factors
      const defaultFactors = {
        electricityFactor: 1,
        shuttleBusFactor: 1,
        carFactor: 1,
        motorcycleFactor: 1
      };
      
      // Call API to get carbon footprint for selected year
      const results = await calculateCarbonFootprint(formData.year, defaultFactors);
      console.log("Year data results:", results);
      
      // Process results for display
      setResultsData(results);
      setIsSubmitted(true);
    } catch (error) {
      setError("Failed to load carbon footprint data for the selected year.");
      console.error("Error loading carbon footprint data:", error);
    } finally {
      setIsLoading(false);
    }
  };

  // Reset form and results
  const handleReset = () => {
    setFormData({
      year: new Date().getFullYear(),
      electricityFactor: 0.5,
      shuttleBusFactor: 0.3,
      carFactor: 0.25,
      motorcycleFactor: 0.12
    });
    setResultsData(null);
    setIsSubmitted(false);
    setError(null);
    setHistoricalData([]);
  };

  // Prepare data for pie chart
  const preparePieChartData = () => {
    if (!resultsData) return [];
    
    return [
      { name: 'Electricity', value: resultsData.electricityEmission },
      { name: 'Shuttle Bus', value: resultsData.shuttleBusEmission },
      { name: 'Car', value: resultsData.carEmission },
      { name: 'Motorcycle', value: resultsData.motorcycleEmission }
    ];
  };

  // Custom tooltip for pie chart
  const CustomTooltip = ({ active, payload, label }) => {
    if (active && payload && payload.length) {
      return (
        <div style={{ 
          backgroundColor: '#fff', 
          padding: '10px', 
          border: '1px solid #ccc',
          borderRadius: '4px' 
        }}>
          <p style={{ margin: 0 }}><strong>{payload[0].name}</strong></p>
          <p style={{ margin: 0 }}>{`Emissions: ${payload[0].value.toFixed(2)} kg CO₂e`}</p>
          <p style={{ margin: 0 }}>{`Percentage: ${(payload[0].value / resultsData.totalEmission * 100).toFixed(2)}%`}</p>
        </div>
      );
    }
    return null;
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
          <h2>Carbon Footprint - Calculations</h2>
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
            <p>Processing...</p>
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
        <div style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
          {/* Calculation Form */}
          <div style={{ 
            backgroundColor: "#fff", 
            padding: "1rem", 
            borderRadius: "4px", 
            boxShadow: "0 2px 4px rgba(0,0,0,0.1)"
          }}>
            <h3>Emission Factors</h3>
            <form onSubmit={handleSubmit}>
              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "1rem" }}>
                {/* Year Input */}
                <div>
                  <label htmlFor="year" style={{ display: "block", marginBottom: "0.5rem", fontWeight: "bold" }}>
                    Year <span style={{ color: "red" }}>*</span>
                  </label>
                  <input
                    type="number"
                    id="year"
                    name="year"
                    value={formData.year}
                    onChange={handleInputChange}
                    required
                    min="2000"
                    max="2100"
                    style={{ 
                      width: "100%", 
                      padding: "0.5rem", 
                      borderRadius: "4px", 
                      border: "1px solid #ccc" 
                    }}
                  />
                </div>
                
                {/* Electricity Factor */}
                <div>
                  <label htmlFor="electricityFactor" style={{ display: "block", marginBottom: "0.5rem", fontWeight: "bold" }}>
                    Electricity Factor
                  </label>
                  <input
                    type="number"
                    id="electricityFactor"
                    name="electricityFactor"
                    value={formData.electricityFactor}
                    onChange={handleInputChange}
                    step="0.01"
                    min="0"
                    style={{ 
                      width: "100%", 
                      padding: "0.5rem", 
                      borderRadius: "4px", 
                      border: "1px solid #ccc" 
                    }}
                  />
                </div>
                
                {/* Shuttle Bus Factor */}
                <div>
                  <label htmlFor="shuttleBusFactor" style={{ display: "block", marginBottom: "0.5rem", fontWeight: "bold" }}>
                    Shuttle Bus Factor
                  </label>
                  <input
                    type="number"
                    id="shuttleBusFactor"
                    name="shuttleBusFactor"
                    value={formData.shuttleBusFactor}
                    onChange={handleInputChange}
                    step="0.01"
                    min="0"
                    style={{ 
                      width: "100%", 
                      padding: "0.5rem", 
                      borderRadius: "4px", 
                      border: "1px solid #ccc" 
                    }}
                  />
                </div>
                
                {/* Car Factor */}
                <div>
                  <label htmlFor="carFactor" style={{ display: "block", marginBottom: "0.5rem", fontWeight: "bold" }}>
                    Car Factor
                  </label>
                  <input
                    type="number"
                    id="carFactor"
                    name="carFactor"
                    value={formData.carFactor}
                    onChange={handleInputChange}
                    step="0.01"
                    min="0"
                    style={{ 
                      width: "100%", 
                      padding: "0.5rem", 
                      borderRadius: "4px", 
                      border: "1px solid #ccc" 
                    }}
                  />
                </div>
                
                {/* Motorcycle Factor */}
                <div>
                  <label htmlFor="motorcycleFactor" style={{ display: "block", marginBottom: "0.5rem", fontWeight: "bold" }}>
                    Motorcycle Factor
                  </label>
                  <input
                    type="number"
                    id="motorcycleFactor"
                    name="motorcycleFactor"
                    value={formData.motorcycleFactor}
                    onChange={handleInputChange}
                    step="0.01"
                    min="0"
                    style={{ 
                      width: "100%", 
                      padding: "0.5rem", 
                      borderRadius: "4px", 
                      border: "1px solid #ccc" 
                    }}
                  />
                </div>
              </div>
              
              {/* Form Buttons */}
              <div style={{ marginTop: "1.5rem", display: "flex", gap: "1rem" }}>
                <button
                  type="submit"
                  disabled={isLoading}
                  style={{
                    padding: "0.5rem 1rem",
                    backgroundColor: "#3498db",
                    color: "white",
                    border: "none",
                    borderRadius: "4px",
                    cursor: isLoading ? "not-allowed" : "pointer"
                  }}
                >
                  Calculate Carbon Footprint
                </button>
                
                <button
                  type="button"
                  onClick={handleLoadYear}
                  disabled={isLoading}
                  style={{
                    padding: "0.5rem 1rem",
                    backgroundColor: "#27ae60",
                    color: "white",
                    border: "none",
                    borderRadius: "4px",
                    cursor: isLoading ? "not-allowed" : "pointer"
                  }}
                >
                  Use Default Factors
                </button>
                
                <button
                  type="button"
                  onClick={handleReset}
                  disabled={isLoading}
                  style={{
                    padding: "0.5rem 1rem",
                    backgroundColor: "#e74c3c",
                    color: "white",
                    border: "none",
                    borderRadius: "4px",
                    cursor: isLoading ? "not-allowed" : "pointer"
                  }}
                >
                  Reset
                </button>
              </div>
            </form>
          </div>
          
          {/* Results Section */}
          {isSubmitted && resultsData && (
            <div style={{ 
              backgroundColor: "#fff", 
              padding: "1rem", 
              borderRadius: "4px", 
              boxShadow: "0 2px 4px rgba(0,0,0,0.1)"
            }}>
              <h3>Emission Results for {resultsData.year}</h3>
              
              {/* Results Summary */}
              <div style={{ 
                display: "grid", 
                gridTemplateColumns: "repeat(auto-fill, minmax(200px, 1fr))", 
                gap: "1rem",
                marginBottom: "1.5rem"
              }}>
                <div style={{ border: "1px solid #ccc", padding: "1rem", borderRadius: "4px" }}>
                  <h4 style={{ margin: "0 0 0.5rem 0", color: "#0088FE" }}>Electricity</h4>
                  <p style={{ fontSize: "1.2rem", fontWeight: "bold", margin: 0 }}>
                    {resultsData.electricityEmission.toFixed(2)} <small>kg CO₂e</small>
                  </p>
                </div>
                
                <div style={{ border: "1px solid #ccc", padding: "1rem", borderRadius: "4px" }}>
                  <h4 style={{ margin: "0 0 0.5rem 0", color: "#00C49F" }}>Shuttle Bus</h4>
                  <p style={{ fontSize: "1.2rem", fontWeight: "bold", margin: 0 }}>
                    {resultsData.shuttleBusEmission.toFixed(2)} <small>kg CO₂e</small>
                  </p>
                </div>
                
                <div style={{ border: "1px solid #ccc", padding: "1rem", borderRadius: "4px" }}>
                  <h4 style={{ margin: "0 0 0.5rem 0", color: "#FFBB28" }}>Car</h4>
                  <p style={{ fontSize: "1.2rem", fontWeight: "bold", margin: 0 }}>
                    {resultsData.carEmission.toFixed(2)} <small>kg CO₂e</small>
                  </p>
                </div>
                
                <div style={{ border: "1px solid #ccc", padding: "1rem", borderRadius: "4px" }}>
                  <h4 style={{ margin: "0 0 0.5rem 0", color: "#FF8042" }}>Motorcycle</h4>
                  <p style={{ fontSize: "1.2rem", fontWeight: "bold", margin: 0 }}>
                    {resultsData.motorcycleEmission.toFixed(2)} <small>kg CO₂e</small>
                  </p>
                </div>
                
                <div style={{ border: "1px solid #3498db", padding: "1rem", borderRadius: "4px", backgroundColor: "#f8f9fa" }}>
                  <h4 style={{ margin: "0 0 0.5rem 0", color: "#3498db" }}>Total Emissions</h4>
                  <p style={{ fontSize: "1.5rem", fontWeight: "bold", margin: 0 }}>
                    {resultsData.totalEmission.toFixed(2)} <small>kg CO₂e</small>
                  </p>
                </div>
              </div>
              
              {/* Pie Chart */}
              <div style={{ height: "400px" }}>
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={preparePieChartData()}
                      dataKey="value"
                      nameKey="name"
                      cx="50%"
                      cy="50%"
                      outerRadius={150}
                      fill="#8884d8"
                      label={({ name, percent }) => `${name}: ${(percent * 100).toFixed(0)}%`}
                    >
                      {preparePieChartData().map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip content={<CustomTooltip />} />
                    <Legend />
                  </PieChart>
                </ResponsiveContainer>
              </div>
            </div>
          )}
          
          {/* Historical Data Chart */}
          {isSubmitted && historicalData.length > 0 && (
            <div style={{ 
              backgroundColor: "#fff", 
              padding: "1rem", 
              borderRadius: "4px", 
              boxShadow: "0 2px 4px rgba(0,0,0,0.1)"
            }}>
              <h3>Historical Carbon Footprint (Last 5 Years)</h3>
              
              <div style={{ height: "400px" }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart
                    data={historicalData}
                    margin={{ top: 20, right: 30, left: 20, bottom: 30 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis dataKey="year" />
                    <YAxis 
                      label={{ 
                        value: 'Carbon Emissions (kg CO₂e)', 
                        angle: -90, 
                        position: 'insideLeft',
                        style: { textAnchor: 'middle' }
                      }} 
                    />
                    <Tooltip formatter={(value) => `${value.toFixed(2)} kg CO₂e`} />
                    <Legend />
                    <Bar dataKey="electricity" name="Electricity" fill="#0088FE" stackId="a" />
                    <Bar dataKey="shuttleBus" name="Shuttle Bus" fill="#00C49F" stackId="a" />
                    <Bar dataKey="car" name="Car" fill="#FFBB28" stackId="a" />
                    <Bar dataKey="motorcycle" name="Motorcycle" fill="#FF8042" stackId="a" />
                  </BarChart>
                </ResponsiveContainer>
              </div>
              
              <div style={{ marginTop: "1rem", fontSize: "0.9rem", color: "#666", textAlign: "center" }}>
                <p>This chart shows the total carbon footprint breakdown for the past 5 years.</p>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default CarbonFootprintCalculations;