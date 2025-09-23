import axios, { type AxiosRequestConfig, type AxiosResponse } from 'axios'
import { useUserStore } from '@/stores/userStore'

const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || (import.meta.env.DEV ? `http://${window.location.hostname}:5000` : ''),
  headers: {
    'Content-Type': 'application/json',
  },
})

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
