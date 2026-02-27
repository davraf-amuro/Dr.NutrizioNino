import { apiClient } from '@/core/http/apiClient'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'

export const getUnitsOfMeasures = async (): Promise<UnitOfMeasureDto[]> => {
  const response = await apiClient.get<UnitOfMeasureDto[]>('/unitsOfMeasures')
  return response.data
}
