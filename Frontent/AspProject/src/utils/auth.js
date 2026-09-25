/**
 * Authentication utility functions
 */

/**
 * Check if the user is authenticated
 * @returns {boolean} - True if authenticated, false otherwise
 */
export const isAuthenticated = () => {
  const token = localStorage.getItem('AuthToken');
  return !!token; // Return true if token exists, false otherwise
};

/**
 * Get the current user's token
 * @returns {string|null} - The user's token or null if not authenticated
 */
export const getToken = () => {
  return localStorage.getItem('AuthToken');
}; 