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

export const createFood = async (food: FoodDto): Promise<string> => {
  const response = await apiClient.post<string>('/foods/create', food)
  return response.data
}
