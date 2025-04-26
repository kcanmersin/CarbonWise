import React, { useState } from "react";
import { useNavigate } from "react-router-dom";

const Sidebar = ({ menuItems }) => {
  const [isOpen, setIsOpen] = useState(true);
  const [expandedMenus, setExpandedMenus] = useState({
    "resourceMonitoring": true, // Set resource monitoring to be expanded by default
    "carbonFootprint": true // Set carbon footprint to be expanded by default
  });
  const navigate = useNavigate();

  const toggleSidebar = () => setIsOpen(!isOpen);
  
  const toggleSubmenu = (key) => {
    setExpandedMenus(prev => ({
      ...prev,
      [key]: !prev[key]
    }));
  };

  const handleNavigation = (item) => {
    if (item.subItems) {
      toggleSubmenu(item.key);
    } else {
      // Navigate based on menu key
      switch (item.key) {
        case "dashboard":
          navigate("/");
          break;
        case "electricity":
          navigate("/electricity");
          break;
        case "water":
          navigate("/water");
          break;
        case "paper":
          navigate("/paper");
          break;
        case "naturalGas":
          navigate("/natural-gas");
          break;
        case "test":
          navigate("/carbon-footprint/test");
          break;
        case "calculations":
          navigate("/carbon-footprint/calculations");
          break;
        case "predictions":
          navigate("/predictions");
          break;
        case "adminTools":
          navigate("/admin");
          break;
        case "reports":
          navigate("/reports");
          break;
        default:
          // For unhandled items, navigate to dashboard
          navigate("/");
      }
    }
  };

  return (
    <div
      className="sidebar"
      style={{
        width: isOpen ? "200px" : "60px",
        background: "#2c3e50",
        color: "#ecf0f1",
        transition: "width 0.3s",
        paddingTop: "1rem",
        height: "100vh",
        overflowX: "hidden",
      }}
    >
      <button
        onClick={toggleSidebar}
        style={{
          background: "none",
          color: "#ecf0f1",
          border: "none",
          marginLeft: "1rem",
          fontSize: "1.5rem",
          cursor: "pointer",
        }}
      >
        ☰
      </button>
      <ul style={{ listStyle: "none", padding: "1rem", margin: 0 }}>
        {menuItems.map((item) => (
          <React.Fragment key={item.key}>
            <li
              style={{
                padding: "0.5rem 0",
                whiteSpace: "nowrap",
                overflow: "hidden",
                textOverflow: "ellipsis",
                cursor: "pointer",
                display: "flex",
                justifyContent: "space-between",
                alignItems: "center",
              }}
              onClick={() => handleNavigation(item)}
            >
              <span>{isOpen ? item.name : item.name.charAt(0)}</span>
              {isOpen && item.subItems && (
                <span style={{ marginLeft: "0.5rem" }}>
                  {expandedMenus[item.key] ? "▼" : "►"}
                </span>
              )}
            </li>
            
            {isOpen && item.subItems && expandedMenus[item.key] && (
              <ul style={{ listStyle: "none", paddingLeft: "1rem", margin: 0 }}>
                {item.subItems.map(subItem => (
                  <li
                    key={subItem.key}
                    style={{
                      padding: "0.3rem 0",
                      whiteSpace: "nowrap",
                      overflow: "hidden",
                      textOverflow: "ellipsis",
                      cursor: "pointer",
                      fontSize: "0.9rem",
                    }}
                    onClick={() => handleNavigation(subItem)}
                  >
                    {subItem.name}
                  </li>
                ))}
              </ul>
            )}
          </React.Fragment>
        ))}
      </ul>
    </div>
  );
};

export default Sidebar;