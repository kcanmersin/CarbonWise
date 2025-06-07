import React, { useState, useEffect } from "react";
import Sidebar from "../components/Sidebar";
import { getBuildings, createBuilding, updateBuilding, deleteBuilding } from "../services/buildingService";

export default function BuildingsManagement() {
  const [buildings, setBuildings] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);
  const [editingBuilding, setEditingBuilding] = useState(null);
  const [formData, setFormData] = useState({
    name: "",
    e_MeterCode: "",
    g_MeterCode: ""
  });

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

  // Fetch buildings data on component mount
  useEffect(() => {
    fetchBuildings();
  }, []);

  const fetchBuildings = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const data = await getBuildings();
      setBuildings(data);
    } catch (err) {
      setError("Failed to fetch buildings. Please try again later.");
      console.error("Error fetching buildings:", err);
    } finally {
      setIsLoading(false);
    }
  };

  // Handle form input changes
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: value
    });
  };

  // Reset form to default values
  const resetForm = () => {
    setFormData({
      name: "",
      e_MeterCode: "",
      g_MeterCode: ""
    });
    setEditingBuilding(null);
  };

  // Handle edit building
  const handleEdit = (building) => {
    setEditingBuilding(building);
    setFormData({
      name: building.name,
      e_MeterCode: building.e_MeterCode || "",
      g_MeterCode: building.g_MeterCode || ""
    });
  };

  // Handle delete building
  const handleDelete = async (buildingId) => {
    if (window.confirm("Are you sure you want to delete this building? This action cannot be undone.")) {
      setIsLoading(true);
      setError(null);
      try {
        await deleteBuilding(buildingId);
        // Refresh buildings list after deletion
        fetchBuildings();
      } catch (err) {
        setError("Failed to delete building. Please try again.");
        console.error("Error deleting building:", err);
      } finally {
        setIsLoading(false);
      }
    }
  };

  // Handle form submission
  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsLoading(true);
    setError(null);
    
    try {
      if (editingBuilding) {
        // Update existing building
        await updateBuilding(editingBuilding.id, formData);
      } else {
        // Create new building
        await createBuilding(formData);
      }
      
      // Reset form after successful submission
      resetForm();
      // Refresh buildings list
      fetchBuildings();
    } catch (err) {
      setError(editingBuilding 
        ? "Failed to update building. Please try again." 
        : "Failed to create building. Please try again.");
      console.error("Error saving building:", err);
    } finally {
      setIsLoading(false);
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
            <h1 style={{ color: "#2c3e50", margin: "0 0 0.5rem 0" }}>Building Management</h1>
            <p style={{ color: "#7f8c8d", margin: 0 }}>Add, update, or delete building information</p>
          </div>
        </div>

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

        {/* Building form */}
        <div style={{
          backgroundColor: "white",
          padding: "1.5rem",
          borderRadius: "8px",
          boxShadow: "0 2px 4px rgba(0, 0, 0, 0.1)",
          marginBottom: "2rem"
        }}>
          <h2 style={{ 
            margin: "0 0 1.5rem 0", 
            color: editingBuilding ? "#3498db" : "#2c3e50" 
          }}>
            {editingBuilding ? `Edit Building: ${editingBuilding.name}` : "Add New Building"}
          </h2>

          <form onSubmit={handleSubmit}>
            <div style={{ 
              display: "grid",
              gridTemplateColumns: "repeat(auto-fill, minmax(250px, 1fr))",
              gap: "1rem",
              marginBottom: "1.5rem"
            }}>
              {/* Building Name */}
              <div>
                <label
                  htmlFor="name"
                  style={{
                    display: "block",
                    marginBottom: "0.5rem",
                    fontWeight: "bold",
                    color: "#34495e"
                  }}
                >
                  Building Name*
                </label>
                <input
                  type="text"
                  id="name"
                  name="name"
                  value={formData.name}
                  onChange={handleInputChange}
                  required
                  style={{
                    width: "100%",
                    padding: "0.75rem",
                    borderRadius: "4px",
                    border: "1px solid #ddd",
                    boxSizing: "border-box"
                  }}
                  placeholder="Enter building name"
                />
              </div>

              {/* Electricity Meter Code */}
              <div>
                <label
                  htmlFor="e_MeterCode"
                  style={{
                    display: "block",
                    marginBottom: "0.5rem",
                    fontWeight: "bold",
                    color: "#34495e"
                  }}
                >
                  Electricity Meter Code
                </label>
                <input
                  type="text"
                  id="e_MeterCode"
                  name="e_MeterCode"
                  value={formData.e_MeterCode}
                  onChange={handleInputChange}
                  style={{
                    width: "100%",
                    padding: "0.75rem",
                    borderRadius: "4px",
                    border: "1px solid #ddd",
                    boxSizing: "border-box"
                  }}
                  placeholder="Enter electricity meter code"
                />
              </div>

              {/* Gas Meter Code */}
              <div>
                <label
                  htmlFor="g_MeterCode"
                  style={{
                    display: "block",
                    marginBottom: "0.5rem",
                    fontWeight: "bold",
                    color: "#34495e"
                  }}
                >
                  Gas Meter Code
                </label>
                <input
                  type="text"
                  id="g_MeterCode"
                  name="g_MeterCode"
                  value={formData.g_MeterCode}
                  onChange={handleInputChange}
                  style={{
                    width: "100%",
                    padding: "0.75rem",
                    borderRadius: "4px",
                    border: "1px solid #ddd",
                    boxSizing: "border-box"
                  }}
                  placeholder="Enter gas meter code"
                />
              </div>
            </div>

            <div style={{ 
              display: "flex",
              gap: "1rem",
              justifyContent: "flex-end"
            }}>
              {editingBuilding && (
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
                  backgroundColor: "#2ecc71",
                  color: "white",
                  border: "none",
                  borderRadius: "4px",
                  cursor: isLoading ? "not-allowed" : "pointer",
                  fontWeight: "bold",
                  opacity: isLoading ? 0.7 : 1,
                  minWidth: "150px"
                }}
              >
                {isLoading ? "Saving..." : (editingBuilding ? "Update Building" : "Add Building")}
              </button>
            </div>
          </form>
        </div>

        {/* Loading indicator */}
        {isLoading && !editingBuilding && (
          <div style={{ 
            textAlign: "center", 
            padding: "2rem", 
            backgroundColor: "white",
            borderRadius: "8px",
            marginBottom: "1.5rem"
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
            <p style={{ marginTop: "1rem" }}>Loading buildings...</p>
          </div>
        )}

        {/* Buildings list */}
        <h2 style={{ color: "#2c3e50", marginBottom: "1rem" }}>Buildings</h2>
        
        {buildings.length === 0 && !isLoading ? (
          <div style={{
            backgroundColor: "white",
            padding: "2rem",
            textAlign: "center",
            borderRadius: "8px",
            boxShadow: "0 2px 4px rgba(0, 0, 0, 0.1)"
          }}>
            <p style={{ color: "#7f8c8d", margin: 0 }}>No buildings found. Add your first building using the form above.</p>
          </div>
        ) : (
          <div style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fill, minmax(300px, 1fr))",
            gap: "1.5rem"
          }}>
            {buildings.map(building => (
              <div 
                key={building.id}
                style={{
                  backgroundColor: "white",
                  borderRadius: "8px",
                  boxShadow: "0 2px 4px rgba(0, 0, 0, 0.1)",
                  overflow: "hidden",
                  transition: "transform 0.2s, box-shadow 0.2s",
                }}
              >
                <div style={{ 
                  backgroundColor: "#3498db", 
                  padding: "1rem",
                  color: "white",
                  display: "flex",
                  justifyContent: "space-between",
                  alignItems: "center"
                }}>
                  <h3 style={{ margin: 0 }}>{building.name}</h3>
                  <div style={{ display: "flex", gap: "0.5rem" }}>
                    <button
                      onClick={() => handleEdit(building)}
                      style={{
                        backgroundColor: "rgba(255, 255, 255, 0.3)",
                        color: "white",
                        border: "none",
                        borderRadius: "4px",
                        width: "32px",
                        height: "32px",
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "center",
                        cursor: "pointer"
                      }}
                      title="Edit Building"
                    >
                      <svg 
                        width="16" 
                        height="16" 
                        viewBox="0 0 24 24" 
                        fill="none" 
                        stroke="currentColor" 
                        strokeWidth="2" 
                        strokeLinecap="round" 
                        strokeLinejoin="round"
                      >
                        <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
                        <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
                      </svg>
                    </button>
                    <button
                      onClick={() => handleDelete(building.id)}
                      style={{
                        backgroundColor: "rgba(255, 255, 255, 0.3)",
                        color: "white",
                        border: "none",
                        borderRadius: "4px",
                        width: "32px",
                        height: "32px",
                        display: "flex",
                        alignItems: "center",
                        justifyContent: "center",
                        cursor: "pointer"
                      }}
                      title="Delete Building"
                    >
                      <svg 
                        width="16" 
                        height="16" 
                        viewBox="0 0 24 24" 
                        fill="none" 
                        stroke="currentColor" 
                        strokeWidth="2" 
                        strokeLinecap="round" 
                        strokeLinejoin="round"
                      >
                        <polyline points="3 6 5 6 21 6"></polyline>
                        <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
                      </svg>
                    </button>
                  </div>
                </div>
                <div style={{ padding: "1rem" }}>
                  <div style={{ marginBottom: "0.5rem" }}>
                    <strong>Electricity Meter:</strong> {building.e_MeterCode || "Not set"}
                  </div>
                  <div>
                    <strong>Gas Meter:</strong> {building.g_MeterCode || "Not set"}
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}