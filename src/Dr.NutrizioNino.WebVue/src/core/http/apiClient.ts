import axios, { AxiosError } from 'axios'

const apiBaseUrl = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '')

export const apiClient = axios.create({
  baseURL: apiBaseUrl,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json'
  }
})

apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<{ message?: string }>) => {
    const defaultMessage = 'Errore di comunicazione con il server.'
    const message = error.response?.data?.message ?? error.message ?? defaultMessage
    return Promise.reject(new Error(message))
  }
)
