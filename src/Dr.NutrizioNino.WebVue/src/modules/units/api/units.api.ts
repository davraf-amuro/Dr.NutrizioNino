import { apiClient } from '@/core/http/apiClient'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'

export interface CreateUnitOfMeasureRequest {
  name: string
  abbreviation: string
}

export const getUnitsOfMeasures = async (): Promise<UnitOfMeasureDto[]> => {
  const response = await apiClient.get<UnitOfMeasureDto[]>('/unitsOfMeasures')
  return response.data
}

export const getUnitOfMeasureById = async (id: string): Promise<UnitOfMeasureDto> => {
  const response = await apiClient.get<UnitOfMeasureDto>(`/unitsOfMeasures/${id}`)
  return response.data
}

export const createUnitOfMeasure = async (payload: CreateUnitOfMeasureRequest): Promise<UnitOfMeasureDto> => {
  const response = await apiClient.post<UnitOfMeasureDto>('/unitsOfMeasures', payload)
  return response.data
}

export const updateUnitOfMeasure = async (uom: UnitOfMeasureDto): Promise<void> => {
  await apiClient.put(`/unitsOfMeasures/${uom.id}`, uom)
}

export const deleteUnitOfMeasure = async (id: string): Promise<void> => {
  await apiClient.delete(`/unitsOfMeasures/${id}`)
}
