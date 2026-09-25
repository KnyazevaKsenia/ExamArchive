import Vue from 'vue'
import App from './App.vue'
import router from './router'
import axios from 'axios'
import { jwtDecode } from 'jwt-decode'
import { API_ENDPOINTS } from './config'
import notifications from './plugins/notifications'

import './assets/main.css'

// Global axios configuration
axios.defaults.withCredentials = true

// Configure axios to use the proper API base URL
axios.defaults.baseURL = 'https://localhost:44356'

// Set default Authorization header with bearer token
const token = localStorage.getItem('AuthToken')
if (token) {
  axios.defaults.headers.common['Authorization'] = `Bearer ${token}`
}

// Token refresh variables
let isRefreshing = false
let refreshSubscribers = []

// Function to subscribe callbacks waiting for new token
const subscribeTokenRefresh = (callback) => {
  refreshSubscribers.push(callback)
}

// Function to notify all subscribers that token is refreshed
const onTokenRefreshed = (token) => {
  refreshSubscribers.forEach(callback => callback(token))
  refreshSubscribers = []
}

// Function to refresh token
const refreshAuthToken = async () => {
  if (isRefreshing) {
    return new Promise((resolve) => {
      subscribeTokenRefresh(token => {
        resolve(token)
      })
    })
  }

  isRefreshing = true

  try {
    const response = await axios.post(API_ENDPOINTS.REFRESH, {}, {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('AuthToken')}`
      },
      withCredentials: true
    })
    
    if (response.data && (response.data.token || response.data.Token)) {
      const newToken = response.data.token || response.data.Token
      localStorage.setItem('AuthToken', newToken)
      console.log('Token refreshed successfully')
      onTokenRefreshed(newToken)
      setupTokenRefreshTimeout(newToken)
      isRefreshing = false
      return newToken
    }
  } catch (error) {
    console.error('Failed to refresh token:', error)
    isRefreshing = false
    throw error
  }
}

// Function to setup token refresh timeout
const setupTokenRefreshTimeout = (token) => {
  if (!token) return
  
  try {
    const decodedToken = jwtDecode(token)
    const expirationTime = decodedToken.exp * 1000 // Convert to milliseconds
    const currentTime = Date.now()
    
    // Calculate time until token expiration (minus 1 minute buffer)
    const timeUntilExpiration = expirationTime - currentTime - 60000
    
    if (timeUntilExpiration <= 0) {
      // Token already expired, refresh immediately
      refreshAuthToken()
    } else {
      // Set timeout to refresh token before it expires
      setTimeout(() => {
        refreshAuthToken()
      }, timeUntilExpiration)
    }
  } catch (error) {
    console.error('Error setting up token refresh:', error)
  }
}

// Configure axios to include credentials and proper headers
axios.interceptors.request.use(config => {
  // Add authentication header if token exists
  const token = localStorage.getItem('AuthToken')
  if (token) {
    config.headers['Authorization'] = `Bearer ${token}`
  }
  return config
})

// Response interceptor for auto token refresh
axios.interceptors.response.use(
  response => response,
  async error => {
    const originalRequest = error.config
    
    // If the error is due to an unauthorized request (401) and we haven't tried refreshing yet
    if (error.response && error.response.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true
      
      try {
        // Refresh the token
        const newToken = await refreshAuthToken()
        
        // Update the authorization header
        originalRequest.headers['Authorization'] = `Bearer ${newToken}`
        
        // Retry the original request
        return axios(originalRequest)
      } catch (refreshError) {
        // If refreshing fails, redirect to login or handle as needed
        console.error('Failed to refresh token, redirecting to login', refreshError)
        return Promise.reject(refreshError)
      }
    }
    
    return Promise.reject(error)
  }
)

// Initialize token refresh on app start if a token exists
const initializeTokenRefresh = () => {
  const token = localStorage.getItem('AuthToken')
  if (token) {
    setupTokenRefreshTimeout(token)
  }
}

// Call initialization
initializeTokenRefresh()

// Use plugins
Vue.use(notifications)

new Vue({
  router,
  render: h => h(App)
}).$mount('#app')
