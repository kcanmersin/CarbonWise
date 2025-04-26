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

export const filterWaters = async (startDate, endDate) => {
  try {
    let url = `${API_URL}/Waters/filter?`;
    if (startDate) {
      url += `startDate=${startDate.toISOString()}`;
    }
    if (endDate) {
      url += `${startDate ? '&' : ''}endDate=${endDate.toISOString()}`;
    }

    const response = await fetch(url, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to filter water records");
    }

    return await response.json();
  } catch (error) {
    console.error("Error filtering water records:", error);
    throw error;
  }
};