const API_URL = "http://localhost:5067/api"; // Adjust to match your backend base URL

export const exportConsumptionData = async (consumptionType, startDate, endDate, includeGraphs = true) => {
  try {
    // Format the query parameters
    const params = new URLSearchParams({
      consumptionType,
      startDate,
      endDate,
      includeGraphs
    });

    // Make a GET request to the export endpoint
    const response = await fetch(`${API_URL}/ConsumptionData/export?${params}`, {
      method: "GET",
      headers: {
        Accept: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
      }
    });

    if (!response.ok) {
      if (response.headers.get("content-type")?.includes("application/json")) {
        const errorData = await response.json();
        throw new Error(errorData.error || "Failed to export consumption data");
      } else {
        throw new Error(`HTTP error: ${response.status}`);
      }
    }

    // Return the response as a blob
    return await response.blob();
  } catch (error) {
    console.error("Error exporting consumption data:", error);
    throw error;
  }
};