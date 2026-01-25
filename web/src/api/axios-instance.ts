import axios, { type AxiosRequestConfig, type AxiosError } from 'axios'
import { useAuthStore } from '@/stores/auth'

interface FailedQueueItem {
  resolve: (token: string) => void
  reject: (error: unknown) => void
}

interface RetryAxiosRequestConfig extends AxiosRequestConfig {
  _retry?: boolean
}

let isRefreshing = false
let failedQueue: FailedQueueItem[] = []

const processQueue = (error: Error | null, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error)
    } else {
      if (token) prom.resolve(token)
    }
  })
  failedQueue = []
}

export const axiosClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL || (import.meta.env.DEV ? `http://${window.location.hostname}:5000` : ''),
  headers: { 'Content-Type': 'application/json' },
  withCredentials: true,
  paramsSerializer: { indexes: null },
})

axiosClient.interceptors.request.use(
  (config) => {
    const authStore = useAuthStore()
    if (authStore.accessToken) {
      config.headers = config.headers ?? {}
      config.headers.Authorization = `Bearer ${authStore.accessToken}`
    }
    return config
  },
  (error) => Promise.reject(error),
)

axiosClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetryAxiosRequestConfig

    if (!originalRequest) return Promise.reject(error)

    const isAuthEndpoint = originalRequest.url?.includes('/api/auth')
    if (error.response?.status === 401 && !originalRequest._retry && !isAuthEndpoint) {
      if (isRefreshing) {
        return new Promise<string>((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        })
          .then((token) => {
            if (originalRequest.headers) {
              originalRequest.headers.Authorization = `Bearer ${token}`
            }
            return axiosClient(originalRequest)
          })
          .catch((err) => Promise.reject(err))
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        const response = await axiosClient.post<{ accessToken: string }>('/api/auth/refresh')

        const newAccessToken = response.data.accessToken

        const authStore = useAuthStore()
        authStore.setToken(newAccessToken)

        processQueue(null, newAccessToken)

        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${newAccessToken}`
        }
        return axiosClient(originalRequest)
      } catch (refreshError) {
        processQueue(refreshError as Error, null)

        const authStore = useAuthStore()
        authStore.logout()

        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  },
)

export const axiosInstance = <T = unknown>(config: AxiosRequestConfig): Promise<T> => {
  const controller = new AbortController()

  const promise = axiosClient({
    ...config,
    signal: config.signal || controller.signal,
  }).then(({ data }) => data)

  // @ts-expect-error custom cancel method
  promise.cancel = () => {
    controller.abort()
  }

  return promise as Promise<T>
}
