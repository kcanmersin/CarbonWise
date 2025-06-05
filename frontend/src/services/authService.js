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

// OAuth Functions - Using the correct endpoints from your Swagger
export const getOAuthLoginUrl = async () => {
  try {
    const response = await fetch(`${API_URL}/oauth/login-url`, {
      method: "GET",
      headers: { "Content-Type": "application/json" },
    });

    if (!response.ok) {
      const err = await response.json();
      throw new Error(err.error || "Failed to get OAuth login URL");
    }

    const data = await response.json();
    return data.loginUrl;
  } catch (error) {
    console.error("Error getting OAuth login URL:", error);
    throw error;
  }
};

export const handleOAuthCallback = async (code, state) => {
  try {
    const response = await fetch(`${API_URL}/oauth/callback`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ code, state }),
    });

    // print the response for debugging
    console.log("Handling OAuth callback with code:", code, "and state:", state);
    if (!response.ok) {
      const err = await response.json();
      throw new Error(err.error || "OAuth callback failed");
    }

    const result = await response.json();
    console.log("OAuth callback response:", result);
    
    // Return the result as-is since your backend should return the correct format
    return result;
  } catch (error) {
    console.error("Error handling OAuth callback:", error);
    throw error;
  }
};

// Simple OAuth redirect (no popup, direct redirect to school login)
export const redirectToOAuthLogin = async () => {
  try {
    const loginUrl = await getOAuthLoginUrl();
    // Direct redirect to OAuth provider (school login page)
    window.location.href = loginUrl;
  } catch (error) {
    console.error("Error redirecting to OAuth login:", error);
    throw error;
  }
};

// Register function (if needed)
export const registerUser = async ({ username, email, password }) => {
  const response = await fetch(`${API_URL}/register`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ username, email, password }),
  });

  if (!response.ok) {
    const err = await response.json();
    throw new Error(err.error || "Registration failed");
  }

  return await response.json();
};

// Auth status check (session-based, no localStorage)
export const checkAuthStatus = () => {
  // Since we're not storing tokens, we'll need to check with the server
  // This would typically be handled by server-side session management
  return false; // Or implement a server check
};

// Logout function (no localStorage to clear)
export const logoutUser = () => {
  // Clear any session data if needed
  // This would typically be a server-side session invalidation
  console.log("Logging out user");
};