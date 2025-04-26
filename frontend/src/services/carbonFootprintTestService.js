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
  const response = await fetch(`${API_URL}/CarbonFootprintTest/start`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
  });

  if (!response.ok) {
    throw new Error("Failed to start test");
  }

  return response.json(); // Should contain testId or test object
};

export const saveTestResponse = async (testId, questionId, optionId) => {
  const response = await fetch(`${API_URL}/CarbonFootprintTest/${testId}/response`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
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
  const response = await fetch(`${API_URL}/CarbonFootprintTest/${testId}/complete`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || "Failed to complete test");
  }

  return response.json();
};
