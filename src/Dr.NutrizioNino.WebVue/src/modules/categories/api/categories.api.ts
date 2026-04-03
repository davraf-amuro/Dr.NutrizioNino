import { apiClient } from '@/core/http/apiClient'
import type { Category } from '@/Interfaces/Category'

export interface CreateCategoryRequest {
  name: string
}

export const getCategories = async (): Promise<Category[]> => {
  const response = await apiClient.get<Category[]>('/categories')
  return response.data
}

export const createCategory = async (payload: CreateCategoryRequest): Promise<Category> => {
  const response = await apiClient.post<Category>('/categories', payload)
  return response.data
}

export const updateCategory = async (category: Category): Promise<void> => {
  await apiClient.put(`/categories/${category.id}`, category)
}

export const deleteCategory = async (id: string): Promise<void> => {
  await apiClient.delete(`/categories/${id}`)
}

export const isCategoryInUse = async (id: string): Promise<boolean> => {
  const response = await apiClient.get<boolean>(`/categories/${id}/is-in-use`)
  return response.data
}
