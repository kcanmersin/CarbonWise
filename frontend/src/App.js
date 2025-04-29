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
import { checkAuthStatus } from "./services/authService";

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  
  // Your project name
  const projectName = "Resource Management System";

  // Check if user is already logged in when app loads
  useEffect(() => {
    const authStatus = checkAuthStatus();
    setIsLoggedIn(authStatus);
    setIsLoading(false);
  }, []);

  const handleSuccessfulLogin = (userData) => {
    console.log("Login successful:", userData);
    setIsLoggedIn(true);
  };

  // Protected route component
  const ProtectedRoute = ({ children }) => {
    if (!isLoggedIn) {
      return <Navigate to="/login" />;
    }
    return children;
  };

  if (isLoading) {
    return (
      <div style={{ 
        display: "flex", 
        justifyContent: "center", 
        alignItems: "center", 
        height: "100vh" 
      }}>
        Loading...
      </div>
    );
  }

  return (
    <Router>
      <div>
        <Helmet>
          <title>{projectName}</title>
          <meta name="description" content="Resource and carbon footprint management application" />
        </Helmet>
        
        <Routes>
          <Route path="/login" element={
            isLoggedIn ? <Navigate to="/" /> : <LoginPage onLoginSuccess={handleSuccessfulLogin} />
          } />
          
          <Route path="/" element={
            <ProtectedRoute>
              <Dashboard />
            </ProtectedRoute>
          } />
          
          <Route path="/electricity" element={
            <ProtectedRoute>
              <ElectricityPage />
            </ProtectedRoute>
          } />
          
          <Route path="/water" element={
            <ProtectedRoute>
              <WaterPage />
            </ProtectedRoute>
          } />
          
          <Route path="/paper" element={
            <ProtectedRoute>
              <PaperPage />
            </ProtectedRoute>
          } />
          
          <Route path="/natural-gas" element={
            <ProtectedRoute>
              <NaturalGasPage />
            </ProtectedRoute>
          } />
          
          <Route path="/carbon-footprint/calculations" element={
            <ProtectedRoute>
              <CarbonFootprintCalculations />
            </ProtectedRoute>
          } />

          <Route path="/carbon-footprint/test" element={
            <ProtectedRoute>
              <CarbonFootprintTestPage />
            </ProtectedRoute>
          } />

          <Route path="/reports" element={
            <ProtectedRoute>
              <ReportsPage />
            </ProtectedRoute>
          } />

          <Route path="/admin" element={
            <ProtectedRoute>
              <AdminTools />
            </ProtectedRoute>
          } />

          <Route path="/admin/buildings" element={
            <ProtectedRoute>
              <BuildingsManagement />
            </ProtectedRoute>
          } />

          <Route path="/admin/consumption-types" element={
            <ProtectedRoute>
              <ConsumptionTypesManagement />
            </ProtectedRoute>
          } />

          <Route path="/admin/school-info" element={
            <ProtectedRoute>
              <SchoolInfoPage />
            </ProtectedRoute>
          } />

          {/* Fallback route */}
          <Route path="*" element={<Navigate to="/" />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;