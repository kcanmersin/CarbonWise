const API_URL = "http://localhost:5067/api"; // Adjust this URL to match your actual API base URL

export const getAllPapers = async () => {
  try {
    const response = await fetch(`${API_URL}/Papers`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch paper records");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching paper records:", error);
    throw error;
  }
};

export const getPaperById = async (id) => {
  try {
    const response = await fetch(`${API_URL}/Papers/${id}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch paper record");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching paper record:", error);
    throw error;
  }
};

export const getPaperByBuilding = async (buildingId) => {
  try {
    const response = await fetch(`${API_URL}/Papers/building/${buildingId}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch paper data");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching paper data:", error);
    throw error;
  }
};

export const getPaperMonthlyAggregate = async (startDate, endDate) => {
  try {
    // Create query string from parameters
    const queryParams = new URLSearchParams();
    if (startDate) queryParams.append("startDate", startDate.toISOString());
    if (endDate) queryParams.append("endDate", endDate.toISOString());

    const response = await fetch(`${API_URL}/Papers/monthly-aggregate?${queryParams}`, {
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

export const getPaperMonthlyTotals = async (startDate, endDate) => {
  try {
    // Create query string from parameters
    const queryParams = new URLSearchParams();
    if (startDate) queryParams.append("startDate", startDate.toISOString());
    if (endDate) queryParams.append("endDate", endDate.toISOString());

    const response = await fetch(`${API_URL}/Papers/monthly-totals?${queryParams}`, {
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

export const filterPapers = async (filterParams) => {
  try {
    // Create query string from filter parameters
    const queryParams = new URLSearchParams();
    if (filterParams.buildingId) queryParams.append("BuildingId", filterParams.buildingId);
    if (filterParams.startDate) queryParams.append("StartDate", filterParams.startDate.toISOString());
    if (filterParams.endDate) queryParams.append("EndDate", filterParams.endDate.toISOString());

    const response = await fetch(`${API_URL}/Papers/filter?${queryParams}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to filter paper data");
    }

    return await response.json();
  } catch (error) {
    console.error("Error filtering paper data:", error);
    throw error;
  }
};

export const createPaper = async (paperData) => {
  try {
    const response = await fetch(`${API_URL}/Papers`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      },
      body: JSON.stringify(paperData)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to create paper record");
    }

    return await response.json();
  } catch (error) {
    console.error("Error creating paper record:", error);
    throw error;
  }
};

export const updatePaper = async (id, paperData) => {
  try {
    const response = await fetch(`${API_URL}/Papers/${id}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      },
      body: JSON.stringify(paperData)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to update paper record");
    }

    return await response.json();
  } catch (error) {
    console.error("Error updating paper record:", error);
    throw error;
  }
};

export const deletePaper = async (id) => {
  try {
    const response = await fetch(`${API_URL}/Papers/${id}`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to delete paper record");
    }

    return true;
  } catch (error) {
    console.error("Error deleting paper record:", error);
    throw error;
  }
};

export const getMonthlyPaperUsage = async (year) => {
  try {
    const startDate = new Date(year, 0, 1); // January 1st of selected year
    const endDate = new Date(year, 11, 31); // December 31st of selected year
    
    const response = await fetch(`${API_URL}/Papers/monthly-usage?startDate=${startDate.toISOString()}&endDate=${endDate.toISOString()}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch monthly paper usage");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching monthly paper usage:", error);
    throw error;
  }
};

export const papersdownloadSampleExcel = async () => {
  try {
    const response = await fetch(`${API_URL}/Papers/downloadSampleExcel`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('access_token')}`, // if using authentication
      },
    });
    
    if (!response.ok) throw new Error('Failed to download template');
    
    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Papers_Template.xlsx`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  } catch (err) {
    console.error("Error downloading sample Excel template:", err);
    throw err;
  }
};

export const papersMultipleUpload = async (file) => {
  try {
    const formData = new FormData();
    formData.append("file", file);

    const response = await fetch(`${API_URL}/Papers/multiple`, {
      method: "POST",
      headers: {
        "Authorization": `Bearer ${localStorage.getItem("access_token")}`
      },
      body: formData
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to upload multiple paper records");
    }

    return await response.json();
  } catch (error) {
    console.error("Error uploading multiple paper records:", error);
    throw error;
  }
};