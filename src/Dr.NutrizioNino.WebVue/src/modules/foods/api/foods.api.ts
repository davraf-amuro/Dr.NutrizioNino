import { apiClient } from '@/core/http/apiClient'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'
import type { ExtractionResultDto } from '@/Interfaces/foods/ExtractedNutrientDto'
import type { FoodSuggestionDto } from '@/Interfaces/foods/FoodSuggestionDto'

export interface VisionProviderDto {
  key: string
  label: string
}

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

export const extractNutrientsFromImage = async (
  base64Image: string,
  providerKey: string,
  mediaType = 'image/jpeg',
  signal?: AbortSignal
): Promise<ExtractionResultDto> => {
  const response = await apiClient.post<ExtractionResultDto>(
    '/foods/extract-nutrients',
    { base64Image, providerKey, mediaType },
    { signal, timeout: 0 }  // timeout 0 = nessun timeout fisso; l'utente annulla manualmente
  )
  return response.data
}

export const getVisionProviders = async (): Promise<VisionProviderDto[]> => {
  const response = await apiClient.get<VisionProviderDto[]>('/vision/providers')
  return response.data
}

export const saveNutrientAlias = async (aiName: string, nutrientId: string): Promise<void> => {
  await apiClient.post('/nutrients/aliases', { aiName, nutrientId })
}

export const getSimilarFoodNames = async (query: string, signal?: AbortSignal): Promise<FoodSuggestionDto[]> => {
  const response = await apiClient.get<FoodSuggestionDto[]>('/foods/similar', { params: { query }, signal })
  return response.data
}
