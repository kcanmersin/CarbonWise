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