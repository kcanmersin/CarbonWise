const API_URL = "http://localhost:5067/api/Auth";
const OAUTH_URL = "http://localhost:5067/api/OAuth";

// Token management functions
export const saveAuthToken = (tokenData) => {
  try {
    // The 'token' field from your response IS the access token (JWT)
    if (tokenData.token) {
      localStorage.setItem('access_token', tokenData.token);
      console.log('JWT access token saved to localStorage');
    }
    return true;
  } catch (error) {
    console.error('Error saving auth token:', error);
    return false;
  }
};

export const getAuthToken = () => {
  return localStorage.getItem('access_token');
};

export const clearAuthData = () => {
  localStorage.removeItem('access_token');
  localStorage.removeItem('user_data'); // If you store user data separately
  console.log('All auth data cleared from storage');
};

// Traditional login
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

  const result = await response.json();
  
  // Save token data after successful login
  saveAuthToken(result);
  
  return result;
};

// OAuth Functions - Simplified for backend-managed PKCE
export const getOAuthLoginUrl = async () => {
  try {
    console.log('Requesting OAuth login URL from:', `${API_URL}/oauth/login-url`);
    
    const response = await fetch(`${API_URL}/oauth/login-url`, {
      method: "GET",
      headers: { "Content-Type": "application/json" },
    });

    console.log('OAuth login URL response status:', response.status);

    if (!response.ok) {
      const err = await response.json().catch(() => ({ error: "Failed to get OAuth login URL" }));
      console.error('OAuth login URL error:', err);
      throw new Error(err.error || "Failed to get OAuth login URL");
    }

    const data = await response.json();
    console.log('OAuth login URL response data:', data);
    
    const loginUrl = data.loginUrl || data.url || data;
    console.log('Final OAuth login URL:', loginUrl);
    
    return loginUrl;
  } catch (error) {
    console.error("Error getting OAuth login URL:", error);
    throw error;
  }
};

// OAuth callback using GET /auth endpoint with query parameters
export const handleOAuthCallback = async (code, state) => {
  try {
    console.log("=== OAuth Callback Debug ===");
    console.log("Code:", code?.substring(0, 10) + '... (length: ' + code?.length + ')');
    console.log("State:", state);
    
    // Build URL with query parameters
    const params = new URLSearchParams({
      code: code,
      state: state
    });
    
    const url = `${OAUTH_URL}/auth?${params.toString()}`;
    console.log("Request URL:", url);
    
    const response = await fetch(url, {
      method: "GET",
      headers: { 
        "Accept": "application/json"
      }
    });

    console.log("Response status:", response.status);
    console.log("Response headers:", Object.fromEntries(response.headers.entries()));

    // Get response body regardless of status
    const responseText = await response.text();
    console.log("Raw response body:", responseText);

    if (!response.ok) {
      let errorMessage;
      try {
        // Try to parse as JSON
        const errorData = JSON.parse(responseText);
        console.error("Parsed error response:", errorData);
        errorMessage = errorData.error || errorData.message || errorData.title || `OAuth callback failed with status: ${response.status}`;
        
        // Log additional error details if available
        if (errorData.detail) console.error("Error detail:", errorData.detail);
        if (errorData.errors) console.error("Validation errors:", errorData.errors);
        if (errorData.traceId) console.error("Trace ID:", errorData.traceId);
        
      } catch (parseError) {
        console.error("Could not parse error response as JSON:", parseError);
        errorMessage = `OAuth callback failed with status: ${response.status}. Response: ${responseText}`;
      }
      throw new Error(errorMessage);
    }

    // Parse successful response
    let result;
    try {
      result = JSON.parse(responseText);
      console.log("Parsed success response:", result);
    } catch (parseError) {
      console.error("Could not parse success response as JSON:", parseError);
      throw new Error("Invalid JSON response from OAuth callback");
    }
    
    // Validate that we received the expected user data
    if (!result || typeof result !== 'object') {
      throw new Error("Invalid response format from OAuth callback");
    }
    
    // Save token data after successful OAuth callback
    saveAuthToken(result);
    
    // Log what we're returning to help debug
    console.log("Returning user data:", {
      hasUser: !!result.user,
      hasToken: !!result.token,
      hasAccessToken: !!result.accessToken,
      keys: Object.keys(result)
    });
    
    return result;
  } catch (error) {
    console.error("=== OAuth Callback Error ===");
    console.error("Error message:", error.message);
    console.error("Error stack:", error.stack);
    throw error;
  }
};

// Alternative function for POST /oauth/callback if you still want to use it
export const handleOAuthCallbackPost = async (code, state) => {
  try {
    console.log("=== OAuth Callback (POST) Debug ===");
    console.log("Code:", code?.substring(0, 10) + '... (length: ' + code?.length + ')');
    console.log("State:", state);
    
    const requestBody = { code, state };
    console.log("Request body:", requestBody);
    console.log("Endpoint:", `${API_URL}/oauth/callback`);
    
    const response = await fetch(`${API_URL}/oauth/callback`, {
      method: "POST",
      headers: { 
        "Content-Type": "application/json",
        "Accept": "application/json"
      },
      body: JSON.stringify(requestBody),
    });

    console.log("Response status:", response.status);
    console.log("Response headers:", Object.fromEntries(response.headers.entries()));

    // Get response body regardless of status
    const responseText = await response.text();
    console.log("Raw response body:", responseText);

    if (!response.ok) {
      let errorMessage;
      try {
        // Try to parse as JSON
        const errorData = JSON.parse(responseText);
        console.error("Parsed error response:", errorData);
        errorMessage = errorData.error || errorData.message || errorData.title || `OAuth callback failed with status: ${response.status}`;
        
        // Log additional error details if available
        if (errorData.detail) console.error("Error detail:", errorData.detail);
        if (errorData.errors) console.error("Validation errors:", errorData.errors);
        if (errorData.traceId) console.error("Trace ID:", errorData.traceId);
        
      } catch (parseError) {
        console.error("Could not parse error response as JSON:", parseError);
        errorMessage = `OAuth callback failed with status: ${response.status}. Response: ${responseText}`;
      }
      throw new Error(errorMessage);
    }

    // Parse successful response
    let result;
    try {
      result = JSON.parse(responseText);
      console.log("Parsed success response:", result);
    } catch (parseError) {
      console.error("Could not parse success response as JSON:", parseError);
      throw new Error("Invalid JSON response from OAuth callback");
    }
    
    // Validate that we received the expected user data
    if (!result || typeof result !== 'object') {
      throw new Error("Invalid response format from OAuth callback");
    }
    
    // Save token data after successful OAuth callback
    saveAuthToken(result);
    
    // Log what we're returning to help debug
    console.log("Returning user data:", {
      hasUser: !!result.user,
      hasToken: !!result.token,
      hasAccessToken: !!result.accessToken,
      keys: Object.keys(result)
    });
    
    return result;
  } catch (error) {
    console.error("=== OAuth Callback (POST) Error ===");
    console.error("Error message:", error.message);
    console.error("Error stack:", error.stack);
    throw error;
  }
};

// Simple OAuth redirect (no popup, direct redirect to school login)
export const redirectToOAuthLogin = async () => {
  try {
    const loginUrl = await getOAuthLoginUrl();
    console.log("Redirecting to OAuth login URL:", loginUrl);
    
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

  const result = await response.json();
  
  // Save token data after successful registration
  saveAuthToken(result);
  
  return result;
};

// Logout function
export const logoutUser = async () => {
  // Clear local storage and session data
  clearAuthData();
  console.log('User logged out and auth data cleared');
  // Optionally, redirect to a logout confirmation page or home page
  window.location.href = '/'; // Redirect to home page after logout
};

// Utility function to parse URL parameters
export const getUrlParameters = () => {
  const params = new URLSearchParams(window.location.search);
  return {
    code: params.get('code'),
    state: params.get('state'),
    error: params.get('error'),
    error_description: params.get('error_description')
  };
};

// Utility function to validate OAuth state (if using client-side state generation)
export const validateOAuthState = (receivedState) => {
  const storedState = sessionStorage.getItem('oauth_state');
  if (!storedState || storedState !== receivedState) {
    throw new Error('Invalid OAuth state parameter. Possible CSRF attack.');
  }
  // Clear the state after validation
  sessionStorage.removeItem('oauth_state');
  return true;
};

// Handle OAuth errors
export const handleOAuthError = (error, error_description) => {
  const errorMessages = {
    'access_denied': 'Access was denied. Please try again.',
    'invalid_request': 'Invalid request. Please try again.',
    'unauthorized_client': 'Unauthorized client. Please contact support.',
    'unsupported_response_type': 'Unsupported response type. Please contact support.',
    'invalid_scope': 'Invalid scope. Please contact support.',
    'server_error': 'Server error. Please try again later.',
    'temporarily_unavailable': 'Service temporarily unavailable. Please try again later.'
  };

  const message = errorMessages[error] || error_description || 'An unknown error occurred during authentication.';
  throw new Error(message);
};