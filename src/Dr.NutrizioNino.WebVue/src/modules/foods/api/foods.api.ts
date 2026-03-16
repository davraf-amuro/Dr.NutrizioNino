import { apiClient } from '@/core/http/apiClient'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'

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
