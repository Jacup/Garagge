/**
 * Application Configuration
 * Central place for all app-wide configuration settings
 */

import { apiConfig, getApiBaseUrl } from './api'

export const config = {
  api: {
    baseUrl: getApiBaseUrl(),
    isDevelopment: apiConfig.isDevelopment,
    isContainer: apiConfig.isContainer,
  },

  app: {
    name: 'Garagge',
    version: '1.0.0',
  },

  development: {
    enableDebugLogs: apiConfig.isDevelopment,
    showApiUrls: apiConfig.isDevelopment,
  },
} as const

// Development helper - log API URL and mode in console
if (config.development.showApiUrls) {
  const mode = config.api.isContainer ? 'Container' : 'IDE'
  const env = config.api.isDevelopment ? 'Development' : 'Production'
  console.log(`🔗 API Base URL: ${config.api.baseUrl} (${mode} + ${env})`)
}
