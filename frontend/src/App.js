import React, { useState } from "react";
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

function App() {
  const [isLoggedIn, setIsLoggedIn] = useState(false);
  const [user, setUser] = useState(null);
  
  // Your project name
  const projectName = "Resource Management System";

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
    setIsLoggedIn(false);
    setUser(null);
    console.log("User logged out");
  };

  // Protected route component
  const ProtectedRoute = ({ children }) => {
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
          
          <Route path="/" element={
            <ProtectedRoute>
              <Dashboard user={user} onLogout={handleLogout} />
            </ProtectedRoute>
          } />
          
          <Route path="/electricity" element={
            <ProtectedRoute>
              <ElectricityPage user={user} onLogout={handleLogout} />
            </ProtectedRoute>
          } />
          
          <Route path="/water" element={
            <ProtectedRoute>
              <WaterPage user={user} onLogout={handleLogout} />
            </ProtectedRoute>
          } />
          
          <Route path="/paper" element={
            <ProtectedRoute>
              <PaperPage user={user} onLogout={handleLogout} />
            </ProtectedRoute>
          } />
          
          <Route path="/natural-gas" element={
            <ProtectedRoute>
              <NaturalGasPage user={user} onLogout={handleLogout} />
            </ProtectedRoute>
          } />
          
          <Route path="/carbon-footprint/calculations" element={
            <ProtectedRoute>
              <CarbonFootprintCalculations user={user} onLogout={handleLogout} />
            </ProtectedRoute>
          } />

          <Route path="/carbon-footprint/test" element={
            <ProtectedRoute>
              <CarbonFootprintTestPage user={user} onLogout={handleLogout} />
            </ProtectedRoute>
          } />

          <Route path="/reports" element={
            <ProtectedRoute>
              <ReportsPage user={user} onLogout={handleLogout} />
            </ProtectedRoute>
          } />

          <Route path="/admin" element={
            <ProtectedRoute>
              <AdminTools user={user} onLogout={handleLogout} />
            </ProtectedRoute>
          } />

          <Route path="/admin/buildings" element={
            <ProtectedRoute>
              <BuildingsManagement user={user} onLogout={handleLogout} />
            </ProtectedRoute>
          } />

          <Route path="/admin/consumption-types" element={
            <ProtectedRoute>
              <ConsumptionTypesManagement user={user} onLogout={handleLogout} />
            </ProtectedRoute>
          } />

          <Route path="/admin/school-info" element={
            <ProtectedRoute>
              <SchoolInfoPage user={user} onLogout={handleLogout} />
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