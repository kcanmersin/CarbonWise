const API_URL = "http://localhost:5067/api"; // Adjust this URL to match your actual API base URL

export const getAllWaters = async () => {
  try {
    const response = await fetch(`${API_URL}/Waters`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch water records");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching water records:", error);
    throw error;
  }
};

export const getWaterById = async (id) => {
  try {
    const response = await fetch(`${API_URL}/Waters/${id}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch water record");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching water record:", error);
    throw error;
  }
};

export const createWater = async (waterData) => {
  try {
    const response = await fetch(`${API_URL}/Waters`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      },
      body: JSON.stringify(waterData)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to create water record");
    }

    return await response.json();
  } catch (error) {
    console.error("Error creating water record:", error);
    throw error;
  }
};

export const updateWater = async (id, waterData) => {
  try {
    const response = await fetch(`${API_URL}/Waters/${id}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      },
      body: JSON.stringify(waterData)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to update water record");
    }

    return await response.json();
  } catch (error) {
    console.error("Error updating water record:", error);
    throw error;
  }
};

export const deleteWater = async (id) => {
  try {
    const response = await fetch(`${API_URL}/Waters/${id}`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to delete water record");
    }

    return true;
  } catch (error) {
    console.error("Error deleting water record:", error);
    throw error;
  }
};

export const filterWaters = async (filterParams) => {
  try {
    // Create query string from filter parameters
    const queryParams = new URLSearchParams();
    if (filterParams.startDate) queryParams.append("StartDate", filterParams.startDate.toISOString());
    if (filterParams.endDate) queryParams.append("EndDate", filterParams.endDate.toISOString());

    const response = await fetch(`${API_URL}/Waters/filter?${queryParams}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to filter water data");
    }

    return await response.json();
  } catch (error) {
    console.error("Error filtering water data:", error);
    throw error;
  }
};

export const waterdownloadSampleExcel = async () => {
  try {
    const response = await fetch(`${API_URL}/Waters/downloadSampleExcel`, {
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
    a.download = `Waters_Template.xlsx`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  } catch (err) {
    console.error("Error downloading sample Excel template:", err);
    throw err;
  }
};

export const watersMultipleUpload = async (file) => {
  try {
    const formData = new FormData();
    formData.append("file", file);

    const response = await fetch(`${API_URL}/Waters/multiple`, {
      method: "POST",
      headers: {
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      },
      body: formData
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to upload multiple water records");
    }

    return await response.json();
  } catch (error) {
    console.error("Error uploading multiple water records:", error);
    throw error;
  }
}