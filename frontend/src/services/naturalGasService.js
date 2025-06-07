const API_URL = "http://localhost:5067/api"; // Adjust this URL to match your actual API base URL

export const getNaturalGasByBuilding = async (buildingId) => {
  try {
    const response = await fetch(`${API_URL}/NaturalGas/building/${buildingId}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch natural gas data");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching natural gas data:", error);
    throw error;
  }
};

export const getNaturalGasById = async (id) => {
  try {
    const response = await fetch(`${API_URL}/NaturalGas/${id}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch natural gas record");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching natural gas record:", error);
    throw error;
  }
};

export const getMonthlyTotals = async (startDate, endDate) => {
  try {
    // Create query string from parameters
    const queryParams = new URLSearchParams();
    if (startDate) queryParams.append("startDate", startDate.toISOString());
    if (endDate) queryParams.append("endDate", endDate.toISOString());

    const response = await fetch(`${API_URL}/NaturalGas/monthly-totals?${queryParams}`, {
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

export const filterNaturalGas = async (filterParams) => {
  try {
    // Create query string from filter parameters
    const queryParams = new URLSearchParams();
    if (filterParams.buildingId) queryParams.append("BuildingId", filterParams.buildingId);
    if (filterParams.startDate) queryParams.append("StartDate", filterParams.startDate.toISOString());
    if (filterParams.endDate) queryParams.append("EndDate", filterParams.endDate.toISOString());

    const response = await fetch(`${API_URL}/NaturalGas/filter?${queryParams}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to filter natural gas data");
    }

    return await response.json();
  } catch (error) {
    console.error("Error filtering natural gas data:", error);
    throw error;
  }
};

export const createNaturalGasRecord = async (naturalGasData) => {
  try {
    const response = await fetch(`${API_URL}/NaturalGas`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      },
      body: JSON.stringify(naturalGasData)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to create natural gas record");
    }

    return await response.json();
  } catch (error) {
    console.error("Error creating natural gas record:", error);
    throw error;
  }
};

export const updateNaturalGasRecord = async (id, naturalGasData) => {
  try {
    const response = await fetch(`${API_URL}/NaturalGas/${id}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      },
      body: JSON.stringify(naturalGasData)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to update natural gas record");
    }

    return await response.json();
  } catch (error) {
    console.error("Error updating natural gas record:", error);
    throw error;
  }
};

export const deleteNaturalGasRecord = async (id) => {
  try {
    const response = await fetch(`${API_URL}/NaturalGas/${id}`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to delete natural gas record");
    }

    return true;
  } catch (error) {
    console.error("Error deleting natural gas record:", error);
    throw error;
  }
};

export const naturalGasdownloadSampleExcel = async () => {
  try {
    const response = await fetch(`${API_URL}/NaturalGas/downloadSampleExcel`, {
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
    a.download = `NaturalGas_Template.xlsx`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  } catch (err) {
    console.error(`Failed to download natural gas template: ${err.message}`);
    throw err; // Rethrow the error for further handling if needed
  }
};

export const naturalGasMultipleUpload = async (file) => {
  try {
    const formData = new FormData();
    formData.append("file", file);

    const response = await fetch(`${API_URL}/NaturalGas/multiple`, {
      method: "POST",
      headers: {
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      },
      body: formData
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to upload multiple natural gas records");
    }

    return await response.json();
  } catch (error) {
    console.error("Error uploading multiple natural gas records:", error);
    throw error;
  }
}