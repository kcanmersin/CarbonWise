const API_URL = "http://localhost:5067/api"; // Adjust this URL to match your actual API base URL

export const calculateCarbonFootprint = async (year, factors) => {
  try {
    // Construct the query parameters
    const queryParams = new URLSearchParams({
      Year: year,
      ElectricityFactor: factors.electricityFactor,
      ShuttleBusFactor: factors.shuttleBusFactor,
      CarFactor: factors.carFactor,
      MotorcycleFactor: factors.motorcycleFactor
    });

    const response = await fetch(`${API_URL}/CarbonFootprints/year/${year}?${queryParams}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to calculate carbon footprint");
    }

    return await response.json();
  } catch (error) {
    console.error("Error calculating carbon footprint:", error);
    throw error;
  }
};

export const getHistoricalFootprint = async (factors) => {
  try {
    // Format dates in ISO string format
    const currentYear = new Date().getFullYear();
    const startYear = currentYear - 4;

    const startDate = `${startYear}-01-01T00:00:00Z`;
    const endDate = `${currentYear}-12-31T23:59:59Z`;
    
    // Build query string with required parameters
    const queryParams = new URLSearchParams();
    queryParams.append("StartDate", startDate);
    queryParams.append("EndDate", endDate);
    
    // Add emission factors if provided
    if (factors) {
      if (factors.electricityFactor) queryParams.append("ElectricityFactor", factors.electricityFactor);
      if (factors.shuttleBusFactor) queryParams.append("ShuttleBusFactor", factors.shuttleBusFactor);
      if (factors.carFactor) queryParams.append("CarFactor", factors.carFactor);
      if (factors.motorcycleFactor) queryParams.append("MotorcycleFactor", factors.motorcycleFactor);
    }
    
    const response = await fetch(`${API_URL}/CarbonFootprints/period?${queryParams}`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
        "Authorization": `Bearer ${localStorage.getItem("token")}`
      }
    });

    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.error || "Failed to fetch historical carbon footprint data");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching historical carbon footprint data:", error);
    throw error;
  }
};