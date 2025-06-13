import React from "react";
import Sidebar from "../components/Sidebar";
import { useNavigate } from "react-router-dom";

export default function AdminTools() {
  const navigate = useNavigate();

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
  { key: "userManagement", name: "User Management" },
  { key: "adminTools", name: "Admin Tools" },
  { key: "reports", name: "Reports" }
];

  const adminCards = [
    {
      title: "Buildings",
      description: "Add, update, or delete building information",
      icon: "🏢",
      color: "#3498db",
      path: "/admin/buildings"
    },
    {
      title: "Consumption Types",
      description: "Manage consumption data for electricity, water, paper, and natural gas",
      icon: "📊",
      color: "#e74c3c",
      path: "/admin/consumption-types"
    },
    {
      title: "School Information",
      description: "Update school details and configuration",
      icon: "🏫",
      color: "#2ecc71",
      path: "/admin/school-info"
    }
  ];

  return (
    <div style={{ display: "flex", height: "100vh" }}>
      <Sidebar menuItems={menuItems} />
      <div style={{ flex: 1, padding: "2rem", backgroundColor: "#f5f7fa", overflowY: "auto" }}>
        <div style={{ 
          display: "flex", 
          justifyContent: "space-between", 
          alignItems: "center",
          marginBottom: "2rem"
        }}>
          <h1 style={{ color: "#2c3e50", margin: 0 }}>Admin Tools</h1>
        </div>

        <div style={{
          display: "grid",
          gridTemplateColumns: "repeat(auto-fill, minmax(350px, 1fr))",
          gap: "2rem",
        }}>
          {adminCards.map((card, index) => (
            <div
              key={index}
              style={{
                backgroundColor: "white",
                borderRadius: "10px",
                boxShadow: "0 4px 6px rgba(0, 0, 0, 0.1)",
                overflow: "hidden",
                transition: "transform 0.2s, box-shadow 0.2s",
                cursor: "pointer",
              }}
              onClick={() => navigate(card.path)}
              onMouseOver={(e) => {
                e.currentTarget.style.transform = "translateY(-5px)";
                e.currentTarget.style.boxShadow = "0 8px 15px rgba(0, 0, 0, 0.15)";
              }}
              onMouseOut={(e) => {
                e.currentTarget.style.transform = "translateY(0)";
                e.currentTarget.style.boxShadow = "0 4px 6px rgba(0, 0, 0, 0.1)";
              }}
            >
              <div style={{ 
                backgroundColor: card.color, 
                padding: "1.5rem",
                display: "flex",
                alignItems: "center",
                gap: "1rem"
              }}>
                <div style={{ 
                  fontSize: "2.5rem",
                  backgroundColor: "rgba(255, 255, 255, 0.3)",
                  width: "60px",
                  height: "60px",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  borderRadius: "50%"
                }}>
                  {card.icon}
                </div>
                <h2 style={{ color: "white", margin: 0 }}>{card.title}</h2>
              </div>
              <div style={{ padding: "1.5rem" }}>
                <p style={{ 
                  color: "#7f8c8d", 
                  margin: 0,
                  fontSize: "1.1rem",
                  lineHeight: "1.5"
                }}>
                  {card.description}
                </p>
              </div>
              <div style={{ 
                padding: "1rem 1.5rem",
                borderTop: "1px solid #ecf0f1",
                display: "flex",
                justifyContent: "flex-end"
              }}>
                <span style={{ 
                  color: card.color, 
                  fontWeight: "bold",
                  display: "flex",
                  alignItems: "center",
                  gap: "0.5rem"
                }}>
                  Manage
                  <svg 
                    width="20" 
                    height="20" 
                    viewBox="0 0 24 24" 
                    fill="none" 
                    stroke="currentColor" 
                    strokeWidth="2" 
                    strokeLinecap="round" 
                    strokeLinejoin="round"
                  >
                    <path d="M5 12h14"></path>
                    <path d="M12 5l7 7-7 7"></path>
                  </svg>
                </span>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}