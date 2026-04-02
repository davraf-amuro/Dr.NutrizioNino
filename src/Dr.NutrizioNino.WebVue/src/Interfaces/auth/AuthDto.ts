export interface LoginRequest {
  userName: string
  password: string
}

export interface LoginResponse {
  userName: string
  role: string
}

export interface MeResponse {
  id: string
  userName: string
  email: string
  dateOfBirth: string
  role: string
}
