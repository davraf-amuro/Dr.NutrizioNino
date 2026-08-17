import { apiClient } from '@/core/http/apiClient'
import { ApiError } from '@/core/http/ApiError'
import type { AddProfileEntryRequest, ProfileEntryDto } from '@/Interfaces/userProfile/ProfileEntryDto'
import type { SciaudoneCardDto } from '@/Interfaces/userProfile/SciaudoneCardDto'

/** Ultima misurazione registrata, null se l'utente non ne ha ancora (404 dal backend). */
export const getCurrentProfileEntry = async (): Promise<ProfileEntryDto | null> => {
  try {
    const response = await apiClient.get<ProfileEntryDto>('/users/me/profile/current')
    return response.data
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) {
      return null
    }
    throw error
  }
}

/** Registra una nuova misurazione: il backend calcola la scheda se peso e peso ideale sono presenti. */
export const addProfileEntry = async (request: AddProfileEntryRequest): Promise<ProfileEntryDto> => {
  const response = await apiClient.post<ProfileEntryDto>('/users/me/profile', request)
  return response.data
}

/** Scheda Sciaudone corrente, null se non ancora calcolabile (404 dal backend). */
export const getSciaudoneCard = async (): Promise<SciaudoneCardDto | null> => {
  try {
    const response = await apiClient.get<SciaudoneCardDto>('/users/me/profile/sciaudone')
    return response.data
  } catch (error) {
    if (error instanceof ApiError && error.status === 404) {
      return null
    }
    throw error
  }
}

/** Storico delle schede, dalla più recente. */
export const getSciaudoneHistory = async (): Promise<SciaudoneCardDto[]> => {
  const response = await apiClient.get<SciaudoneCardDto[]>('/users/me/profile/sciaudone/history')
  return response.data
}
