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

export const filterPapers = async (startDate, endDate) => {
  try {
    let url = `${API_URL}/Papers/filter?`;
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
      throw new Error(error.error || "Failed to filter paper records");
    }

    return await response.json();
  } catch (error) {
    console.error("Error filtering paper records:", error);
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