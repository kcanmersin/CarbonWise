const API_URL = "http://localhost:5067/api"; // Same base URL

export const getTestQuestions = async () => {
  const response = await fetch(`${API_URL}/CarbonFootprintTest/questions`, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
    },
  });

  if (!response.ok) {
    throw new Error("Failed to load questions");
  }

  return response.json();
};

export const startCarbonFootprintTest = async () => {
  const token = localStorage.getItem('access_token'); // This is your JWT
  
  if (!token) {
    throw new Error("No authentication token found. Please log in again.");
  }

  const response = await fetch(`${API_URL}/CarbonFootprintTest/start`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${token}`, // Send the JWT as Bearer token
    },
  });

  console.log("Start test response:", response);

  if (!response.ok) {
    throw new Error("Failed to start test");
  }

  return response.json();
};

export const saveTestResponse = async (testId, questionId, optionId) => {
  const token = localStorage.getItem('access_token');
  if (!token) {
    throw new Error("No authentication token found. Please log in again.");
  }

  const response = await fetch(`${API_URL}/CarbonFootprintTest/${testId}/response`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${token}`,
    },
    body: JSON.stringify({ questionId, optionId }),
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || "Failed to save response");
  }

  return response.json();
};

export const completeTest = async (testId) => {
  const token = localStorage.getItem('access_token');
  if (!token) {
    throw new Error("No authentication token found. Please log in again.");
  }

  const response = await fetch(`${API_URL}/CarbonFootprintTest/${testId}/complete`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || "Failed to complete test");
  }

  return response.json();
};


export const getStats = async () => {
  const response = await fetch(`${API_URL}/CarbonFootprintTest/stats`, {
    method: "GET",
    headers: {
      "Content-Type": "application/json",
    },
  });

  if (!response.ok) {
    throw new Error("Failed to load stats");
  }

  return response.json();
};