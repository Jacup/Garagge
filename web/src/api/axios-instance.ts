import axios, { type AxiosRequestConfig, type AxiosResponse } from 'axios'
import { useUserStore } from '@/stores/userStore'

const getApiBaseUrl = () => {
  return import.meta.env.VITE_API_URL || 'http://localhost:5000'
}

const axiosClient = axios.create({
  baseURL: getApiBaseUrl(),
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
})

// Development helper
if (import.meta.env.DEV) {
  console.log(`🔗 API Base URL: ${getApiBaseUrl()}`)
}

axiosClient.interceptors.request.use(
  (config) => {
    const userStore = useUserStore()
    if (userStore.accessToken) {
      config.headers = config.headers ?? {}
      config.headers.Authorization = `Bearer ${userStore.accessToken}`
    }
    return config
  },
  (error) => Promise.reject(error),
)

axiosClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      const userStore = useUserStore()
      userStore.clearToken()
    }
    return Promise.reject(error)
  },
)

export const axiosInstance = <T = unknown>(config: AxiosRequestConfig): Promise<AxiosResponse<T>> => {
  return axiosClient.request<T>(config)
}

export { axiosClient }
