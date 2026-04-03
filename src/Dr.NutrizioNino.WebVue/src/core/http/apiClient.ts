import axios, { AxiosError } from 'axios'
import { ApiError } from '@/core/http/ApiError'
import router from '@/router'
import { useAuth, getToken } from '@/modules/auth/composables/useAuth'

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '')

export const apiClient = axios.create({
  baseURL: apiBaseUrl,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

apiClient.interceptors.request.use((config) => {
  const token = getToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<{ message?: string; title?: string; detail?: string; traceId?: string }>) => {
    const defaultMessage = 'Errore di comunicazione con il server.'
    const status = error.response?.status
    const payload = error.response?.data
    const message = payload?.detail ?? payload?.message ?? error.message ?? defaultMessage

    if (status === 401 && router.currentRoute.value.name !== 'login') {
      const { resetAuth } = useAuth()
      resetAuth()
      router.push({
        name: 'login',
        query: {
          redirect: router.currentRoute.value.fullPath,
          reason: 'session_expired'
        }
      })
    }

    return Promise.reject(
      new ApiError(message, {
        status,
        title: payload?.title,
        detail: payload?.detail,
        message: payload?.message,
        traceId: payload?.traceId
      })
    )
  }
)
