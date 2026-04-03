import { apiClient } from '@/core/http/apiClient'

export interface AdminUser {
  id: string
  userName: string
  email: string
  dateOfBirth: string
  role: string
}

export interface CreateAdminUserRequest {
  userName: string
  email: string
  password: string
  dateOfBirth: string
  role: string
}

export interface UpdateAdminUserRequest {
  userName: string
  email: string
  dateOfBirth: string
}

export const getAdminUsers = async (): Promise<AdminUser[]> => {
  const response = await apiClient.get<AdminUser[]>('/admin/users')
  return response.data
}

export const getAdminUserById = async (id: string): Promise<AdminUser> => {
  const response = await apiClient.get<AdminUser>(`/admin/users/${id}`)
  return response.data
}

export const createAdminUser = async (payload: CreateAdminUserRequest): Promise<void> => {
  await apiClient.post('/admin/users', payload)
}

export const updateAdminUser = async (id: string, payload: UpdateAdminUserRequest): Promise<void> => {
  await apiClient.put(`/admin/users/${id}`, payload)
}

export const deleteAdminUser = async (id: string): Promise<void> => {
  await apiClient.delete(`/admin/users/${id}`)
}

export const changeAdminUserRole = async (id: string, role: string): Promise<void> => {
  await apiClient.patch(`/admin/users/${id}/role`, { role })
}
