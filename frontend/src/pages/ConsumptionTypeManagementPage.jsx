import React, { useState, useEffect, useRef  } from "react";
import Sidebar from "../components/Sidebar";
import { getBuildings } from "../services/buildingService";

// Import the specific services with their correct function names
import { 
  getElectricByBuilding, 
  createElectricRecord, 
  updateElectricRecord, 
  deleteElectricRecord,
  electricitydownloadSampleExcel,
  electricityMultipleUpload
} from "../services/electricityService";

import {
  getNaturalGasByBuilding,
  createNaturalGasRecord,
  updateNaturalGasRecord,
  deleteNaturalGasRecord,
  naturalGasdownloadSampleExcel,
  naturalGasMultipleUpload
} from "../services/naturalGasService";

import {
  getPaperByBuilding,
  createPaper,
  updatePaper,
  deletePaper,
  papersdownloadSampleExcel,
  papersMultipleUpload
} from "../services/paperService";

import {
  getWaterByBuilding,
  createWater,
  updateWater,
  deleteWater,
  waterdownloadSampleExcel,
  watersMultipleUpload
} from "../services/waterService";

export default function ConsumptionTypesManagement() {
  const [consumptionRecords, setConsumptionRecords] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [selectedType, setSelectedType] = useState("Electric");
  const [editingRecord, setEditingRecord] = useState(null);
  const formRef = useRef(null);
  const fileInputRef = useRef(null);
  
  // Initialize form data with proper fields for each type
  const initialFormData = {
    // Common fields
    date: new Date().toISOString().split('T')[0],
    buildingId: "",
    
    // Electric specific fields
    initialMeterValue: 0,
    finalMeterValue: 0,
    kwhValue: 0,
    
    // NaturalGas specific fields
    sM3Value: 0,
    
    // Paper specific fields
    usage: 0,
    
    // Water specific fields
    // initialMeterValue and finalMeterValue are already included above
  };
  
  const [formData, setFormData] = useState(initialFormData);
  const [buildings, setBuildings] = useState([]);
  const [selectedBuildingId, setSelectedBuildingId] = useState("");

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
  { key: "userManagement", name: "User Management" }, // 👈 NEW ITEM
  { key: "adminTools", name: "Admin Tools" },         // 👈 KEPT ORIGINAL
  { key: "reports", name: "Reports" }
];

  // Updated consumption types - ALL now require buildings
  const consumptionTypes = [
    { type: "Electric", requiresBuilding: true, unit: "kWh", color: "#3498db", icon: "⚡" },
    { type: "NaturalGas", requiresBuilding: true, unit: "m³", color: "#e74c3c", icon: "🔥" },
    { type: "Water", requiresBuilding: true, unit: "m³", color: "#2ecc71", icon: "💧" },
    { type: "Paper", requiresBuilding: true, unit: "kg", color: "#f39c12", icon: "📄" }
  ];

  // Fetch data on component mount
  useEffect(() => {
    fetchBuildings();
  }, []);

  // Fetch data when selected type or building changes
  useEffect(() => {
    if (selectedBuildingId) {
      fetchConsumptionData();
    }
  }, [selectedType, selectedBuildingId]);

  // Fetch consumption data based on selected type and building
  const fetchConsumptionData = async () => {
    setIsLoading(true);
    setError(null);
    try {
      let data;
      
      const today = new Date();
      const fiveYearsAgo = new Date();
      fiveYearsAgo.setFullYear(today.getFullYear() - 5);
      
      switch (selectedType) {
        case "Electric":
          if (selectedBuildingId) {
            data = await getElectricByBuilding(selectedBuildingId);
          } else {
            data = [];
          }
          break;
        case "NaturalGas":
          if (selectedBuildingId) {
            data = await getNaturalGasByBuilding(selectedBuildingId);
          } else {
            data = [];
          }
          break;
        case "Water":
          if (selectedBuildingId) {
            data = await getWaterByBuilding(selectedBuildingId);
          } else {
            data = [];
          }
          break;
        case "Paper":
          if (selectedBuildingId) {
            data = await getPaperByBuilding(selectedBuildingId);
          } else {
            data = [];
          }
          break;
        default:
          throw new Error(`Unknown consumption type: ${selectedType}`);
      }
      
      setConsumptionRecords(data);
    } catch (err) {
      setError(`Failed to fetch ${selectedType.toLowerCase()} data. Please try again later.`);
      console.error(`Error fetching ${selectedType.toLowerCase()} data:`, err);
    } finally {
      setIsLoading(false);
    }
  };

  const fetchBuildings = async () => {
    try {
      const data = await getBuildings();
      setBuildings(data);
      
      // Set default building if buildings exist
      if (data.length > 0) {
        setFormData(prev => ({
          ...prev,
          buildingId: data[0].id
        }));
        setSelectedBuildingId(data[0].id);
      }
    } catch (err) {
      console.error("Error fetching buildings:", err);
    }
  };

  // Handle type selection
  const handleTypeSelect = (type) => {
    setSelectedType(type);
    resetForm();
  };

  // Handle building selection
  const handleBuildingSelect = (e) => {
    setSelectedBuildingId(e.target.value);
  };

  // Handle form input changes
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    const numericFields = [
      'initialMeterValue', 
      'finalMeterValue', 
      'kwhValue', 
      'sM3Value', 
      'usage'
    ];
    
    // Convert numeric fields to numbers
    const processedValue = numericFields.includes(name) 
      ? parseFloat(value) || 0 
      : value;
    
    setFormData({
      ...formData,
      [name]: processedValue
    });
  };

  // Calculate derived values when initial and final meter values change
  useEffect(() => {
    if (selectedType === "Electric") {
      const kwhValue = formData.finalMeterValue - formData.initialMeterValue;
      if (kwhValue >= 0) {
        setFormData(prev => ({
          ...prev,
          kwhValue
        }));
      }
    }
  }, [formData.initialMeterValue, formData.finalMeterValue, selectedType]);

  // Reset form to default values based on selected type
  const resetForm = () => {
    const currentDate = new Date().toISOString().split('T')[0];
    
    const defaultFormData = {
      date: currentDate,
      initialMeterValue: 0,
      finalMeterValue: 0,
      buildingId: selectedBuildingId || (buildings.length > 0 ? buildings[0].id : "")
    };
    
    // Add type-specific default fields
    switch (selectedType) {
      case "Electric":
        defaultFormData.kwhValue = 0;
        break;
      case "NaturalGas":
        defaultFormData.sM3Value = 0;
        break;
      case "Paper":
        defaultFormData.usage = 0;
        break;
      case "Water":
        // Water already has initialMeterValue and finalMeterValue
        break;
      default:
        break;
    }
    
    setFormData(defaultFormData);
    setEditingRecord(null);
  };

  // Handle edit record
  const handleEdit = (record) => {
    setEditingRecord(record);
    
    // Create a base form data object
    const editFormData = {
      date: record.date ? record.date.split('T')[0] : new Date().toISOString().split('T')[0],
      buildingId: record.buildingId || selectedBuildingId,
      initialMeterValue: 0,
      finalMeterValue: 0
    };
    
    // Add type-specific fields
    switch (selectedType) {
      case "Electric":
        editFormData.initialMeterValue = record.initialMeterValue || 0;
        editFormData.finalMeterValue = record.finalMeterValue || 0;
        editFormData.kwhValue = record.kwhValue || 0;
        break;
      case "NaturalGas":
        editFormData.initialMeterValue = record.initialMeterValue || 0;
        editFormData.finalMeterValue = record.finalMeterValue || 0;
        editFormData.sM3Value = record.sM3Value || 0;
        break;
      case "Paper":
        editFormData.usage = record.usage || 0;
        break;
      case "Water":
        editFormData.initialMeterValue = record.initialMeterValue || 0;
        editFormData.finalMeterValue = record.finalMeterValue || 0;
        break;
      default:
        break;
    }
    
    setFormData(editFormData);
    
    // Scroll to the form
    if (formRef.current) {
      formRef.current.scrollIntoView({ 
        behavior: 'smooth',
        block: 'start'
      });
    }
  };

  // Handle delete record
  const handleDelete = async (record) => {
    if (window.confirm(`Are you sure you want to delete this ${selectedType} record? This action cannot be undone.`)) {
      setIsLoading(true);
      setError(null);
      try {
        // Use the appropriate delete function based on selected type
        switch (selectedType) {
          case "Electric":
            await deleteElectricRecord(record.id);
            break;
          case "NaturalGas":
            await deleteNaturalGasRecord(record.id);
            break;
          case "Water":
            await deleteWater(record.id);
            break;
          case "Paper":
            await deletePaper(record.id);
            break;
          default:
            throw new Error(`Unknown consumption type: ${selectedType}`);
        }
        
        fetchConsumptionData(); // Refresh data after deletion
      } catch (err) {
        setError(`Failed to delete ${selectedType} record. Please try again.`);
        console.error(`Error deleting ${selectedType} record:`, err);
      } finally {
        setIsLoading(false);
      }
    }
  };

  // Prepare record data based on selected type
  const prepareRecordData = () => {
    const baseData = {
      date: new Date(formData.date).toISOString(),
      buildingId: formData.buildingId // All types now require buildingId
    };
    
    // Add type-specific fields
    switch (selectedType) {
      case "Electric":
        return {
          ...baseData,
          initialMeterValue: parseFloat(formData.initialMeterValue),
          finalMeterValue: parseFloat(formData.finalMeterValue),
          kwhValue: parseFloat(formData.kwhValue)
        };
      case "NaturalGas":
        return {
          ...baseData,
          initialMeterValue: parseFloat(formData.initialMeterValue),
          finalMeterValue: parseFloat(formData.finalMeterValue),
          sM3Value: parseFloat(formData.sM3Value)
        };
      case "Paper":
        return {
          ...baseData,
          usage: parseFloat(formData.usage)
        };
      case "Water":
        return {
          ...baseData,
          initialMeterValue: parseFloat(formData.initialMeterValue),
          finalMeterValue: parseFloat(formData.finalMeterValue),
          usage: parseFloat(formData.finalMeterValue) - parseFloat(formData.initialMeterValue) // Calculate usage for water
        };
      default:
        return baseData;
    }
  };

  // Submit form
  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);
    
    try {
      const recordData = prepareRecordData();

      if (editingRecord) {
        // Update existing record
        switch (selectedType) {
          case "Electric":
            await updateElectricRecord(editingRecord.id, recordData);
            break;
          case "NaturalGas":
            await updateNaturalGasRecord(editingRecord.id, recordData);
            break;
          case "Water":
            await updateWater(editingRecord.id, recordData);
            break;
          case "Paper":
            await updatePaper(editingRecord.id, recordData);
            break;
          default:
            throw new Error(`Unknown consumption type: ${selectedType}`);
        }
      } else {
        // Create new record
        switch (selectedType) {
          case "Electric":
            await createElectricRecord(recordData);
            break;
          case "NaturalGas":
            await createNaturalGasRecord(recordData);
            break;
          case "Water":
            await createWater(recordData);
            break;
          case "Paper":
            await createPaper(recordData);
            break;
          default:
            throw new Error(`Unknown consumption type: ${selectedType}`);
        }
      }
      
      // Reset form and refresh data
      resetForm();
      fetchConsumptionData();
    } catch (err) {
      setError(editingRecord 
        ? `Failed to update ${selectedType} record. Please try again.` 
        : `Failed to create ${selectedType} record. Please try again.`);
      console.error(`Error saving ${selectedType} record:`, err);
    } finally {
      setIsLoading(false);
    }
  };

  // Get current consumption type config
  const getCurrentTypeConfig = () => {
    return consumptionTypes.find(t => t.type === selectedType);
  };

  // Format date for display
  const formatDate = (dateString) => {
    if (!dateString) return "N/A";
    
    const date = new Date(dateString);
    if (isNaN(date.getTime())) return dateString;
    
    return date.toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  // Get building name by ID
  const getBuildingName = (buildingId) => {
    if (!buildingId) return "N/A";
    const building = buildings.find(b => b.id === buildingId);
    return building ? building.name : "Unknown Building";
  };

  const handleDownloadTemplate = async () => {
    setError(null);
    try {
      switch (selectedType) {
        case "Electric":
          await electricitydownloadSampleExcel();
          break;
        case "NaturalGas":
          await naturalGasdownloadSampleExcel();
          break;
        case "Water":
          await waterdownloadSampleExcel();
          break;
        case "Paper":
          await papersdownloadSampleExcel();
          break;
        default:
          throw new Error(`Unknown consumption type: ${selectedType}`);
      }
    } catch (err) {
      setError(err.message);
    }
  };
  
  // Upload handler for current consumption type
  const handleUploadExcel = async (e) => {
    const file = e.target.files[0];
    if (!file) return;

    setIsLoading(true);
    setError(null);
    
    try {
      switch (selectedType) {
        case "Electric":
          await electricityMultipleUpload(file);
          break;
        case "NaturalGas":
          await naturalGasMultipleUpload(file);
          break;
        case "Water":
          await watersMultipleUpload(file);
          break;
        case "Paper":
          await papersMultipleUpload(file);
          break;
        default:
          throw new Error(`Unknown consumption type: ${selectedType}`);
      }
      
      // Refresh data after successful upload
      fetchConsumptionData();
      alert(`Successfully uploaded multiple ${selectedType} records!`);
    } catch (err) {
      setError(err.message);
    } finally {
      setIsLoading(false);
      e.target.value = ''; // Reset file input
    }
  };

  return (
    <div style={{ display: "flex", minHeight: "100vh" }}>
      <Sidebar menuItems={menuItems} />
      <div style={{ flex: 1, padding: "2rem", backgroundColor: "#f5f7fa", overflowY: "auto" }}>
        <div style={{ 
          display: "flex", 
          justifyContent: "space-between", 
          alignItems: "center",
          marginBottom: "2rem"
        }}>
          <div>
            <h1 style={{ color: "#2c3e50", margin: "0 0 0.5rem 0" }}>Consumption Types Management</h1>
            <p style={{ color: "#7f8c8d", margin: 0 }}>Add, update, or delete consumption data records</p>
          </div>
        </div>

        {/* Consumption Type Selector and Download Button */}
        <div style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          marginBottom: "2rem",
          gap: "1rem"
        }}>
          {/* Consumption type buttons */}
          <div style={{
            display: "flex",
            gap: "1rem",
            overflowX: "auto",
            paddingBottom: "0.5rem"
          }}>
            {consumptionTypes.map(type => (
              <button
                key={type.type}
                onClick={() => handleTypeSelect(type.type)}
                style={{
                  padding: "1rem 1.5rem",
                  backgroundColor: selectedType === type.type ? type.color : "white",
                  color: selectedType === type.type ? "white" : "#34495e",
                  border: `2px solid ${type.color}`,
                  borderRadius: "8px",
                  cursor: "pointer",
                  fontWeight: "bold",
                  display: "flex",
                  alignItems: "center",
                  gap: "0.5rem",
                  minWidth: "150px",
                  justifyContent: "center"
                }}
              >
                <span style={{ fontSize: "1.5rem" }}>{type.icon}</span>
                {type.type}
              </button>
            ))}
          </div>
          
          {/* Download button */}
          <button
            onClick={handleDownloadTemplate}
            style={{
              padding: "1rem 1.5rem",
              backgroundColor: getCurrentTypeConfig().color,
              color: "white",
              border: "none",
              borderRadius: "8px",
              cursor: "pointer",
              fontWeight: "bold",
              display: "flex",
              alignItems: "center",
              gap: "0.5rem",
              minWidth: "200px"
            }}
          >
            📥 Download Excel Template
          </button>
        </div>

        <div ref={formRef} style={{ marginBottom: "2rem" }}></div>

        {/* Error message */}
        {error && (
          <div style={{
            backgroundColor: "#f8d7da",
            color: "#721c24",
            padding: "1rem",
            borderRadius: "8px",
            marginBottom: "1.5rem"
          }}>
            <strong>Error:</strong> {error}
          </div>
        )}

        {/* Form for adding/editing consumption data */}
        <div style={{
          backgroundColor: "white",
          padding: "1.5rem",
          borderRadius: "8px",
          boxShadow: "0 2px 4px rgba(0, 0, 0, 0.1)",
          marginBottom: "2rem"
        }}>
          <h2 style={{ 
            margin: "0 0 1.5rem 0", 
            color: getCurrentTypeConfig().color,
            display: "flex",
            alignItems: "center",
            gap: "0.5rem"
          }}>
            <span style={{ fontSize: "1.5rem" }}>{getCurrentTypeConfig().icon}</span>
            {editingRecord ? `Edit ${selectedType} Record` : `Add New ${selectedType} Record`}
          </h2>

          <form onSubmit={handleSubmit}>
            <div style={{ 
              display: "grid",
              gridTemplateColumns: "repeat(auto-fill, minmax(250px, 1fr))",
              gap: "1rem",
              marginBottom: "1.5rem"
            }}>
              {/* Date - Common for all types */}
              <div>
                <label
                  htmlFor="date"
                  style={{
                    display: "block",
                    marginBottom: "0.5rem",
                    fontWeight: "bold",
                    color: "#34495e"
                  }}
                >
                  Date*
                </label>
                <input
                  type="date"
                  id="date"
                  name="date"
                  value={formData.date}
                  onChange={handleInputChange}
                  required
                  style={{
                    width: "100%",
                    padding: "0.75rem",
                    borderRadius: "4px",
                    border: "1px solid #ddd",
                    boxSizing: "border-box"
                  }}
                />
              </div>

              {/* Building - Required for ALL types now */}
              <div>
                <label
                  htmlFor="buildingId"
                  style={{
                    display: "block",
                    marginBottom: "0.5rem",
                    fontWeight: "bold",
                    color: "#34495e"
                  }}
                >
                  Building*
                </label>
                <select
                  id="buildingId"
                  name="buildingId"
                  value={formData.buildingId}
                  onChange={handleInputChange}
                  required
                  style={{
                    width: "100%",
                    padding: "0.75rem",
                    borderRadius: "4px",
                    border: "1px solid #ddd",
                    backgroundColor: "white",
                    boxSizing: "border-box"
                  }}
                >
                  <option value="">Select a building</option>
                  {buildings.map(building => (
                    <option key={building.id} value={building.id}>{building.name}</option>
                  ))}
                </select>
              </div>

              {/* Electric & Natural Gas & Water - Initial & Final Meter Values */}
              {(selectedType === "Electric" || selectedType === "NaturalGas" || selectedType === "Water") && (
                <>
                  <div>
                    <label
                      htmlFor="initialMeterValue"
                      style={{
                        display: "block",
                        marginBottom: "0.5rem",
                        fontWeight: "bold",
                        color: "#34495e"
                      }}
                    >
                      Initial Meter Value*
                    </label>
                    <input
                      type="number"
                      id="initialMeterValue"
                      name="initialMeterValue"
                      value={formData.initialMeterValue}
                      onChange={handleInputChange}
                      required
                      min="0"
                      step="0.01"
                      style={{
                        width: "100%",
                        padding: "0.75rem",
                        borderRadius: "4px",
                        border: "1px solid #ddd",
                        boxSizing: "border-box"
                      }}
                    />
                  </div>
                  <div>
                    <label
                      htmlFor="finalMeterValue"
                      style={{
                        display: "block",
                        marginBottom: "0.5rem",
                        fontWeight: "bold",
                        color: "#34495e"
                      }}
                    >
                      Final Meter Value*
                    </label>
                    <input
                      type="number"
                      id="finalMeterValue"
                      name="finalMeterValue"
                      value={formData.finalMeterValue}
                      onChange={handleInputChange}
                      required
                      min="0"
                      step="0.01"
                      style={{
                        width: "100%",
                        padding: "0.75rem",
                        borderRadius: "4px",
                        border: "1px solid #ddd",
                        boxSizing: "border-box"
                      }}
                    />
                  </div>
                </>
              )}

              {/* Electric - kWh Value */}
              {selectedType === "Electric" && (
                <div>
                  <label
                    htmlFor="kwhValue"
                    style={{
                      display: "block",
                      marginBottom: "0.5rem",
                      fontWeight: "bold",
                      color: "#34495e"
                    }}
                  >
                    kWh Value*
                  </label>
                  <input
                    type="number"
                    id="kwhValue"
                    name="kwhValue"
                    value={formData.kwhValue}
                    onChange={handleInputChange}
                    required
                    readOnly={true}
                    style={{
                      width: "100%",
                      padding: "0.75rem",
                      borderRadius: "4px",
                      border: "1px solid #ddd",
                      boxSizing: "border-box",
                      backgroundColor: "#f5f5f5"
                    }}
                  />
                  <small style={{ color: "#7f8c8d" }}>Calculated from meter values</small>
                </div>
              )}

              {/* Natural Gas - sM3 Value */}
              {selectedType === "NaturalGas" && (
                <div>
                  <label
                    htmlFor="sM3Value"
                    style={{
                      display: "block",
                      marginBottom: "0.5rem",
                      fontWeight: "bold",
                      color: "#34495e"
                    }}
                  >
                    Standard m³ Value*
                  </label>
                  <input
                    type="number"
                    id="sM3Value"
                    name="sM3Value"
                    value={formData.sM3Value}
                    onChange={handleInputChange}
                    required
                    min="0"
                    style={{
                      width: "100%",
                      padding: "0.75rem",
                      borderRadius: "4px",
                      border: "1px solid #ddd",
                      boxSizing: "border-box"
                    }}
                  />
                </div>
              )}

              {/* Paper - Usage */}
              {selectedType === "Paper" && (
                <div>
                  <label
                    htmlFor="usage"
                    style={{
                      display: "block",
                      marginBottom: "0.5rem",
                      fontWeight: "bold",
                      color: "#34495e"
                    }}
                  >
                    Usage (kg)*
                  </label>
                  <input
                    type="number"
                    id="usage"
                    name="usage"
                    value={formData.usage}
                    onChange={handleInputChange}
                    required
                    min="0"
                    step="0.01"
                    style={{
                      width: "100%",
                      padding: "0.75rem",
                      borderRadius: "4px",
                      border: "1px solid #ddd",
                      boxSizing: "border-box"
                    }}
                  />
                </div>
              )}

            </div>

            <div style={{ 
              display: "flex",
              gap: "1rem",
              justifyContent: "flex-end",
              alignItems: "center"
            }}>
              {editingRecord && (
                <button
                  type="button"
                  onClick={resetForm}
                  style={{
                    padding: "0.75rem 1.5rem",
                    backgroundColor: "#e74c3c",
                    color: "white",
                    border: "none",
                    borderRadius: "4px",
                    cursor: "pointer",
                    fontWeight: "bold"
                  }}
                >
                  Cancel Edit
                </button>
              )}
              <button
                type="submit"
                disabled={isLoading}
                style={{
                  padding: "0.75rem 1.5rem",
                  backgroundColor: getCurrentTypeConfig().color,
                  color: "white",
                  border: "none",
                  borderRadius: "4px",
                  cursor: isLoading ? "not-allowed" : "pointer",
                  fontWeight: "bold",
                  opacity: isLoading ? 0.7 : 1,
                  minWidth: "150px"
                }}
              >
                {isLoading ? "Saving..." : (editingRecord ? `Update ${selectedType} Record` : `Add ${selectedType} Record`)}
              </button>
              
              {/* Add upload button here */}
              <div>
                <input
                  type="file"
                  ref={fileInputRef}
                  onChange={handleUploadExcel}
                  accept=".xlsx,.xls"
                  style={{ display: 'none' }}
                />
                <button
                  type="button"
                  onClick={() => fileInputRef.current?.click()}
                  disabled={isLoading}
                  style={{
                    padding: "0.75rem 1.5rem",
                    backgroundColor: "#9b59b6",
                    color: "white",
                    border: "none",
                    borderRadius: "4px",
                    cursor: isLoading ? "not-allowed" : "pointer",
                    fontWeight: "bold",
                    opacity: isLoading ? 0.7 : 1,
                    minWidth: "150px"
                  }}
                >
                  📤 Add Multiple Records
                </button>
              </div>
            </div>
          </form>
        </div>

        {/* Building Selector - Now displayed for ALL types since all require buildings */}
        <div style={{
          backgroundColor: "white",
          padding: "1.5rem",
          borderRadius: "8px",
          boxShadow: "0 2px 4px rgba(0, 0, 0, 0.1)",
          marginBottom: "2rem"
        }}>
          <h2 style={{ margin: "0 0 1rem 0", color: "#2c3e50" }}>Select Building</h2>
          <select
            value={selectedBuildingId}
            onChange={handleBuildingSelect}
            style={{
              width: "100%",
              padding: "0.75rem",
              borderRadius: "4px",
              border: "1px solid #ddd",
              backgroundColor: "white",
              boxSizing: "border-box",
              fontSize: "1rem"
            }}
          >
            <option value="">Select a building</option>
            {buildings.map(building => (
              <option key={building.id} value={building.id}>{building.name}</option>
            ))}
          </select>
        </div>

        {/* Records List */}
        <div style={{
          backgroundColor: "white",
          padding: "1.5rem",
          borderRadius: "8px",
          boxShadow: "0 2px 4px rgba(0, 0, 0, 0.1)"
        }}>
          <h2 style={{ 
            color: "#2c3e50", 
            marginBottom: "1rem",
            display: "flex",
            alignItems: "center",
            gap: "0.5rem"
          }}>
            <span style={{ fontSize: "1.2rem" }}>{getCurrentTypeConfig().icon}</span>
            {selectedType} Records
          </h2>

          {/* Loading indicator */}
          {isLoading && !editingRecord && (
            <div style={{ 
              textAlign: "center", 
              padding: "2rem"
            }}>
              <div style={{
                display: "inline-block",
                width: "40px",
                height: "40px",
                border: "4px solid #f3f3f3",
                borderTop: `4px solid ${getCurrentTypeConfig().color}`,
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
              <p style={{ marginTop: "1rem" }}>Loading {selectedType} records...</p>
            </div>
          )}

          {/* Empty state message */}
          {!isLoading && consumptionRecords.length === 0 && (
            <div style={{
              textAlign: "center",
              padding: "2rem",
              color: "#7f8c8d"
            }}>
              {!selectedBuildingId ? (
                <p>Please select a building to view {selectedType.toLowerCase()} records.</p>
              ) : (
                <p>No {selectedType.toLowerCase()} records found for this building. Add your first record using the form above.</p>
              )}
            </div>
          )}

          {/* Records Table */}
          {!isLoading && consumptionRecords.length > 0 && (
            <div style={{ overflowX: "auto" }}>
              <table style={{
                width: "100%",
                borderCollapse: "collapse",
                borderSpacing: 0,
                fontSize: "0.95rem"
              }}>
                <thead>
                  <tr style={{
                    backgroundColor: "#f8f9fa",
                    borderBottom: "2px solid #e9ecef"
                  }}>
                    <th style={{ padding: "0.75rem 1rem", textAlign: "left" }}>Date</th>
                    
                    {/* Show different columns based on type */}
                    {selectedType === "Electric" && (
                      <>
                        <th style={{ padding: "0.75rem 1rem", textAlign: "left" }}>Initial Reading</th>
                        <th style={{ padding: "0.75rem 1rem", textAlign: "left" }}>Final Reading</th>
                        <th style={{ padding: "0.75rem 1rem", textAlign: "left" }}>kWh Value</th>
                      </>
                    )}
                    
                    {selectedType === "NaturalGas" && (
                      <>
                        <th style={{ padding: "0.75rem 1rem", textAlign: "left" }}>Initial Reading</th>
                        <th style={{ padding: "0.75rem 1rem", textAlign: "left" }}>Final Reading</th>
                        <th style={{ padding: "0.75rem 1rem", textAlign: "left" }}>SM³ Value</th>
                      </>
                    )}
                    
                    {selectedType === "Water" && (
                      <>
                        <th style={{ padding: "0.75rem 1rem", textAlign: "left" }}>Initial Reading</th>
                        <th style={{ padding: "0.75rem 1rem", textAlign: "left" }}>Final Reading</th>
                        <th style={{ padding: "0.75rem 1rem", textAlign: "left" }}>Consumption (m³)</th>
                      </>
                    )}
                    
                    {selectedType === "Paper" && (
                      <th style={{ padding: "0.75rem 1rem", textAlign: "left" }}>Usage (kg)</th>
                    )}
                    
                    {/* Building column - now shown for ALL types */}
                    <th style={{ padding: "0.75rem 1rem", textAlign: "left" }}>Building</th>
                    
                    <th style={{ padding: "0.75rem 1rem", textAlign: "center" }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {consumptionRecords.map(record => (
                    <tr key={record.id} style={{ borderBottom: "1px solid #e9ecef" }}>
                      <td style={{ padding: "0.75rem 1rem" }}>{formatDate(record.date)}</td>
                      
                      {/* Electric specific columns */}
                      {selectedType === "Electric" && (
                        <>
                          <td style={{ padding: "0.75rem 1rem" }}>{record.initialMeterValue}</td>
                          <td style={{ padding: "0.75rem 1rem" }}>{record.finalMeterValue}</td>
                          <td style={{ padding: "0.75rem 1rem", fontWeight: "bold" }}>{record.kwhValue} kWh</td>
                        </>
                      )}
                      
                      {/* NaturalGas specific columns */}
                      {selectedType === "NaturalGas" && (
                        <>
                          <td style={{ padding: "0.75rem 1rem" }}>{record.initialMeterValue}</td>
                          <td style={{ padding: "0.75rem 1rem" }}>{record.finalMeterValue}</td>
                          <td style={{ padding: "0.75rem 1rem", fontWeight: "bold" }}>{record.sM3Value} m³</td>
                        </>
                      )}
                      
                      {/* Water specific columns */}
                      {selectedType === "Water" && (
                        <>
                          <td style={{ padding: "0.75rem 1rem" }}>{record.initialMeterValue}</td>
                          <td style={{ padding: "0.75rem 1rem" }}>{record.finalMeterValue}</td>
                          <td style={{ padding: "0.75rem 1rem", fontWeight: "bold" }}>
                            {record.usage ? record.usage.toFixed(2) : (record.finalMeterValue - record.initialMeterValue).toFixed(2)} m³
                          </td>
                        </>
                      )}
                      
                      {/* Paper specific columns */}
                      {selectedType === "Paper" && (
                        <td style={{ padding: "0.75rem 1rem", fontWeight: "bold" }}>{record.usage} kg</td>
                      )}
                      
                      {/* Building column - now shown for ALL types */}
                      <td style={{ padding: "0.75rem 1rem" }}>{getBuildingName(record.buildingId)}</td>
                      
                      {/* Actions column */}
                      <td style={{ 
                        padding: "0.75rem 1rem", 
                        textAlign: "center",
                        whiteSpace: "nowrap"
                      }}>
                        <button
                          onClick={() => handleEdit(record)}
                          style={{
                            padding: "0.4rem 0.7rem",
                            backgroundColor: "white",
                            color: "#3498db",
                            border: "1px solid #3498db",
                            borderRadius: "4px",
                            cursor: "pointer",
                            marginRight: "0.5rem"
                          }}
                          title={`Edit ${selectedType} Record`}
                        >
                          Edit
                        </button>
                        <button
                          onClick={() => handleDelete(record)}
                          style={{
                            padding: "0.4rem 0.7rem",
                            backgroundColor: "white",
                            color: "#e74c3c",
                            border: "1px solid #e74c3c",
                            borderRadius: "4px",
                            cursor: "pointer"
                          }}
                          title={`Delete ${selectedType} Record`}
                        >
                          Delete
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}