import { apiClient } from '@/core/http/apiClient'
import type { SimulationSectionDto, SimulationSectionReorderItem } from '@/Interfaces/sectionConfigs/SectionConfigDto'

export const sectionConfigsApi = {
  getAll: () =>
    apiClient.get<SimulationSectionDto[]>('/sections').then((r) => r.data),

  getActive: () =>
    apiClient.get<SimulationSectionDto[]>('/sections/active').then((r) => r.data),

  create: (name: string) =>
    apiClient.post('/sections', { name }),

  update: (id: string, name: string) =>
    apiClient.put(`/sections/${id}`, { name }),

  delete: (id: string) =>
    apiClient.delete(`/sections/${id}`),

  reorder: (items: SimulationSectionReorderItem[]) =>
    apiClient.put('/sections/reorder', items)
}
