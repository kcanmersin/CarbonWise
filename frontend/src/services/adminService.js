const API_URL = "http://localhost:5067/api";

export const promoteUser = async (userId) => {
    const token = localStorage.getItem('access_token'); // This is your JWT
    if (!token) {
        throw new Error("No authentication token found. Please log in again.");
    }

    const response = await fetch(`${API_URL}/Admin/promote-user`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`, // Send the JWT as Bearer token
        },
        body: JSON.stringify({ userId }),
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error || "Failed to promote user");
    }

    return response.json();
}

export const demoteUser = async (userId) => {
    const token = localStorage.getItem('access_token'); // This is your JWT
    if (!token) {
        throw new Error("No authentication token found. Please log in again.");
    }

    const response = await fetch(`${API_URL}/Admin/demote-user`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`, // Send the JWT as Bearer token
        },
        body: JSON.stringify({ userId }),
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error || "Failed to demote user");
    }

    return response.json();
}

export const changeRole = async (userId, newRole) => {
    const token = localStorage.getItem('access_token'); // This is your JWT
    if (!token) {
        throw new Error("No authentication token found. Please log in again.");
    }

    const response = await fetch(`${API_URL}/Admin/change-role`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`, // Send the JWT as Bearer token
        },
        body: JSON.stringify({ userId, newRole }),
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error || "Failed to change user role");
    }

    return response.json();
}

export const getAllUsers = async () => {
    const token = localStorage.getItem('access_token'); // This is your JWT
    if (!token) {
        throw new Error("No authentication token found. Please log in again.");
    }

    const response = await fetch(`${API_URL}/Admin/users`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`, // Send the JWT as Bearer token
        },
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error || "Failed to fetch users");
    }

    return response.json();
}

export const getUsersByRole = async (role) => {
  const token = localStorage.getItem('access_token');
  if (!token) {
    throw new Error("No authentication token found. Please log in again.");
  }

  const response = await fetch(`${API_URL}/Admin/users/role/${role}`, {
    method: "GET", // Use GET instead of POST
    headers: {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${token}`,
    },
  });

  if (!response.ok) {
    const error = await response.json();
    throw new Error(error.error || "Failed to fetch users by role");
  }

  return response.json();
};

export const getUserById = async (userId) => {
    const token = localStorage.getItem('access_token'); // This is your JWT
    if (!token) {
        throw new Error("No authentication token found. Please log in again.");
    }

    const response = await fetch(`${API_URL}/Admin/users/${userId}`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`, // Send the JWT as Bearer token
        },
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error || "Failed to fetch user by ID");
    }

    return response.json();
}

export const adminDashboardStats = async () => {
    const token = localStorage.getItem('access_token'); // This is your JWT
    if (!token) {
        throw new Error("No authentication token found. Please log in again.");
    }

    const response = await fetch(`${API_URL}/Admin/dashboard/stats`, {
        method: "GET",
        headers: {
            "Content-Type": "application/json",
            "Authorization": `Bearer ${token}`, // Send the JWT as Bearer token
        },
    });

    if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error || "Failed to fetch admin dashboard stats");
    }

    // print the response for debugging
    console.log("Admin Dashboard Stats Response:", response);

    return response.json();
}