export interface LoginRequest {
  userName: string
  password: string
}

export interface LoginResponse {
  token: string
  userName: string
  role: string
}

export interface MeResponse {
  id: string
  userName: string
  email: string
  dateOfBirth: string
  role: string
  themePreference: string
}
