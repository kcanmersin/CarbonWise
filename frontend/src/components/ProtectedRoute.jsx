import React, { useState, useEffect } from "react";
import { Navigate } from "react-router-dom";
import { getCurrentUserInfo } from "../services/userService";

const ProtectedRoute = ({ children, requiredRole = null, allowedRoles = [] }) => {
  const [currentUser, setCurrentUser] = useState(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState(null);

  // Define role hierarchy for comparison
  const roleHierarchy = {
    "User": 0,
    "Admin": 1,
    "SuperUser": 2
  };

  useEffect(() => {
    const checkUserAccess = async () => {
      try {
        setIsLoading(true);
        const userInfo = await getCurrentUserInfo();
        setCurrentUser(userInfo);
      } catch (error) {
        console.error("Error fetching user info:", error);
        setError("Authentication failed");
      } finally {
        setIsLoading(false);
      }
    };

    checkUserAccess();
  }, []);

  // Show loading state
  if (isLoading) {
    return (
      <div style={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        height: "100vh",
        backgroundColor: "#f5f7fa"
      }}>
        <div style={{
          textAlign: "center",
          padding: "2rem",
          backgroundColor: "white",
          borderRadius: "12px",
          boxShadow: "0 4px 6px rgba(0,0,0,0.1)"
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
          <p style={{ marginTop: "1rem", color: "#7f8c8d" }}>Verifying access...</p>
        </div>
      </div>
    );
  }

  // If there's an error or no user, redirect to login
  if (error || !currentUser) {
    return <Navigate to="/auth" />;
  }

  // Check role-based access
  const hasAccess = () => {
    if (!requiredRole && allowedRoles.length === 0) {
      // No specific role requirements, just need to be logged in
      return true;
    }

    const userRole = currentUser.role;
    const userRoleLevel = roleHierarchy[userRole] || 0;

    // Check if user has the required role or higher
    if (requiredRole) {
      const requiredRoleLevel = roleHierarchy[requiredRole] || 0;
      return userRoleLevel >= requiredRoleLevel;
    }

    // Check if user's role is in the allowed roles list
    if (allowedRoles.length > 0) {
      return allowedRoles.includes(userRole);
    }

    return false;
  };

  // If user doesn't have access, show access denied page
  if (!hasAccess()) {
    return (
      <div style={{
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        height: "100vh",
        backgroundColor: "#f5f7fa"
      }}>
        <div style={{
          textAlign: "center",
          padding: "3rem",
          backgroundColor: "white",
          borderRadius: "12px",
          boxShadow: "0 4px 6px rgba(0,0,0,0.1)",
          maxWidth: "500px"
        }}>
          <div style={{ fontSize: "4rem", marginBottom: "1rem" }}>🚫</div>
          <h2 style={{ color: "#e74c3c", marginBottom: "1rem" }}>Access Denied</h2>
          <p style={{ color: "#7f8c8d", marginBottom: "1rem" }}>
            You don't have permission to access this page.
          </p>
          <div style={{
            padding: "1rem",
            backgroundColor: "#f8f9fa",
            borderRadius: "8px",
            marginBottom: "1.5rem"
          }}>
            <p style={{ margin: 0, fontSize: "0.9rem", color: "#495057" }}>
              <strong>Your Role:</strong> {currentUser.role}
              <br />
              <strong>Required:</strong> {requiredRole || allowedRoles.join(", ")}
            </p>
          </div>
          <button
            onClick={() => window.history.back()}
            style={{
              padding: "0.75rem 1.5rem",
              backgroundColor: "#3498db",
              color: "white",
              border: "none",
              borderRadius: "6px",
              cursor: "pointer",
              fontSize: "1rem",
              fontWeight: "600"
            }}
          >
            Go Back
          </button>
        </div>
      </div>
    );
  }

  // User has access, render the protected content
  return children;
};

export default ProtectedRoute;