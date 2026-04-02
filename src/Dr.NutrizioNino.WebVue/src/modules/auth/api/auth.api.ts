import { apiClient } from '@/core/http/apiClient'
import type { LoginRequest, LoginResponse, MeResponse } from '@/Interfaces/auth/AuthDto'

export const authApi = {
  login: (data: LoginRequest) =>
    apiClient.post<LoginResponse>('/auth/login', data).then((r) => r.data),

  logout: () => apiClient.post('/auth/logout'),

  me: () => apiClient.get<MeResponse>('/auth/me').then((r) => r.data)
}
