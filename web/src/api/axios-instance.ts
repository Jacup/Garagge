import axios, { type AxiosRequestConfig, type AxiosError } from 'axios'
import { useAuthStore } from '@/stores/auth'

interface FailedQueueItem {
  resolve: () => void
  reject: (error: unknown) => void
}

interface RetryAxiosRequestConfig extends AxiosRequestConfig {
  _retry?: boolean
}

let isRefreshing = false
let failedQueue: FailedQueueItem[] = []

const processQueue = (error: Error | null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error)
    } else {
      prom.resolve()
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

axiosClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as RetryAxiosRequestConfig

    if (!originalRequest) return Promise.reject(error)

    const isAuthEndpoint = originalRequest.url?.includes('/api/auth')
    if (error.response?.status === 401 && !originalRequest._retry && !isAuthEndpoint) {
      if (isRefreshing) {
        return new Promise<void>((resolve, reject) => {
          failedQueue.push({ resolve, reject })
        })
          .then(() => axiosClient(originalRequest))
          .catch((err) => Promise.reject(err))
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        await axiosClient.post('/api/auth/refresh')

        processQueue(null)
        return axiosClient(originalRequest)
      } catch (refreshError) {
        processQueue(refreshError as Error)

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
