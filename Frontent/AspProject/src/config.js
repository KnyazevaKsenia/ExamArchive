// API endpoints configuration
export const API_BASE_URL = 'https://localhost:44356';

export const API_ENDPOINTS = {
  // Authentication endpoints
  REGISTER: `${API_BASE_URL}/auth/register`,
  LOGIN: `${API_BASE_URL}/auth/login`,
  REFRESH: `${API_BASE_URL}/auth/refresh`,
  LOGOUT: `${API_BASE_URL}/auth/logout`,
  
  // User endpoints
  USER_PROFILE: `${API_BASE_URL}/user/profile`,
  
  // Materials endpoints
  GET_MATERIALS: `${API_BASE_URL}/materials/get-materials`,
  GET_MATERIAL_DETAILS: `${API_BASE_URL}/materials/get-material`,
  ADD_MATERIAL: `${API_BASE_URL}/adding/add-material`,
  SEARCH_BY_KEYWORD: `${API_BASE_URL}/materials/find-by-keyword`,
  GET_FILE: `${API_BASE_URL}/materials/file`,
  
  // Profile endpoints
  PROFILE_MATERIALS: `${API_BASE_URL}/profile/materials`,
  DELETE_MATERIAL: `${API_BASE_URL}/profile/delete`,
  CHANGE_MATERIAL: `${API_BASE_URL}/profile/change`,
  
  // Selection lists
  SELECTION_LIST: `${API_BASE_URL}/adding/selection-list`,
  
  // Exams endpoints
  GET_EXAMS: `${API_BASE_URL}/exams`,
  START_EXAM: `${API_BASE_URL}/exams/start`,
  
  // Favorites endpoints
  GET_FAVORITES: `${API_BASE_URL}/favorites`,
  ADD_FAVORITE: `${API_BASE_URL}/favorites/add`,
  REMOVE_FAVORITE: `${API_BASE_URL}/favorites/remove`,
  GET_STUDENT_FAVORITES_IDS: `${API_BASE_URL}/favorites/get-student-info`
};

export default API_ENDPOINTS; 

