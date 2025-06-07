const API_URL = "http://localhost:5067/api"; // Adjust to match your backend base URL

export const getConsumptionTypes = async () => {
  try {
    const response = await fetch(`${API_URL}/Reports/consumption-types`, {
      method: "GET",
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) {
      throw new Error("Failed to fetch consumption types");
    }

    return await response.json();
  } catch (error) {
    console.error("Error fetching consumption types:", error);
    throw error;
  }
};


export const generateCarbonFootprintPdfReport = async (startDate, endDate) => {
  try {
    // Format the query parameters
    const params = new URLSearchParams({
      startDate,
      endDate
    });

    // Make a GET request to the PDF Reports endpoint
    const response = await fetch(`${API_URL}/PdfReports/carbon-footprint?${params}`, {
      method: "GET",
      headers: {
        Accept: "application/pdf"
      }
    });

    if (!response.ok) {
      if (response.headers.get("content-type")?.includes("application/json")) {
        const errorData = await response.json();
        throw new Error(errorData.error || "Failed to generate carbon footprint PDF report");
      } else {
        throw new Error(`HTTP error: ${response.status}`);
      }
    }

    // Return the response as a blob
    return await response.blob();
  } catch (error) {
    console.error("Error generating carbon footprint PDF report:", error);
    throw error;
  }
};

export const generateConsumptionPdfReport = async (consumptionType, buildingId, startDate, endDate) => {
  try {
    // Format the query parameters
    const params = new URLSearchParams({
      consumptionType,
      startDate,
      endDate
    });

    // Add buildingId parameter only if it's provided
    if (buildingId) {
      params.append("buildingId", buildingId);
    }

    // Make a GET request to the PDF Reports endpoint
    const response = await fetch(`${API_URL}/PdfReports/consumption?${params}`, {
      method: "GET",
      headers: {
        Accept: "application/pdf"
      }
    });

    if (!response.ok) {
      if (response.headers.get("content-type")?.includes("application/json")) {
        const errorData = await response.json();
        throw new Error(errorData.error || "Failed to generate consumption PDF report");
      } else {
        throw new Error(`HTTP error: ${response.status}`);
      }
    }

    // Return the response as a blob
    return await response.blob();
  } catch (error) {
    console.error("Error generating consumption PDF report:", error);
    throw error;
  }
};