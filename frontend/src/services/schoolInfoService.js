const API_URL = 'http://localhost:5067/api/SchoolInfo';

// Helper function to handle fetch responses
const handleResponse = async (response) => {
  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(errorText || `HTTP error! Status: ${response.status}`);
  }
  return response.json();
};

export const getSchoolInfoById = async (id) => {
  try {
    const response = await fetch(`${API_URL}/${id}`);
    return handleResponse(response);
  } catch (error) {
    console.error('Error fetching school info by ID:', error);
    throw error;
  }
};

export const getSchoolInfoByYear = async (year) => {
  try {
    const response = await fetch(`${API_URL}/year/${year}`);
    return handleResponse(response);
  } catch (error) {
    console.error('Error fetching school info by year:', error);
    throw error;
  }
};

export const createSchoolInfo = async (schoolInfoData) => {
  try {
    const response = await fetch(API_URL, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(schoolInfoData)
    });
    return handleResponse(response);
  } catch (error) {
    console.error('Error creating school info:', error);
    throw error;
  }
};

export const updateSchoolInfo = async (id, schoolInfoData) => {
  try {
    const response = await fetch(`${API_URL}/${id}`, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(schoolInfoData)
    });
    return handleResponse(response);
  } catch (error) {
    console.error('Error updating school info:', error);
    throw error;
  }
};

export const getAllSchoolInfo = async () => {
  try {
    // Assuming an endpoint exists to get all school info
    // If not available in the controller, you might need to request it added
    const response = await fetch(API_URL);
    return handleResponse(response);
  } catch (error) {
    console.error('Error fetching all school info:', error);
    throw error;
  }
};