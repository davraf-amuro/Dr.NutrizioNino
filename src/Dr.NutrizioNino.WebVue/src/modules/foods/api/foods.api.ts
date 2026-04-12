import { apiClient } from '@/core/http/apiClient'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'
import type { ExtractedNutrientDto } from '@/Interfaces/foods/ExtractedNutrientDto'

export const getFoodsDashboard = async (): Promise<FoodDashboardDto[]> => {
  const response = await apiClient.get<FoodDashboardDto[]>('/foods/dashboard')
  return response.data
}

export const getFoodDashboardRow = async (id: string): Promise<FoodDashboardDto> => {
  const response = await apiClient.get<FoodDashboardDto>(`/foods/dashboard/${id}`)
  return response.data
}

export const getNewFood = async (): Promise<FoodDto> => {
  const response = await apiClient.get<FoodDto>('/foods/getnewfood')
  return response.data
}

export const getFoodById = async (id: string): Promise<FoodDto> => {
  const response = await apiClient.get<FoodDto>(`/foods/${id}`)
  return response.data
}

export const createFood = async (food: FoodDto): Promise<string> => {
  const response = await apiClient.post<string>('/foods/Create', food)
  return response.data
}

export const updateFood = async (food: FoodDto): Promise<void> => {
  await apiClient.put(`/foods/${food.id}`, food)
}

export const deleteFood = async (id: string): Promise<void> => {
  await apiClient.delete(`/foods/${id}`)
}

export const cloneFood = async (id: string): Promise<string> => {
  const response = await apiClient.post<{ id: string }>(`/foods/${id}/clone`)
  return response.data.id
}

export const extractNutrientsFromImage = async (base64Image: string, mediaType = 'image/jpeg'): Promise<ExtractedNutrientDto[]> => {
  const response = await apiClient.post<ExtractedNutrientDto[]>('/foods/extract-nutrients', { base64Image, mediaType }, { timeout: 180_000 })
  return response.data
}
