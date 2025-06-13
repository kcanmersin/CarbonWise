import React, { useState, useEffect } from "react";
import { Helmet } from "react-helmet";
import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import Dashboard from "./pages/Dashboard";
import ElectricityPage from "./pages/ElectricityPage";
import WaterPage from "./pages/WaterPage";
import PaperPage from "./pages/PaperPage";
import NaturalGasPage from "./pages/NaturalGasPage";
import CarbonFootprintCalculations from "./pages/CarbonFootprintCalculations";
import CarbonFootprintTestPage from "./pages/CarbonFootprintTestPage";
import ReportsPage from "./pages/ReportsPage";
import AdminTools from "./pages/AdminTools";
import BuildingsManagement from "./pages/BuildingManagementPage";
import ConsumptionTypesManagement from "./pages/ConsumptionTypeManagementPage";
import SchoolInfoPage from "./pages/SchoolInfoManagementPage";
import UserManagementPage from "./pages/UserManagementPage";
import PredictionsPage from "./pages/PredictionsPage";
import ProtectedRoute from "./components/ProtectedRoute"; // Import the new ProtectedRoute
import { getCurrentUserInfo } from "./services/userService"; // Import to check token validity
import { getAuthToken, clearAuthData } from "./services/authService"; // Import auth utilities

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [user, setUser] = useState(null);
  const [isCheckingAuth, setIsCheckingAuth] = useState(true);
  
  // Your project name
  const projectName = "Resource Management System";

  // Check for existing authentication on app load
  useEffect(() => {
    const checkExistingAuth = async () => {
      try {
        // Check if there's a token in localStorage
        const token = getAuthToken();
        
        if (!token) {
          // No token found, user is not logged in
          setIsLoggedIn(false);
          setUser(null);
          setIsCheckingAuth(false);
          return;
        }

        // Token exists, verify it's still valid by calling the API
        const userInfo = await getCurrentUserInfo();
        
        if (userInfo) {
          // Token is valid, set user as logged in
          setUser(userInfo);
          setIsLoggedIn(true);
          console.log("User authenticated from stored token:", userInfo);
        } else {
          // Token is invalid, clear it
          clearAuthData();
          setIsLoggedIn(false);
          setUser(null);
        }
      } catch (error) {
        console.error("Error checking authentication:", error);
        // Token is invalid or API error, clear auth data
        clearAuthData();
        setIsLoggedIn(false);
        setUser(null);
      } finally {
        setIsCheckingAuth(false);
      }
    };

    checkExistingAuth();
  }, []);

  const handleSuccessfulLogin = (authResult) => {
    console.log("Login successful:", authResult);
    
    // Handle different response formats from backend
    if (authResult && authResult.user) {
      setUser(authResult.user);
      setIsLoggedIn(true);
      console.log("User logged in:", authResult.user);
    } else if (authResult && authResult.success && authResult.user) {
      setUser(authResult.user);
      setIsLoggedIn(true);
      console.log("User logged in:", authResult.user);
    } else {
      console.error("Invalid login result:", authResult);
    }
  };

  const handleLogout = () => {
    clearAuthData(); // Clear token and user data
    setIsLoggedIn(false);
    setUser(null);
    console.log("User logged out");
  };

  // Show loading screen while checking authentication
  if (isCheckingAuth) {
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
          <p style={{ marginTop: "1rem", color: "#7f8c8d", fontSize: "1.1rem" }}>
            Loading...
          </p>
        </div>
      </div>
    );
  }

  // Simple authentication check for login redirect
  const AuthCheck = ({ children }) => {
    if (!isLoggedIn) {
      return <Navigate to="/auth" />;
    }
    return children;
  };

  return (
    <Router>
      <div>
        <Helmet>
          <title>{projectName}</title>
          <meta name="description" content="Resource and carbon footprint management application" />
        </Helmet>
        
        <Routes>
          <Route 
            path="/auth" 
            element={
              isLoggedIn ? 
                <Navigate to="/" /> : 
                <LoginPage onLoginSuccess={handleSuccessfulLogin} />
            } 
          />
          
          {/* Dashboard - accessible to all authenticated users */}
          <Route path="/" element={
            <AuthCheck>
              <ProtectedRoute allowedRoles={["User", "Admin", "SuperUser"]}>
                <Dashboard user={user} onLogout={handleLogout} />
              </ProtectedRoute>
            </AuthCheck>
          } />
          
          {/* Resource Monitoring Pages - accessible to all authenticated users */}
          <Route path="/electricity" element={
            <AuthCheck>
              <ProtectedRoute allowedRoles={["User", "Admin", "SuperUser"]}>
                <ElectricityPage user={user} onLogout={handleLogout} />
              </ProtectedRoute>
            </AuthCheck>
          } />
          
          <Route path="/water" element={
            <AuthCheck>
              <ProtectedRoute allowedRoles={["User", "Admin", "SuperUser"]}>
                <WaterPage user={user} onLogout={handleLogout} />
              </ProtectedRoute>
            </AuthCheck>
          } />
          
          <Route path="/paper" element={
            <AuthCheck>
              <ProtectedRoute allowedRoles={["User", "Admin", "SuperUser"]}>
                <PaperPage user={user} onLogout={handleLogout} />
              </ProtectedRoute>
            </AuthCheck>
          } />
          
          <Route path="/natural-gas" element={
            <AuthCheck>
              <ProtectedRoute allowedRoles={["User", "Admin", "SuperUser"]}>
                <NaturalGasPage user={user} onLogout={handleLogout} />
              </ProtectedRoute>
            </AuthCheck>
          } />
          
          {/* Carbon Footprint Pages - accessible to all authenticated users */}
          <Route path="/carbon-footprint/calculations" element={
            <AuthCheck>
              <ProtectedRoute allowedRoles={["User", "Admin", "SuperUser"]}>
                <CarbonFootprintCalculations user={user} onLogout={handleLogout} />
              </ProtectedRoute>
            </AuthCheck>
          } />

          <Route path="/carbon-footprint/test" element={
            <AuthCheck>
              <ProtectedRoute allowedRoles={["User", "Admin", "SuperUser"]}>
                <CarbonFootprintTestPage user={user} onLogout={handleLogout} />
              </ProtectedRoute>
            </AuthCheck>
          } />

          {/* Reports - accessible to all authenticated users */}
          <Route path="/reports" element={
            <AuthCheck>
              <ProtectedRoute allowedRoles={["Admin", "SuperUser"]}>
                <ReportsPage user={user} onLogout={handleLogout} />
              </ProtectedRoute>
            </AuthCheck>
          } />

          {/* Predictions - accessible to Admin and SuperUser only */}
          <Route path="/predictions" element={
            <AuthCheck>
              <ProtectedRoute allowedRoles={["User", "Admin", "SuperUser"]}>
                <PredictionsPage user={user} onLogout={handleLogout} />
              </ProtectedRoute>
            </AuthCheck>
          } />

          {/* Admin Tools - accessible to Admin and SuperUser */}
          <Route path="/admin" element={
            <AuthCheck>
              <ProtectedRoute allowedRoles={["Admin", "SuperUser"]}>
                <AdminTools user={user} onLogout={handleLogout} />
              </ProtectedRoute>
            </AuthCheck>
          } />

          <Route path="/admin/buildings" element={
            <AuthCheck>
              <ProtectedRoute allowedRoles={["Admin", "SuperUser"]}>
                <BuildingsManagement user={user} onLogout={handleLogout} />
              </ProtectedRoute>
            </AuthCheck>
          } />

          <Route path="/admin/consumption-types" element={
            <AuthCheck>
              <ProtectedRoute allowedRoles={["Admin", "SuperUser"]}>
                <ConsumptionTypesManagement user={user} onLogout={handleLogout} />
              </ProtectedRoute>
            </AuthCheck>
          } />

          <Route path="/admin/school-info" element={
            <AuthCheck>
              <ProtectedRoute allowedRoles={["Admin", "SuperUser"]}>
                <SchoolInfoPage user={user} onLogout={handleLogout} />
              </ProtectedRoute>
            </AuthCheck>
          } />

          {/* User Management - accessible to SuperUser only */}
          <Route path="/admin/user-management" element={
            <AuthCheck>
              <ProtectedRoute allowedRoles={["SuperUser"]}>
                <UserManagementPage user={user} onLogout={handleLogout} />
              </ProtectedRoute>
            </AuthCheck>
          } />

          {/* Fallback route - redirect based on authentication status */}
          <Route path="*" element={
            isLoggedIn ? <Navigate to="/" /> : <Navigate to="/auth" />
          } />
        </Routes>
      </div>
    </Router>
  );
}
export default App;