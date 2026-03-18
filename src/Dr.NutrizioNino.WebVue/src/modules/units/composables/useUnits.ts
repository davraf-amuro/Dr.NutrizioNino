import { ref } from 'vue'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import { useAsyncState } from '@/core/composables/useAsyncState'
import {
  createUnitOfMeasure,
  deleteUnitOfMeasure,
  getUnitOfMeasureById,
  getUnitsOfMeasures,
  updateUnitOfMeasure
} from '@/modules/units/api/units.api'

type UnitFormMode = 'create' | 'edit'

const cacheTtlMs = 60_000
let unitsCache: UnitOfMeasureDto[] | null = null
let unitsCacheAt = 0

export const useUnits = () => {
  const { isLoading, errorMessage, run } = useAsyncState()
  const units = ref<UnitOfMeasureDto[]>([])
  const isEditing = ref(false)
  const formMode = ref<UnitFormMode>('create')
  const selectedUnit = ref<UnitOfMeasureDto | null>(null)

  const updateCache = (items: UnitOfMeasureDto[]) => {
    unitsCache = [...items]
    unitsCacheAt = Date.now()
  }

  const loadUnits = async (force = false) => {
    const hasValidCache = !force && unitsCache && Date.now() - unitsCacheAt < cacheTtlMs
    if (hasValidCache && unitsCache) {
      units.value = [...unitsCache]
      return
    }

    const result = await run(() => getUnitsOfMeasures())
    if (result) {
      units.value = result
      updateCache(result)
    }
  }

  const startCreateUnit = () => {
    formMode.value = 'create'
    selectedUnit.value = null
    isEditing.value = true
  }

  const startEditUnit = async (unit: UnitOfMeasureDto) => {
    formMode.value = 'edit'
    const result = await run(() => getUnitOfMeasureById(unit.id))
    if (result) {
      selectedUnit.value = result
      isEditing.value = true
    }
  }

  const cancelEdit = () => {
    selectedUnit.value = null
    isEditing.value = false
  }

  const submitUnit = async (payload: UnitOfMeasureDto) => {
    if (formMode.value === 'edit' && selectedUnit.value) {
      const result = await run(async () => {
        await updateUnitOfMeasure(payload)
        return payload
      })

      if (result) {
        const index = units.value.findIndex((u) => u.id === result.id)
        if (index >= 0) {
          units.value[index] = result
        }
        updateCache(units.value)
        selectedUnit.value = null
        isEditing.value = false
      }
      return
    }

    const created = await run(() =>
      createUnitOfMeasure({ name: payload.name, abbreviation: payload.abbreviation })
    )
    if (created) {
      units.value.push(created)
      updateCache(units.value)
      isEditing.value = false
    }
  }

  const removeUnit = async (unit: UnitOfMeasureDto) => {
    const removed = await run(async () => {
      await deleteUnitOfMeasure(unit.id)
      return unit.id
    })

    if (removed) {
      units.value = units.value.filter((u) => u.id !== removed)
      updateCache(units.value)
    }
  }

  return {
    units,
    isEditing,
    formMode,
    selectedUnit,
    isLoading,
    errorMessage,
    loadUnits,
    startCreateUnit,
    startEditUnit,
    cancelEdit,
    submitUnit,
    removeUnit
  }
}
