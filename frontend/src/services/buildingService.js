const API_URL = "http://localhost:5067/api"; // Adjust this URL to match your actual API base URL

export const getBuildings = async () => {
  try {
    const response = await fetch(`${API_URL}/Buildings`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("access_token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch buildings");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching buildings:", error);
    throw error;
  }
};

export const getBuildingById = async (id) => {
  try {
    const response = await fetch(`${API_URL}/Buildings/${id}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("access_token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch building");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching building:", error);
    throw error;
  }
};

export const createBuilding = async (buildingData) => {
  try {
    const response = await fetch(`${API_URL}/Buildings`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("access_token")}`
      },
      body: JSON.stringify(buildingData)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to create building");
    }

    return await response.json();
  } catch (error) {
    console.error("Error creating building:", error);
    throw error;
  }
};

export const updateBuilding = async (id, buildingData) => {
  try {
    const response = await fetch(`${API_URL}/Buildings/${id}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("access_token")}`
      },
      body: JSON.stringify(buildingData)
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to update building");
    }

    return await response.json();
  } catch (error) {
    console.error("Error updating building:", error);
    throw error;
  }
};

export const deleteBuilding = async (id) => {
  try {
    const response = await fetch(`${API_URL}/Buildings/${id}`, {
      method: "DELETE",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("access_token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to delete building");
    }

    return true;
  } catch (error) {
    console.error("Error deleting building:", error);
    throw error;
  }
};