import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { logoutUser } from "../services/authService";
import { getCurrentUserInfo } from "../services/userService";

const Sidebar = ({ menuItems }) => {
  const [isOpen, setIsOpen] = useState(true);
  const [expandedMenus, setExpandedMenus] = useState({
    "resourceMonitoring": true,
    "carbonFootprint": true
  });
  const [isLoggingOut, setIsLoggingOut] = useState(false);
  const [currentUser, setCurrentUser] = useState(null);
  const [isLoadingUser, setIsLoadingUser] = useState(true);
  const navigate = useNavigate();

  // Fetch current user info on component mount
  useEffect(() => {
    const fetchUserInfo = async () => {
      try {
        setIsLoadingUser(true);
        const userInfo = await getCurrentUserInfo();
        setCurrentUser(userInfo);
      } catch (error) {
        console.error("Error fetching user info:", error);
        // If we can't get user info, redirect to login
        navigate("/auth");
      } finally {
        setIsLoadingUser(false);
      }
    };

    fetchUserInfo();
  }, [navigate]);

  // Define role-based access control
  const rolePermissions = {
    "User": [
      "dashboard",
      "resource-monitoring", // Add the parent menu
      "electricity", 
      "water", 
      "paper", 
      "naturalGas",
      "carbon-footprint", // Add the parent menu
      "test",
      "calculations",
      "predictions",
    ],
    "Admin": [
      "dashboard",
      "resource-monitoring", // Add the parent menu
      "electricity", 
      "water", 
      "paper", 
      "naturalGas",
      "carbon-footprint", // Add the parent menu
      "test",
      "calculations",
      "predictions",
      "reports",
      "adminTools"
    ],
    "SuperUser": [
      "dashboard",
      "resource-monitoring", // Add the parent menu
      "electricity", 
      "water", 
      "paper", 
      "naturalGas",
      "carbon-footprint", // Add the parent menu
      "test",
      "calculations",
      "predictions",
      "userManagement",
      "adminTools",
      "reports"
    ]
  };

  // Check if user has access to a menu item
  const hasAccess = (itemKey) => {
    if (!currentUser || !currentUser.role) return false;
    const allowedItems = rolePermissions[currentUser.role] || [];
    return allowedItems.includes(itemKey);
  };

  // Filter menu items based on user role
  const getFilteredMenuItems = () => {
    if (!currentUser) return [];

    return menuItems.map(item => {
      // Check if main item is accessible
      if (!hasAccess(item.key)) {
        return null; // Hide the entire menu item
      }

      // If item has sub-items, filter them too
      if (item.subItems) {
        const filteredSubItems = item.subItems.filter(subItem => hasAccess(subItem.key));
        
        // If no sub-items are accessible, hide the main item
        if (filteredSubItems.length === 0) {
          return null;
        }

        return {
          ...item,
          subItems: filteredSubItems
        };
      }

      return item;
    }).filter(Boolean); // Remove null items
  };

  const toggleSidebar = () => setIsOpen(!isOpen);
  
  const toggleSubmenu = (key) => {
    setExpandedMenus(prev => ({
      ...prev,
      [key]: !prev[key]
    }));
  };

  const handleNavigation = (item) => {
    // Double-check access before navigation
    if (!hasAccess(item.key)) {
      console.warn(`Access denied to ${item.key} for user role: ${currentUser?.role}`);
      return;
    }

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
        case "userManagement":
          navigate("/admin/user-management");
          break;
        case "adminTools":
          navigate("/admin");
          break;
        case "reports":
          navigate("/reports");
          break;
        default:
          navigate("/");
      }
    }
  };

  const handleLogout = async () => {
    if (isLoggingOut) return;
    
    try {
      setIsLoggingOut(true);
      await logoutUser();
    } catch (error) {
      console.error("Error during logout:", error);
      window.location.href = '/';
    } finally {
      setIsLoggingOut(false);
    }
  };

  // Show loading state while fetching user info
  if (isLoadingUser) {
    return (
      <div
        style={{
          width: "200px",
          background: "#2c3e50",
          color: "#ecf0f1",
          height: "100vh",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          flexDirection: "column"
        }}
      >
        <div style={{
          width: "30px",
          height: "30px",
          border: "3px solid #ecf0f1",
          borderTop: "3px solid transparent",
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
        <p style={{ marginTop: "1rem", fontSize: "0.9rem" }}>Loading...</p>
      </div>
    );
  }

  const filteredMenuItems = getFilteredMenuItems();

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
        display: "flex",
        flexDirection: "column",
      }}
    >
      {/* Toggle Button */}
      <button
        onClick={toggleSidebar}
        style={{
          background: "none",
          color: "#ecf0f1",
          border: "none",
          marginLeft: "1rem",
          fontSize: "1.5rem",
          cursor: "pointer",
          padding: "0.5rem",
          borderRadius: "4px",
          transition: "background-color 0.2s",
        }}
        onMouseEnter={(e) => e.target.style.backgroundColor = "#34495e"}
        onMouseLeave={(e) => e.target.style.backgroundColor = "transparent"}
        title={isOpen ? "Collapse sidebar" : "Expand sidebar"}
      >
        ☰
      </button>

      {/* User Role Badge */}
      {isOpen && currentUser && (
        <div style={{
          margin: "1rem",
          padding: "0.5rem",
          backgroundColor: "#34495e",
          borderRadius: "6px",
          textAlign: "center",
          fontSize: "0.8rem"
        }}>
          <div style={{ opacity: 0.7 }}>Logged in as</div>
          <div style={{ fontWeight: "bold", color: "#3498db" }}>
            {currentUser.role}
          </div>
        </div>
      )}

      {/* Menu Items */}
      <ul style={{ 
        listStyle: "none", 
        padding: "1rem", 
        margin: 0,
        flex: 1,
        overflowY: "auto"
      }}>
        {filteredMenuItems.map((item) => (
          <React.Fragment key={item.key}>
            <li
              style={{
                padding: "0.75rem 0.5rem",
                whiteSpace: "nowrap",
                overflow: "hidden",
                textOverflow: "ellipsis",
                cursor: "pointer",
                display: "flex",
                justifyContent: "space-between",
                alignItems: "center",
                borderRadius: "6px",
                transition: "background-color 0.2s",
                marginBottom: "0.25rem",
              }}
              onMouseEnter={(e) => e.target.style.backgroundColor = "#34495e"}
              onMouseLeave={(e) => e.target.style.backgroundColor = "transparent"}
              onClick={() => handleNavigation(item)}
            >
              <span style={{ fontSize: "0.95rem" }}>
                {isOpen ? item.name : item.name.charAt(0)}
              </span>
              {isOpen && item.subItems && (
                <span style={{ 
                  marginLeft: "0.5rem",
                  fontSize: "0.8rem",
                  transition: "transform 0.2s"
                }}>
                  {expandedMenus[item.key] ? "▼" : "►"}
                </span>
              )}
            </li>
            
            {isOpen && item.subItems && expandedMenus[item.key] && (
              <ul style={{ 
                listStyle: "none", 
                paddingLeft: "1.5rem", 
                margin: "0 0 0.5rem 0",
                borderLeft: "2px solid #34495e"
              }}>
                {item.subItems.map(subItem => (
                  <li
                    key={subItem.key}
                    style={{
                      padding: "0.5rem 0.75rem",
                      whiteSpace: "nowrap",
                      overflow: "hidden",
                      textOverflow: "ellipsis",
                      cursor: "pointer",
                      fontSize: "0.85rem",
                      borderRadius: "4px",
                      transition: "background-color 0.2s",
                      marginBottom: "0.125rem",
                    }}
                    onMouseEnter={(e) => e.target.style.backgroundColor = "#34495e"}
                    onMouseLeave={(e) => e.target.style.backgroundColor = "transparent"}
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

      {/* Logout Button */}
      <div style={{ 
        padding: "1rem",
        borderTop: "1px solid #34495e",
        marginTop: "auto"
      }}>
        <button
          onClick={handleLogout}
          disabled={isLoggingOut}
          style={{
            width: "100%",
            background: isLoggingOut ? "#7f8c8d" : "#e74c3c",
            color: "#fff",
            border: "none",
            padding: isOpen ? "0.75rem 1rem" : "0.75rem",
            borderRadius: "6px",
            cursor: isLoggingOut ? "not-allowed" : "pointer",
            fontSize: isOpen ? "0.9rem" : "1.2rem",
            fontWeight: "600",
            transition: "all 0.2s",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            gap: "0.5rem",
          }}
          onMouseEnter={(e) => {
            if (!isLoggingOut) {
              e.target.style.backgroundColor = "#c0392b";
            }
          }}
          onMouseLeave={(e) => {
            if (!isLoggingOut) {
              e.target.style.backgroundColor = "#e74c3c";
            }
          }}
          title="Logout"
        >
          {isLoggingOut ? (
            <>
              {isOpen && (
                <>
                  <div style={{
                    width: "16px",
                    height: "16px",
                    border: "2px solid #fff",
                    borderTop: "2px solid transparent",
                    borderRadius: "50%",
                    animation: "spin 1s linear infinite"
                  }}></div>
                  Logging out...
                </>
              )}
              {!isOpen && "⏳"}
            </>
          ) : (
            <>
              <span>🚪</span>
              {isOpen && "Logout"}
            </>
          )}
        </button>
      </div>
    </div>
  );
};

export default Sidebar;