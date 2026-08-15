import { apiClient } from '@/core/http/apiClient'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import type { RecipeIngredientDto } from '@/Interfaces/recipes/RecipeIngredientDto'
import type { RecipeDetailDto } from '@/Interfaces/recipes/RecipeDetailDto'

export interface CreateRecipeRequest {
  name: string
  ingredients: RecipeIngredientDto[]
}

export const getRecipesDashboard = async (): Promise<FoodDashboardDto[]> => {
  const response = await apiClient.get<FoodDashboardDto[]>('/recipes/dashboard')
  return response.data
}

export const createRecipe = async (dto: CreateRecipeRequest): Promise<string> => {
  const response = await apiClient.post<string>('/recipes', dto)
  return response.data
}

export const getRecipe = async (id: string): Promise<RecipeDetailDto> => {
  const response = await apiClient.get<RecipeDetailDto>(`/recipes/${id}`)
  return response.data
}

export const deleteRecipe = async (id: string): Promise<void> => {
  await apiClient.delete(`/recipes/${id}`)
}
