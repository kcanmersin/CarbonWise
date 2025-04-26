import React from "react";
import Sidebar from "../components/Sidebar";

export default function Dashboard() {
  // Menu items matching the image with nested structure
  const menuItems = [
    { name: "Dashboard", key: "dashboard" },
    { 
      name: "Resource Monitoring", 
      key: "resourceMonitoring",
      subItems: [
        { name: "Electricity", key: "electricity" },
        { name: "Water", key: "water" },
        { name: "Paper", key: "paper" },
        { name: "Natural Gas", key: "naturalGas" }
      ]
    },
    { 
      name: "Carbon Footprint", 
      key: "carbonFootprint",
      subItems: [
        { name: "Test", key: "test" },
        { name: "Calculations", key: "calculations" }
      ]
    },
    { name: "Predictions", key: "predictions" },
    { name: "Admin Tools", key: "adminTools" },
    { name: "Reports", key: "reports" }
  ];

  return (
    <div style={{ display: "flex", height: "100vh" }}>
      <Sidebar menuItems={menuItems} />
      <div style={{ flex: 1, padding: "2rem" }}>
        <h1>Dashboard</h1>
        <p>Welcome to your dashboard. Choose a section from the sidebar.</p>
        
        <div style={{ marginTop: "2rem" }}>
          <h2>Quick Access</h2>
          <div style={{ 
            display: "grid", 
            gridTemplateColumns: "repeat(auto-fill, minmax(250px, 1fr))", 
            gap: "1rem",
            marginTop: "1rem"
          }}>
            {/* Electricity card */}
            <div 
              style={{ 
                backgroundColor: "#3498db", 
                color: "white",
                padding: "1.5rem",
                borderRadius: "8px",
                cursor: "pointer",
                boxShadow: "0 4px 6px rgba(0,0,0,0.1)"
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
                boxShadow: "0 4px 6px rgba(0,0,0,0.1)"
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
                boxShadow: "0 4px 6px rgba(0,0,0,0.1)"
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
                boxShadow: "0 4px 6px rgba(0,0,0,0.1)"
              }}
              onClick={() => window.location.href = "/natural-gas"}
            >
              <h3 style={{ margin: "0 0 0.5rem 0" }}>Natural Gas</h3>
              <p style={{ margin: 0 }}>Monitor and analyze natural gas consumption</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}