import { ref } from 'vue'
import { useAsyncState } from '@/core/composables/useAsyncState'
import {
  getSimulations,
  getSimulationDetail,
  createSimulation,
  renameSimulation,
  deleteSimulation,
  cloneSimulation,
  addEntry,
  updateEntryQuantity,
  deleteEntry,
  compareSimulations
} from '@/modules/dailySimulations/api/dailySimulations.api'
import type {
  DailySimulationListItemDto,
  DailySimulationDetailDto,
  DailySimulationCompareDto,
  AddSimulationEntryRequest
} from '@/Interfaces/dailySimulations/DailySimulationDto'

export const useDailySimulations = () => {
  const { isLoading, errorMessage, run } = useAsyncState()

  const simulations = ref<DailySimulationListItemDto[]>([])
  const currentDetail = ref<DailySimulationDetailDto | null>(null)
  const compareResult = ref<DailySimulationCompareDto | null>(null)

  const loadSimulations = async () => {
    const data = await run(() => getSimulations())
    if (data) simulations.value = data
  }

  const loadDetail = async (id: string) => {
    const data = await run(() => getSimulationDetail(id))
    if (data) currentDetail.value = data
  }

  const closeDetail = () => {
    currentDetail.value = null
  }

  const create = async (name: string): Promise<string | null> => {
    const result = await run(() => createSimulation(name))
    if (result) {
      await loadSimulations()
      return result.id
    }
    return null
  }

  const rename = async (id: string, name: string): Promise<boolean> => {
    const result = await run(() => renameSimulation(id, name))
    if (result !== null) {
      const item = simulations.value.find((s) => s.id === id)
      if (item) item.name = name
      if (currentDetail.value?.id === id) currentDetail.value = { ...currentDetail.value, name }
      return true
    }
    return false
  }

  const remove = async (id: string): Promise<boolean> => {
    const result = await run(() => deleteSimulation(id))
    if (result !== null) {
      simulations.value = simulations.value.filter((s) => s.id !== id)
      if (currentDetail.value?.id === id) currentDetail.value = null
      return true
    }
    return false
  }

  const clone = async (id: string): Promise<string | null> => {
    const result = await run(() => cloneSimulation(id))
    if (result) {
      await loadSimulations()
      return result.id
    }
    return null
  }

  const addSimulationEntry = async (simulationId: string, dto: AddSimulationEntryRequest): Promise<boolean> => {
    const result = await run(() => addEntry(simulationId, dto))
    if (result !== null) {
      await loadDetail(simulationId)
      return true
    }
    return false
  }

  const updateQuantity = async (simulationId: string, entryId: string, quantityGrams: number): Promise<boolean> => {
    const result = await run(() => updateEntryQuantity(simulationId, entryId, quantityGrams))
    if (result !== null) {
      await loadDetail(simulationId)
      return true
    }
    return false
  }

  const removeEntry = async (simulationId: string, entryId: string): Promise<boolean> => {
    const result = await run(() => deleteEntry(simulationId, entryId))
    if (result !== null) {
      await loadDetail(simulationId)
      return true
    }
    return false
  }

  const compare = async (id1: string, id2: string): Promise<boolean> => {
    const result = await run(() => compareSimulations(id1, id2))
    if (result) {
      compareResult.value = result
      return true
    }
    return false
  }

  const closeCompare = () => {
    compareResult.value = null
  }

  return {
    simulations,
    currentDetail,
    compareResult,
    isLoading,
    errorMessage,
    loadSimulations,
    loadDetail,
    closeDetail,
    create,
    rename,
    remove,
    clone,
    addSimulationEntry,
    updateQuantity,
    removeEntry,
    compare,
    closeCompare
  }
}
