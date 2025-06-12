import React, { useState, useEffect, useRef } from "react";
import Sidebar from "../components/Sidebar";
import {
  getSchoolInfoByYear,
  createSchoolInfo,
  updateSchoolInfo,
  // getAllSchoolInfo // No longer needed for initial load
} from "../services/schoolInfoService";

export default function SchoolInfoPage() {
  // Removed allSchoolInfos state as we fetch on demand
  // const [allSchoolInfos, setAllSchoolInfos] = useState([]);
  const [selectedInfo, setSelectedInfo] = useState(null);
  const [isLoading, setIsLoading] = useState(false); // Controls loading state for specific year fetch
  const [isSubmitting, setIsSubmitting] = useState(false); // Controls loading state for form submission
  const [error, setError] = useState(null);
  const [editingInfo, setEditingInfo] = useState(null);
  const [selectedYear, setSelectedYear] = useState(""); // Start with no year selected
  const formRef = useRef(null);

  // Form data remains the same
  const [formData, setFormData] = useState({
    numberOfPeople: 0,
    year: new Date().getFullYear(), // Default to current year for *new* entries
    campusVehicleEntry: {
      carsManagedByUniversity: 0,
      carsEnteringUniversity: 0,
      motorcyclesEnteringUniversity: 0
    }
  });

  // Generate years from 2022 to 2025 (or dynamically if preferred)
  // Consider fetching available years from an API if the list is dynamic
  const years = [2022, 2023, 2024, 2025];

  // Menu items for sidebar remain the same
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

  // --- REMOVED ---
  // Initial fetch of all data is removed.
  // useEffect(() => {
  //   fetchSchoolInfoData();
  // }, []);

  // Fetch school info *only* when selectedYear changes and is not empty
  useEffect(() => {
    if (selectedYear) {
      // Clear previous data and errors before fetching new year
      setSelectedInfo(null);
      setError(null);
      fetchSchoolInfoByYear(parseInt(selectedYear));
    } else {
      // Clear data if no year is selected
      setSelectedInfo(null);
      setError(null); // Clear any previous errors
    }
  }, [selectedYear]); // Dependency array ensures this runs only when selectedYear changes


  // --- REMOVED ---
  // The fetchSchoolInfoData function (which fetched all records) is removed.
  // const fetchSchoolInfoData = async () => { ... };


  // Fetch school info for a specific year
  const fetchSchoolInfoByYear = async (year) => {
    if (!year) return;

    setIsLoading(true); // Start loading indicator for fetching year data
    // setError(null); // Already cleared in the useEffect hook
    try {
      const info = await getSchoolInfoByYear(year);
      setSelectedInfo(info);
    } catch (err) {
      // Assuming the service throws an error (e.g., 404) when not found
      setSelectedInfo(null); // Ensure no stale data is shown
      setError(`No school information found for the year ${year}. You can add it using the form below.`);
      console.error(`Error fetching school info for year ${year}:`, err);
    } finally {
      setIsLoading(false); // Stop loading indicator
    }
  };

  // Handle year selection change
  const handleYearChange = (e) => {
    setSelectedYear(e.target.value); // This will trigger the useEffect hook
  };

  // Handle form input changes - unchanged
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    const numValue = name !== "year" ? parseInt(value) || 0 : parseInt(value);

    setFormData({
      ...formData,
      [name]: numValue
    });
  };

  // Handle vehicle entry input changes - unchanged
  const handleVehicleInputChange = (e) => {
    const { name, value } = e.target;
    const numValue = parseInt(value) || 0;

    setFormData({
      ...formData,
      campusVehicleEntry: {
        ...formData.campusVehicleEntry,
        [name]: numValue
      }
    });
  };

  // Reset form to default values - unchanged
  const resetForm = () => {
    setFormData({
      numberOfPeople: 0,
      year: new Date().getFullYear(), // Reset year to current for potential new entry
      campusVehicleEntry: {
        carsManagedByUniversity: 0,
        carsEnteringUniversity: 0,
        motorcyclesEnteringUniversity: 0
      }
    });
    setEditingInfo(null); // Exit edit mode
    setError(null); // Clear form-related errors
  };

  // Handle edit school info - Minor adjustment to use selectedInfo directly if editing the selected year
  const handleEdit = (infoToEdit) => {
    setEditingInfo(infoToEdit); // Keep track of the original record being edited (including its ID)

    // Set form data based on the info being edited
    setFormData({
      numberOfPeople: infoToEdit.numberOfPeople,
      year: infoToEdit.year, // Keep the original year
      campusVehicleEntry: infoToEdit.campusVehicleEntry ? {
        carsManagedByUniversity: infoToEdit.campusVehicleEntry.carsManagedByUniversity || 0,
        carsEnteringUniversity: infoToEdit.campusVehicleEntry.carsEnteringUniversity || 0,
        motorcyclesEnteringUniversity: infoToEdit.campusVehicleEntry.motorcyclesEnteringUniversity || 0
      } : {
        carsManagedByUniversity: 0,
        carsEnteringUniversity: 0,
        motorcyclesEnteringUniversity: 0
      }
    });

    // Scroll to form
    if (formRef.current) {
      formRef.current.scrollIntoView({
        behavior: 'smooth',
        block: 'start'
      });
    }
  };

  // Handle form submission - Adjusted loading state and refresh logic
  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSubmitting(true); // Use separate loading state for submission
    setError(null);

    try {
      let successfullySubmittedYear;
      if (editingInfo) {
        // Prepare update data - year is not updatable via API usually
        const updateData = {
          numberOfPeople: formData.numberOfPeople,
          campusVehicleEntry: formData.campusVehicleEntry
        };
        // Update existing record
        await updateSchoolInfo(editingInfo.id, updateData);
        successfullySubmittedYear = editingInfo.year;
      } else {
        // Create new record
        await createSchoolInfo(formData);
        successfullySubmittedYear = formData.year;
      }

      resetForm(); // Reset form after successful submission

      // Refresh the data for the year that was just added/updated *if* it's the currently selected year
      // Or, if a new record was added, maybe select that year automatically? (Optional behavior)
      if (successfullySubmittedYear.toString() === selectedYear) {
         await fetchSchoolInfoByYear(successfullySubmittedYear); // Use await to ensure fetch completes
      } else {
        // Optional: If a new year was added, automatically select it in the dropdown
        // setSelectedYear(successfullySubmittedYear.toString());
      }

    } catch (err) {
      setError(editingInfo
        ? "Failed to update school information. Please check the details and try again."
        : "Failed to create school information. A record for this year might already exist, or there was a server issue.");
      console.error("Error saving school info:", err);
    } finally {
      setIsSubmitting(false); // Stop submission loading indicator
    }
  };

  // Format year for display - unchanged
  const formatYear = (year) => {
    return year.toString();
  };

  // --- JSX Structure remains largely the same ---
  // Key changes are in the "School Info Records Section" conditional rendering
  return (
    <div style={{ display: "flex", minHeight: "100vh" }}>
      <Sidebar menuItems={menuItems} />
      <div style={{ flex: 1, padding: "2rem", backgroundColor: "#f5f7fa", overflowY: "auto" }}>
        {/* Header - unchanged */}
        <div style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          marginBottom: "2rem"
        }}>
          <div>
            <h1 style={{ color: "#2c3e50", margin: "0 0 0.5rem 0" }}>School Information Management</h1>
            <p style={{ color: "#7f8c8d", margin: 0 }}>Add or update annual school population and vehicle information</p>
          </div>
        </div>

        {/* Error message display (consolidated for fetch and submit errors) */}
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

        {/* Form for adding/editing school info - unchanged */}
        <div
          ref={formRef}
          style={{
            backgroundColor: "white",
            padding: "1.5rem",
            borderRadius: "8px",
            boxShadow: "0 2px 4px rgba(0, 0, 0, 0.1)",
            marginBottom: "2rem",
            border: editingInfo ? "2px solid #3498db" : "none" // Highlight when editing
          }}
        >
          <h2 style={{
            margin: "0 0 1.5rem 0",
            color: "#3498db",
            display: "flex",
            alignItems: "center",
            gap: "0.5rem",
            backgroundColor: editingInfo ? "#ebf5fb" : "transparent",
            padding: editingInfo ? "0.5rem 1rem" : "0",
            borderRadius: editingInfo ? "4px" : "0"
          }}>
            <span style={{ fontSize: "1.5rem" }}>🏫</span>
            {editingInfo ? `Edit School Information (${editingInfo.year})` : "Add New School Information"}
          </h2>

          <form onSubmit={handleSubmit}>
            <div style={{
              display: "grid",
              gridTemplateColumns: "repeat(auto-fill, minmax(250px, 1fr))",
              gap: "1rem",
              marginBottom: "1.5rem"
            }}>
              {/* Year Input */}
              <div>
                <label htmlFor="year" style={{ display: "block", marginBottom: "0.5rem", fontWeight: "bold", color: "#34495e" }}>
                  Academic Year*
                </label>
                <input
                  type="number"
                  id="year"
                  name="year"
                  value={formData.year}
                  onChange={handleInputChange}
                  required
                  min="2000"
                  max="2100"
                  disabled={editingInfo !== null} // Disable year when editing
                  style={{
                    width: "100%", padding: "0.75rem", borderRadius: "4px", border: "1px solid #ddd", boxSizing: "border-box",
                    backgroundColor: editingInfo ? "#f5f5f5" : "white"
                  }}
                  placeholder="e.g., 2024"
                />
                {editingInfo && (
                  <small style={{ color: "#7f8c8d" }}>Year cannot be changed when editing.</small>
                )}
              </div>

              {/* Number of People Input */}
              <div>
                <label htmlFor="numberOfPeople" style={{ display: "block", marginBottom: "0.5rem", fontWeight: "bold", color: "#34495e" }}>
                  Number of People*
                </label>
                <input
                  type="number"
                  id="numberOfPeople"
                  name="numberOfPeople"
                  value={formData.numberOfPeople}
                  onChange={handleInputChange}
                  required
                  min="0"
                  style={{ width: "100%", padding: "0.75rem", borderRadius: "4px", border: "1px solid #ddd", boxSizing: "border-box" }}
                  placeholder="Total students and staff"
                />
              </div>

              {/* Campus Vehicle Entry Section */}
               <div style={{ gridColumn: "1 / -1" }}>
                 <h3 style={{ color: "#34495e", marginTop: "1rem", marginBottom: "1rem" }}>
                   Campus Vehicle Information
                 </h3>
               </div>

              {/* Cars Managed By University */}
              <div>
                 <label htmlFor="carsManagedByUniversity" style={{ display: "block", marginBottom: "0.5rem", fontWeight: "bold", color: "#34495e" }}>
                   Cars Managed By University
                 </label>
                 <input
                   type="number" id="carsManagedByUniversity" name="carsManagedByUniversity"
                   value={formData.campusVehicleEntry.carsManagedByUniversity} onChange={handleVehicleInputChange} min="0"
                   style={{ width: "100%", padding: "0.75rem", borderRadius: "4px", border: "1px solid #ddd", boxSizing: "border-box" }}
                 />
               </div>

              {/* Cars Entering University */}
              <div>
                 <label htmlFor="carsEnteringUniversity" style={{ display: "block", marginBottom: "0.5rem", fontWeight: "bold", color: "#34495e" }}>
                   Cars Entering University
                 </label>
                 <input
                   type="number" id="carsEnteringUniversity" name="carsEnteringUniversity"
                   value={formData.campusVehicleEntry.carsEnteringUniversity} onChange={handleVehicleInputChange} min="0"
                   style={{ width: "100%", padding: "0.75rem", borderRadius: "4px", border: "1px solid #ddd", boxSizing: "border-box" }}
                 />
               </div>

               {/* Motorcycles Entering University */}
              <div>
                 <label htmlFor="motorcyclesEnteringUniversity" style={{ display: "block", marginBottom: "0.5rem", fontWeight: "bold", color: "#34495e" }}>
                   Motorcycles Entering University
                 </label>
                 <input
                   type="number" id="motorcyclesEnteringUniversity" name="motorcyclesEnteringUniversity"
                   value={formData.campusVehicleEntry.motorcyclesEnteringUniversity} onChange={handleVehicleInputChange} min="0"
                   style={{ width: "100%", padding: "0.75rem", borderRadius: "4px", border: "1px solid #ddd", boxSizing: "border-box" }}
                 />
               </div>
            </div>

            {/* Form Action Buttons */}
            <div style={{ display: "flex", gap: "1rem", justifyContent: "flex-end" }}>
              {editingInfo && (
                <button
                  type="button" onClick={resetForm}
                  style={{ padding: "0.75rem 1.5rem", backgroundColor: "#e74c3c", color: "white", border: "none", borderRadius: "4px", cursor: "pointer", fontWeight: "bold" }}
                >
                  Cancel Edit
                </button>
              )}
              <button
                type="submit"
                disabled={isSubmitting} // Use isSubmitting state here
                style={{
                  padding: "0.75rem 1.5rem", backgroundColor: "#3498db", color: "white", border: "none", borderRadius: "4px",
                  cursor: isSubmitting ? "not-allowed" : "pointer", fontWeight: "bold", opacity: isSubmitting ? 0.7 : 1, minWidth: "150px"
                }}
              >
                {isSubmitting ? "Saving..." : (editingInfo ? "Update Information" : "Add Information")}
              </button>
            </div>
          </form>
        </div>

        {/* School Info Records Section - Updated Logic */}
        <div style={{
          backgroundColor: "white",
          padding: "1.5rem",
          borderRadius: "8px",
          boxShadow: "0 2px 4px rgba(0, 0, 0, 0.1)"
        }}>
          <h2 style={{ color: "#2c3e50", marginBottom: "1.5rem", display: "flex", alignItems: "center", gap: "0.5rem" }}>
            <span style={{ fontSize: "1.2rem" }}>📋</span>
            School Information Records
          </h2>

          {/* Year Selector - Unchanged */}
          <div style={{ marginBottom: "2rem" }}>
            <label htmlFor="yearSelector" style={{ display: "block", marginBottom: "0.5rem", fontWeight: "bold", color: "#34495e" }}>
              Select Academic Year to View/Edit
            </label>
            <select
              id="yearSelector"
              value={selectedYear}
              onChange={handleYearChange}
              style={{ width: "100%", padding: "0.75rem", borderRadius: "4px", border: "1px solid #ddd", backgroundColor: "white", fontSize: "1rem", boxSizing: "border-box" }}
            >
              <option value="">-- Select a Year --</option>
              {years.map(year => (
                <option key={year} value={year.toString()}>{year}</option>
              ))}
            </select>
          </div>

          {/* --- Updated Conditional Rendering Logic --- */}

          {/* 1. Initial State / No Year Selected */}
          {!selectedYear && (
            <div style={{ textAlign: "center", padding: "2rem", color: "#7f8c8d" }}>
              <p>Please select an academic year from the dropdown above to view its information.</p>
            </div>
          )}

          {/* 2. Year Selected, Loading Data */}
          {selectedYear && isLoading && (
            <div style={{ textAlign: "center", padding: "2rem" }}>
              <div style={{
                 display: "inline-block", width: "40px", height: "40px", border: "4px solid #f3f3f3",
                 borderTop: "4px solid #3498db", borderRadius: "50%", animation: "spin 1s linear infinite"
               }}></div>
               <style>{`@keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }`}</style>
              <p style={{ marginTop: "1rem" }}>Loading information for {selectedYear}...</p>
            </div>
          )}

          {/* 3. Year Selected, Loading Finished, No Data Found (Error state handled by the main error display now) */}
          {/* Error message is displayed at the top */}
          {/* {selectedYear && !isLoading && error && !selectedInfo && ( ... )} // This state is covered by the main error display */}


          {/* 4. Year Selected, Loading Finished, Data Found */}
          {selectedYear && !isLoading && selectedInfo && (
            <div style={{ backgroundColor: "#f8f9fa", padding: "1.5rem", borderRadius: "8px" }}>
              {/* Header with Edit Button */}
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "1.5rem" }}>
                 <h3 style={{ margin: 0, color: "#2c3e50" }}>
                   {selectedInfo.year} School Information
                 </h3>
                 <button
                   onClick={() => handleEdit(selectedInfo)} // Pass the currently displayed info to edit
                   style={{
                     padding: "0.5rem 1rem", backgroundColor: "#3498db", color: "white", border: "none", borderRadius: "4px",
                     cursor: "pointer", fontWeight: "bold", display: "flex", alignItems: "center", gap: "0.5rem"
                   }}
                 >
                   <span>✏️</span> Edit
                 </button>
               </div>

              {/* Data Display Grid */}
              <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(250px, 1fr))", gap: "1.5rem" }}>
                {/* Population Card */}
                <div style={{ backgroundColor: "white", padding: "1rem", borderRadius: "8px", boxShadow: "0 1px 3px rgba(0,0,0,0.1)" }}>
                   <h4 style={{ color: "#7f8c8d", margin: "0 0 0.5rem 0" }}>School Population</h4>
                   <p style={{ fontSize: "1.5rem", fontWeight: "bold", margin: 0, color: "#2c3e50" }}>
                     {selectedInfo.numberOfPeople.toLocaleString()} people
                   </p>
                 </div>
                {/* Vehicle Cards (conditional) */}
                {selectedInfo.campusVehicleEntry && (
                   <>
                    <div style={{ backgroundColor: "white", padding: "1rem", borderRadius: "8px", boxShadow: "0 1px 3px rgba(0,0,0,0.1)" }}>
                      <h4 style={{ color: "#7f8c8d", margin: "0 0 0.5rem 0" }}>University-Managed Cars</h4>
                      <p style={{ fontSize: "1.5rem", fontWeight: "bold", margin: 0, color: "#2c3e50" }}>
                        {(selectedInfo.campusVehicleEntry.carsManagedByUniversity || 0).toLocaleString()} vehicles
                      </p>
                    </div>
                    <div style={{ backgroundColor: "white", padding: "1rem", borderRadius: "8px", boxShadow: "0 1px 3px rgba(0,0,0,0.1)" }}>
                      <h4 style={{ color: "#7f8c8d", margin: "0 0 0.5rem 0" }}>Campus-Entering Cars</h4>
                      <p style={{ fontSize: "1.5rem", fontWeight: "bold", margin: 0, color: "#2c3e50" }}>
                        {(selectedInfo.campusVehicleEntry.carsEnteringUniversity || 0).toLocaleString()} vehicles
                      </p>
                    </div>
                    <div style={{ backgroundColor: "white", padding: "1rem", borderRadius: "8px", boxShadow: "0 1px 3px rgba(0,0,0,0.1)" }}>
                      <h4 style={{ color: "#7f8c8d", margin: "0 0 0.5rem 0" }}>Campus-Entering Motorcycles</h4>
                      <p style={{ fontSize: "1.5rem", fontWeight: "bold", margin: 0, color: "#2c3e50" }}>
                        {(selectedInfo.campusVehicleEntry.motorcyclesEnteringUniversity || 0).toLocaleString()} vehicles
                      </p>
                    </div>
                  </>
                 )}
              </div>
            </div>
          )}

        </div> {/* End School Info Records Section */}
      </div> {/* End Main Content Area */}
    </div> /* End Flex Container */
  );
}