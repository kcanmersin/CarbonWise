const API_URL = "http://localhost:5067/api/Auth";

export const loginUser = async ({ username, password }) => {
  const response = await fetch(`${API_URL}/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, password }),
  });

  if (!response.ok) {
    const err = await response.json();
    throw new Error(err.error || "Login failed");
  }

  return await response.json();
};

// Add more auth-related functions here as needed
export const checkAuthStatus = () => {
  return localStorage.getItem("token") ? true : false;
};

export const logoutUser = () => {
  localStorage.removeItem("token");
};