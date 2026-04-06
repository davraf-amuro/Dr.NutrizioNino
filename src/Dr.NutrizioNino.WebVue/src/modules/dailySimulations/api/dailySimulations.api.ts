import { apiClient } from '@/core/http/apiClient'
import type {
  DailySimulationListItemDto,
  DailySimulationDetailDto,
  DailySimulationCompareDto,
  AddSimulationEntryRequest
} from '@/Interfaces/dailySimulations/DailySimulationDto'

export const getSimulations = async (): Promise<DailySimulationListItemDto[]> => {
  const response = await apiClient.get<DailySimulationListItemDto[]>('/daily-simulations')
  return response.data
}

export const getSimulationDetail = async (id: string): Promise<DailySimulationDetailDto> => {
  const response = await apiClient.get<DailySimulationDetailDto>(`/daily-simulations/${id}`)
  return response.data
}

export const createSimulation = async (name: string): Promise<{ id: string }> => {
  const response = await apiClient.post<{ id: string }>('/daily-simulations', { name })
  return response.data
}

export const renameSimulation = async (id: string, name: string): Promise<void> => {
  await apiClient.put(`/daily-simulations/${id}`, { name })
}

export const deleteSimulation = async (id: string): Promise<void> => {
  await apiClient.delete(`/daily-simulations/${id}`)
}

export const cloneSimulation = async (id: string): Promise<{ id: string }> => {
  const response = await apiClient.post<{ id: string }>(`/daily-simulations/${id}/clone`)
  return response.data
}

export const addEntry = async (simulationId: string, dto: AddSimulationEntryRequest): Promise<{ id: string }> => {
  const response = await apiClient.post<{ id: string }>(`/daily-simulations/${simulationId}/entries`, dto)
  return response.data
}

export const updateEntryQuantity = async (simulationId: string, entryId: string, quantityGrams: number): Promise<void> => {
  await apiClient.put(`/daily-simulations/${simulationId}/entries/${entryId}`, { quantityGrams })
}

export const deleteEntry = async (simulationId: string, entryId: string): Promise<void> => {
  await apiClient.delete(`/daily-simulations/${simulationId}/entries/${entryId}`)
}

export const compareSimulations = async (id1: string, id2: string): Promise<DailySimulationCompareDto> => {
  const response = await apiClient.get<DailySimulationCompareDto>('/daily-simulations/compare', {
    params: { id1, id2 }
  })
  return response.data
}
