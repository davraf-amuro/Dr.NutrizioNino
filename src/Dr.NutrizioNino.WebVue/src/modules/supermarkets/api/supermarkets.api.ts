import { apiClient } from '@/core/http/apiClient'
import type { Supermarket } from '@/Interfaces/Supermarket'

export interface CreateSupermarketRequest {
  name: string
}

export const getSupermarkets = async (): Promise<Supermarket[]> => {
  const response = await apiClient.get<Supermarket[]>('/supermarkets')
  return response.data
}

export const createSupermarket = async (payload: CreateSupermarketRequest): Promise<Supermarket> => {
  const response = await apiClient.post<Supermarket>('/supermarkets', payload)
  return response.data
}

export const updateSupermarket = async (supermarket: Supermarket): Promise<void> => {
  await apiClient.put(`/supermarkets/${supermarket.id}`, supermarket)
}

export const deleteSupermarket = async (id: string): Promise<void> => {
  await apiClient.delete(`/supermarkets/${id}`)
}

export const isSupermarketInUse = async (id: string): Promise<boolean> => {
  const response = await apiClient.get<boolean>(`/supermarkets/${id}/is-in-use`)
  return response.data
}
