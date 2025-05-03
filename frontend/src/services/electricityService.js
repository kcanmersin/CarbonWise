const API_URL = "http://localhost:5067/api"; // Adjust this URL to match your actual API base URL

export const getElectricByBuilding = async (buildingId) => {
  try {
    const response = await fetch(`${API_URL}/Electrics/building/${buildingId}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch electricity data");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching electricity data:", error);
    throw error;
  }
};

export const getElectricById = async (id) => {
  try {
    const response = await fetch(`${API_URL}/Electrics/${id}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch electricity record");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching electricity record:", error);
    throw error;
  }
};

export const getMonthlyAggregate = async (startDate, endDate) => {
  try {
    // Create query string from parameters
    const queryParams = new URLSearchParams();
    if (startDate) queryParams.append("startDate", startDate.toISOString());
    if (endDate) queryParams.append("endDate", endDate.toISOString());

    const response = await fetch(`${API_URL}/Electrics/monthly-aggregate?${queryParams}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch monthly aggregate data");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching monthly aggregate data:", error);
    throw error;
  }
};

export const getMonthlyTotals = async (startDate, endDate) => {
  try {
    // Create query string from parameters
    const queryParams = new URLSearchParams();
    if (startDate) queryParams.append("startDate", startDate.toISOString());
    if (endDate) queryParams.append("endDate", endDate.toISOString());

    const response = await fetch(`${API_URL}/Electrics/monthly-totals?${queryParams}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch monthly totals data");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching monthly totals data:", error);
    throw error;
  }
};

export const filterElectrics = async (filterParams) => {
  try {
    // Create query string from filter parameters
    const queryParams = new URLSearchParams();
    if (filterParams.buildingId) queryParams.append("BuildingId", filterParams.buildingId);
    if (filterParams.startDate) queryParams.append("StartDate", filterParams.startDate.toISOString());
    if (filterParams.endDate) queryParams.append("EndDate", filterParams.endDate.toISOString());

    const response = await fetch(`${API_URL}/Electrics/filter?${queryParams}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to filter electricity data");
    }

    return await response.json();
  } catch (error) {
    console.error("Error filtering electricity data:", error);
    throw error;
  }
};

export const createElectricRecord = async (electricData) => {
  try {
    const response = await fetch(`${API_URL}/Electrics`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      },
      body: JSON.stringify(electricData)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to create electricity record");
    }

    return await response.json();
  } catch (error) {
    console.error("Error creating electricity record:", error);
    throw error;
  }
};

export const updateElectricRecord = async (id, electricData) => {
  try {
    const response = await fetch(`${API_URL}/Electrics/${id}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      },
      body: JSON.stringify(electricData)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to update electricity record");
    }

    return await response.json();
  } catch (error) {
    console.error("Error updating electricity record:", error);
    throw error;
  }
};

export const deleteElectricRecord = async (id) => {
  try {
    const response = await fetch(`${API_URL}/Electrics/${id}`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to delete electricity record");
    }

    return true;
  } catch (error) {
    console.error("Error deleting electricity record:", error);
    throw error;
  }
};

export const electricitydownloadSampleExcel = async () => {
  try {
    const response = await fetch(`${API_URL}/Electrics/downloadSampleExcel`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`, // if using authentication
      },
    });
    
    if (!response.ok) throw new Error('Failed to download template');
    
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Electricity_Template.xlsx`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  } catch (err) {
    console.error(`Failed to download electricity template: ${err.message}`);
    throw err; // Rethrow the error for further handling if needed
  }
};

export const electricityMultipleUpload = async (file) => {
  try {
    const formData = new FormData();
    formData.append("file", file);

    const response = await fetch(`${API_URL}/Electrics/multiple`, {
      method: "POST",
      headers: {
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      },
      body: formData
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to upload electricity data");
    }

    return await response.json();
  } catch (error) {
    console.error("Error uploading electricity data:", error);
    throw error;
  }
}