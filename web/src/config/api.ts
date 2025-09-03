/**
 * API Configuration
 * Centralized configuration for API endpoints and connection settings
 */

/**
 * Determines if the app is running in development mode
 */
function isDevelopmentMode(): boolean {
  // Check build-time dev mode (IDE)
  if (import.meta.env.DEV) {
    return true
  }
  
  // Check custom mode variable (container with VITE_MODE=development)
  if (import.meta.env.VITE_MODE === 'development') {
    return true
  }
  
  return false
}

/**
 * Determines if the app is running in a container
 */
function isContainerMode(): boolean {
  // If it's not build-time dev mode, assume container
  return !import.meta.env.DEV
}

/**
 * Determines the appropriate API base URL based on the environment
 * 
 * @returns The API base URL to use for HTTP requests
 */
export function getApiBaseUrl(): string {
  // Check if we have an explicit environment variable set
  if (import.meta.env.VITE_API_URL) {
    return import.meta.env.VITE_API_URL
  }

  // IDE mode - connect to localhost
  if (import.meta.env.DEV) {
    return 'http://localhost:5000'
  }

  // Container mode - always connect to server container
  return 'http://server:8080'
}

/**
 * API configuration object
 */
export const apiConfig = {
  baseUrl: getApiBaseUrl(),
  timeout: 10000, // 10 seconds
  headers: {
    'Content-Type': 'application/json',
  },
  isDevelopment: isDevelopmentMode(),
  isContainer: isContainerMode(),
} as const
