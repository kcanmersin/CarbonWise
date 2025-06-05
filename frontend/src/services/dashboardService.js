const API_URL = "http://localhost:5067/api"; // Adjust to match your backend base URL

// Function to fetch air quality data by city name
export const getCityAirQuality = async (cityName) => {
  try {
    const response = await fetch(`${API_URL}/externalapis/airquality/city/${cityName}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}` // Include if your API requires auth
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || `Failed to fetch air quality data for ${cityName}`);
    }

    return await response.json();
  } catch (error) {
    console.error(`Error fetching air quality data for ${cityName}:`, error);
    throw error;
  }
};

// Function to fetch air quality data by geo-location (lat, lng)
export const getGeoAirQuality = async (lat, lng) => {
  try {
    const response = await fetch(`${API_URL}/externalapis/airquality/geo?lat=${lat}&lng=${lng}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}` // Include if your API requires auth
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch air quality data by geo-location");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching geo air quality data:", error);
    throw error;
  }
};

export const getMonthlyAirQuality = async (city) => {
  try {
    const response = await fetch(`${API_URL}/ExternalApis/airquality/database/last30days?city=${city}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}` // Include if your API requires auth
      }
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch monthly air quality data");
    }
    return await response.json();
  } catch (error) {
    console.error("Error fetching monthly air quality data:", error);
    throw error;
  }
};
