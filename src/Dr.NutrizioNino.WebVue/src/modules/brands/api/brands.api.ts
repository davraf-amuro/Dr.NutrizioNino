import { apiClient } from '@/core/http/apiClient'
import type { Brand } from '@/Interfaces/Brand'

export interface CreateBrandRequest {
  name: string
}

export const getBrands = async (): Promise<Brand[]> => {
  const response = await apiClient.get<Brand[]>('/brands')
  return response.data
}

export const createBrand = async (payload: CreateBrandRequest): Promise<Brand> => {
  const response = await apiClient.post<Brand>('/brands', payload)
  return response.data
}

export const updateBrand = async (brand: Brand): Promise<void> => {
  await apiClient.put(`/brands/${brand.id}`, brand)
}

export const deleteBrand = async (id: string): Promise<void> => {
  await apiClient.delete(`/brands/${id}`)
}

export const isBrandInUse = async (id: string): Promise<boolean> => {
  const response = await apiClient.get<boolean>(`/brands/${id}/is-in-use`)
  return response.data
}
