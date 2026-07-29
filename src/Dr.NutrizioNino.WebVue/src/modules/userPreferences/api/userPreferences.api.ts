import { apiClient } from '@/core/http/apiClient'

export const getChartPreferences = async (): Promise<string[]> => {
  const response = await apiClient.get<{ visibleNutrients: string[] }>('/users/me/chart-preferences')
  return response.data.visibleNutrients
}

export const updateChartPreferences = async (visibleNutrients: string[]): Promise<void> => {
  await apiClient.put('/users/me/chart-preferences', { visibleNutrients })
}
