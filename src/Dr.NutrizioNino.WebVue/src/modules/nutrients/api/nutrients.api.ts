import { apiClient } from '@/core/http/apiClient'
import type { Nutrient } from '@/Interfaces/Nutrients/Nutrient'

export interface CreateNutrientRequest {
  name: string
}

export const getNutrients = async (): Promise<Nutrient[]> => {
  const response = await apiClient.get<Nutrient[]>('/nutrients')
  return response.data
}

export const getNutrientById = async (id: string): Promise<Nutrient> => {
  const response = await apiClient.get<Nutrient>(`/nutrients/${id}`)
  return response.data
}

export const createNutrient = async (payload: CreateNutrientRequest): Promise<Nutrient> => {
  const response = await apiClient.post<Nutrient>('/nutrients', payload)
  return response.data
}

export const updateNutrient = async (nutrient: Nutrient): Promise<void> => {
  await apiClient.put(`/nutrients/${nutrient.id}`, nutrient)
}

export const deleteNutrient = async (id: string): Promise<void> => {
  await apiClient.delete(`/nutrients/${id}`)
}
