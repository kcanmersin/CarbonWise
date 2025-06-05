import React, { useState, useEffect } from "react";
import Sidebar from "../components/Sidebar";
import { getConsumptionTypes } from "../services/reportsPDFService";
import { getBuildings } from "../services/buildingService";
import { generateCarbonFootprintPdfReport, generateConsumptionPdfReport } from "../services/reportsPDFService";
import { exportConsumptionData } from "../services/consumptionDataService";

const ReportsPage = () => {
  // State variables
  const [reportType, setReportType] = useState("consumption");
  const [exportFormat, setExportFormat] = useState("pdf");
  const [buildings, setBuildings] = useState([]);
  const [selectedBuildingId, setSelectedBuildingId] = useState("");
  const [selectedConsumptionType, setSelectedConsumptionType] = useState("");
  const [startDate, setStartDate] = useState("2024-01-01T00:00:00.000Z"); // default start date
  const [endDate, setEndDate] = useState("2024-12-31T23:59:59.999Z"); // default end date
  const [includeGraphs, setIncludeGraphs] = useState(true);
  const [consumptionTypes, setConsumptionTypes] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  const availableYears = [2022, 2023, 2024, 2025];

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

  // Fetch buildings and consumption types on component mount
  useEffect(() => {
    const fetchData = async () => {
      try {
        const consumptionTypesData = await getConsumptionTypes();
        setConsumptionTypes(consumptionTypesData);
        
        if (consumptionTypesData.length > 0) {
          setSelectedConsumptionType(consumptionTypesData[0]);
        }
        
        // Fetch buildings
        const buildingsData = await getBuildings();
        setBuildings(buildingsData);

        if (buildingsData.length > 0) {
          setSelectedBuildingId(buildingsData[0].id);
        }
      } catch (error) {
        setError("Failed to fetch data. Please try again later.");
      }
    };

    fetchData();
  }, []);

  const handleStartDateChange = (e) => {
    setStartDate(e.target.value);
  };

  const handleEndDateChange = (e) => {
    setEndDate(e.target.value);
  };

  const handleYearChange = (e) => {
    // Adjust the date range based on selected year
    const year = parseInt(e.target.value);
    setStartDate(`${year}-01-01T00:00:00.000Z`);
    setEndDate(`${year}-12-31T23:59:59.999Z`);
  };

  const handleConsumptionTypeChange = (e) => {
    setSelectedConsumptionType(e.target.value);
  };

  const handleReportTypeChange = (e) => {
    setReportType(e.target.value);
    
    // Reset export format when switching report types
    if (e.target.value === "carbonFootprint") {
      setExportFormat("pdf"); // Carbon footprint is only available as PDF
    }
  };

  const handleExportFormatChange = (e) => {
    setExportFormat(e.target.value);
  };

  const handleIncludeGraphsChange = (e) => {
    setIncludeGraphs(e.target.checked);
  };

  const handleDownloadReport = async () => {
    try {
      setIsLoading(true);
      
      // Format dates to ensure they are valid ISO strings
      const formattedStartDate = new Date(startDate).toISOString();
      const formattedEndDate = new Date(endDate).toISOString();
      
      if (reportType === "consumption") {
        if (exportFormat === "pdf") {
          // ALL consumption types now require building selection (or allow "All Buildings")
          let buildingIdToSend = selectedBuildingId || null;
          
          // Generate consumption PDF report
          const pdfBlob = await generateConsumptionPdfReport(
            selectedConsumptionType,
            buildingIdToSend,
            formattedStartDate,
            formattedEndDate
          );

          // Create download link
          const startDateStr = formattedStartDate.slice(0,10).replace(/-/g, "");
          const endDateStr = formattedEndDate.slice(0,10).replace(/-/g, "");
          const buildingStr = buildingIdToSend ? `_${buildingIdToSend.substring(0,8)}` : "_AllBuildings";
          
          const fileName = `${selectedConsumptionType}ConsumptionReport${buildingStr}_${startDateStr}-${endDateStr}.pdf`;
          
          const link = document.createElement("a");
          link.href = URL.createObjectURL(pdfBlob);
          link.download = fileName;
          link.click();
        } else if (exportFormat === "excel") {
          // Export consumption data as Excel - now includes building context
          const excelBlob = await exportConsumptionData(
            selectedConsumptionType,
            formattedStartDate,
            formattedEndDate,
            includeGraphs,
            selectedBuildingId // Add building ID for Excel export too
          );
          
          // Create download link
          const startDateStr = formattedStartDate.slice(0,10).replace(/-/g, "");
          const endDateStr = formattedEndDate.slice(0,10).replace(/-/g, "");
          const buildingStr = selectedBuildingId ? `_${selectedBuildingId.substring(0,8)}` : "_AllBuildings";
          
          const fileName = `${selectedConsumptionType}_ConsumptionData${buildingStr}_${startDateStr}-${endDateStr}.xlsx`;
          
          const link = document.createElement("a");
          link.href = URL.createObjectURL(excelBlob);
          link.download = fileName;
          link.click();
        }
      } else {
        // Generate carbon footprint PDF report
        const pdfBlob = await generateCarbonFootprintPdfReport(
          formattedStartDate,
          formattedEndDate
        );
        
        // Create download link
        const startDateStr = formattedStartDate.slice(0,10).replace(/-/g, "");
        const endDateStr = formattedEndDate.slice(0,10).replace(/-/g, "");
        const fileName = `CarbonFootprintReport_${startDateStr}-${endDateStr}.pdf`;
        
        const link = document.createElement("a");
        link.href = URL.createObjectURL(pdfBlob);
        link.download = fileName;
        link.click();
      }
    } catch (error) {
      setError(`Failed to generate ${reportType} report: ${error.message}`);
    } finally {
      setIsLoading(false);
    }
  };

  // Updated: Building selection should be shown for ALL consumption types
  const shouldShowBuildingSelect = () => {
    return reportType === "consumption";
  };

  // Get building-specific consumption types that require building selection
  const getBuildingRequiredTypes = () => {
    return ["Electric", "NaturalGas", "Water", "Paper"]; // All types now require buildings
  };

  // Get user-friendly consumption type labels
  const getConsumptionTypeLabel = (type) => {
    const labels = {
      "Electric": "Electricity",
      "NaturalGas": "Natural Gas", 
      "Water": "Water",
      "Paper": "Paper"
    };
    return labels[type] || type;
  };

  const formatInputLabel = (text) => {
    return {
      fontWeight: "600",
      marginBottom: "0.5rem",
      display: "block"
    };
  };

  const inputStyle = {
    padding: "0.5rem",
    borderRadius: "4px",
    border: "1px solid #ccc",
    backgroundColor: "#eee",
    width: "100%",
    marginBottom: "1rem"
  };

  const checkboxStyle = {
    marginRight: "0.5rem"
  };

  return (
    <div style={{ display: "flex", height: "100vh" }}>
      <Sidebar menuItems={menuItems} />
      <div style={{ flex: 1, padding: "1rem", backgroundColor: "#f5f5f5", overflowY: "auto" }}>
        <div style={{ backgroundColor: "#333", color: "#fff", padding: "0.5rem 1rem", marginBottom: "1rem" }}>
          <h2>Reports</h2>
        </div>

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
            <p>Generating report...</p>
          </div>
        )}

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

        <div style={{ 
          backgroundColor: "#fff", 
          padding: "1.5rem", 
          borderRadius: "8px", 
          boxShadow: "0 4px 6px rgba(0,0,0,0.1)", 
          marginBottom: "1rem",
          maxWidth: "700px",
          margin: "0 auto"
        }}>
          <h3 style={{ 
            marginBottom: "1.5rem", 
            textAlign: "center",
            color: "#2c3e50",
            fontSize: "1.5rem"
          }}>
            📊 Generate Reports
          </h3>
          
          {/* Report Type Selection */}
          <div style={{ marginBottom: "1.5rem" }}>
            <label style={formatInputLabel()} htmlFor="reportTypeSelect">
              📄 Report Type:
            </label>
            <select 
              id="reportTypeSelect"
              value={reportType}
              onChange={handleReportTypeChange}
              style={inputStyle}
            >
              <option value="consumption">📈 Consumption Report</option>
              <option value="carbonFootprint">🌱 Carbon Footprint Report</option>
            </select>
          </div>

          {/* Export Format Selection (only for consumption reports) */}
          {reportType === "consumption" && (
            <div style={{ marginBottom: "1.5rem" }}>
              <label style={formatInputLabel()} htmlFor="exportFormatSelect">
                💾 Export Format:
              </label>
              <select 
                id="exportFormatSelect"
                value={exportFormat}
                onChange={handleExportFormatChange}
                style={inputStyle}
              >
                <option value="pdf">📑 PDF Report</option>
                <option value="excel">📊 Excel Data</option>
              </select>
            </div>
          )}

          {/* Consumption Type Selection */}
          {reportType === "consumption" && (
            <div style={{ marginBottom: "1.5rem" }}>
              <label style={formatInputLabel()} htmlFor="consumptionSelect">
                ⚡ Resource Type:
              </label>
              <select 
                id="consumptionSelect"
                value={selectedConsumptionType}
                onChange={handleConsumptionTypeChange}
                style={inputStyle}
              >
                {consumptionTypes.map((type) => (
                  <option key={type} value={type}>
                    {getConsumptionTypeLabel(type)}
                  </option>
                ))}
              </select>
            </div>
          )}

          {/* Building Selection - Now shown for ALL consumption types */}
          {shouldShowBuildingSelect() && (
            <div style={{ marginBottom: "1.5rem" }}>
              <label style={formatInputLabel()} htmlFor="buildingSelect">
                🏢 Building:
              </label>
              <select 
                id="buildingSelect"
                value={selectedBuildingId}
                onChange={(e) => setSelectedBuildingId(e.target.value)}
                style={inputStyle}
              >
                <option value="">🏫 All Buildings</option>
                {buildings.map(building => (
                  <option key={building.id} value={building.id}>
                    {building.name}
                  </option>
                ))}
              </select>
              <small style={{ color: "#6c757d", fontSize: "0.85rem" }}>
                Select a specific building or choose "All Buildings" for combined data
              </small>
            </div>
          )}

          {/* Year Selection */}
          <div style={{ marginBottom: "1.5rem" }}>
            <label style={formatInputLabel()} htmlFor="yearSelect">
              📅 Year:
            </label>
            <select 
              id="yearSelect"
              onChange={handleYearChange}
              style={inputStyle}
              defaultValue={2024}
            >
              {availableYears.map(year => (
                <option key={year} value={year}>{year}</option>
              ))}
            </select>
          </div>

          {/* Date Range Selection */}
          <div style={{ 
            display: "grid", 
            gridTemplateColumns: "1fr 1fr", 
            gap: "1rem",
            marginBottom: "1.5rem"
          }}>
            <div>
              <label style={formatInputLabel()} htmlFor="startDate">
                📅 Start Date:
              </label>
              <input 
                type="datetime-local"
                id="startDate"
                value={startDate.slice(0, 19)}
                onChange={handleStartDateChange}
                style={inputStyle}
              />
            </div>

            <div>
              <label style={formatInputLabel()} htmlFor="endDate">
                📅 End Date:
              </label>
              <input 
                type="datetime-local"
                id="endDate"
                value={endDate.slice(0, 19)}
                onChange={handleEndDateChange}
                style={inputStyle}
              />
            </div>
          </div>

          {/* Include Graphs Option (for Excel export) */}
          {reportType === "consumption" && exportFormat === "excel" && (
            <div style={{ 
              marginBottom: "1.5rem",
              padding: "1rem",
              backgroundColor: "#f8f9fa",
              borderRadius: "6px",
              border: "1px solid #e9ecef"
            }}>
              <label style={{ 
                display: "flex", 
                alignItems: "center", 
                cursor: "pointer",
                fontWeight: "500"
              }}>
                <input 
                  type="checkbox"
                  checked={includeGraphs}
                  onChange={handleIncludeGraphsChange}
                  style={{
                    ...checkboxStyle,
                    width: "18px",
                    height: "18px"
                  }}
                />
                📊 Include graphs in Excel export
              </label>
              <small style={{ color: "#6c757d", fontSize: "0.85rem", marginLeft: "24px" }}>
                Add visual charts to the Excel file for better data analysis
              </small>
            </div>
          )}

          {/* Download Button */}
          <button
            onClick={handleDownloadReport}
            disabled={isLoading}
            style={{
              padding: "1rem 1.5rem",
              backgroundColor: (() => {
                if (isLoading) return "#94a3b8";
                if (reportType === "consumption") {
                  return exportFormat === "pdf" ? "#10b981" : "#f59e0b";
                }
                return "#3b82f6";
              })(),
              color: "#fff",
              border: "none",
              borderRadius: "8px",
              cursor: isLoading ? "not-allowed" : "pointer",
              width: "100%",
              fontWeight: "600",
              fontSize: "1rem",
              transition: "all 0.3s ease",
              boxShadow: isLoading ? "none" : "0 4px 6px rgba(0,0,0,0.1)"
            }}
            onMouseEnter={(e) => {
              if (!isLoading) {
                e.target.style.transform = "translateY(-2px)";
                e.target.style.boxShadow = "0 6px 12px rgba(0,0,0,0.15)";
              }
            }}
            onMouseLeave={(e) => {
              if (!isLoading) {
                e.target.style.transform = "translateY(0)";
                e.target.style.boxShadow = "0 4px 6px rgba(0,0,0,0.1)";
              }
            }}
          >
            {isLoading ? (
              <div style={{ display: "flex", alignItems: "center", justifyContent: "center", gap: "0.5rem" }}>
                <div style={{
                  width: "20px",
                  height: "20px",
                  border: "2px solid rgba(255,255,255,0.3)",
                  borderTop: "2px solid white",
                  borderRadius: "50%",
                  animation: "spin 1s linear infinite"
                }} />
                Generating Report...
              </div>
            ) : (
              (() => {
                if (reportType === "consumption") {
                  const buildingText = selectedBuildingId ? " (Selected Building)" : " (All Buildings)";
                  return exportFormat === "pdf" 
                    ? `📑 Download ${getConsumptionTypeLabel(selectedConsumptionType)} Report${buildingText}` 
                    : `📊 Download ${getConsumptionTypeLabel(selectedConsumptionType)} Data${buildingText}`;
                }
                return "🌱 Download Carbon Footprint Report";
              })()
            )}
          </button>
        </div>
      </div>
    </div>
  );
};

export default ReportsPage;