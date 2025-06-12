const API_URL = "http://localhost:5067/api";

export const getUser = async (userId) => {
    const response = await fetch(`${API_URL}/Users/${userId}`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
        },
    });
    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error || "Failed to fetch user data");
    }
    return response.json();
};

export const getCurrentUserInfo = async () => {
    const token = localStorage.getItem('access_token'); // This is your JWT
    if (!token) {
        throw new Error("No authentication token found. Please log in again.");
    }

    const response = await fetch(`${API_URL}/Users/me`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`, // Send the JWT as Bearer token
        },
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error || "Failed to fetch current user info");
    }

    return response.json();
}

export const getCurrentUserProfile = async () => {
    const token = localStorage.getItem('access_token'); // This is your JWT
    if (!token) {
        throw new Error("No authentication token found. Please log in again.");
    }

    const response = await fetch(`${API_URL}/Users/me/profile`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`, // Send the JWT as Bearer token
        },
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error || "Failed to fetch current user profile");
    }

    return response.json();
}

export const getCurrentUserClaims = async () => {
    const token = localStorage.getItem('access_token'); // This is your JWT
    if (!token) {
        throw new Error("No authentication token found. Please log in again.");
    }

    const response = await fetch(`${API_URL}/Users/me/claims`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`, // Send the JWT as Bearer token
        },
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error || "Failed to fetch current user claims");
    }

    return response.json();
}