import { apiClient } from '@/core/http/apiClient'
import { ApiError } from '@/core/http/ApiError'
import type { NutritionalTargetDto, SetNutritionalTargetRequest } from '@/Interfaces/nutritionalTarget/NutritionalTargetDto'

// null se l'utente non ha ancora impostato un fabbisogno (404 dal backend)
export const getMyNutritionalTarget = async (): Promise<NutritionalTargetDto | null> => {
  try {
    const response = await apiClient.get<NutritionalTargetDto>('/users/me/nutritional-target')
    return response.data
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) {
      return null
    }
    throw error
  }
}

export const setMyNutritionalTarget = async (request: SetNutritionalTargetRequest): Promise<NutritionalTargetDto> => {
  const response = await apiClient.put<NutritionalTargetDto>('/users/me/nutritional-target', request)
  return response.data
}
