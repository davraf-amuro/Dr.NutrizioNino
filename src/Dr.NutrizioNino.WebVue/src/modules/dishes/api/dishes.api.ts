import { apiClient } from '@/core/http/apiClient'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import type { DishIngredientDto } from '@/Interfaces/dishes/DishIngredientDto'
import type { DishDetailDto } from '@/Interfaces/dishes/DishDetailDto'

export interface CreateDishRequest {
  name: string
  ingredients: DishIngredientDto[]
}

export const getDishesDashboard = async (): Promise<FoodDashboardDto[]> => {
  const response = await apiClient.get<FoodDashboardDto[]>('/dishes/dashboard')
  return response.data
}

export const createDish = async (dto: CreateDishRequest): Promise<string> => {
  const response = await apiClient.post<string>('/dishes', dto)
  return response.data
}

export const getDish = async (id: string): Promise<DishDetailDto> => {
  const response = await apiClient.get<DishDetailDto>(`/dishes/${id}`)
  return response.data
}

export const deleteDish = async (id: string): Promise<void> => {
  await apiClient.delete(`/dishes/${id}`)
}
