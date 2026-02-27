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
