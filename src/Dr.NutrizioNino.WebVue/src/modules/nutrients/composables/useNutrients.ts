import { ref } from 'vue'
import type { Nutrient } from '@/Interfaces/Nutrients/Nutrient'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import { useAsyncState } from '@/core/composables/useAsyncState'
import { createNutrient, deleteNutrient, getNutrientById, getNutrients, updateNutrient } from '@/modules/nutrients/api/nutrients.api'
import { getUnitsOfMeasures } from '@/modules/units/api/units.api'

type NutrientFormMode = 'create' | 'edit'

const cacheTtlMs = 60_000
let nutrientsCache: Nutrient[] | null = null
let nutrientsCacheAt = 0
let unitsCache: UnitOfMeasureDto[] | null = null
let unitsCacheAt = 0

export const useNutrients = () => {
  const { isLoading, errorMessage, run } = useAsyncState()
  const nutrients = ref<Nutrient[]>([])
  const unitsOfMeasures = ref<UnitOfMeasureDto[]>([])
  const isEditing = ref(false)
  const formMode = ref<NutrientFormMode>('create')
  const selectedNutrient = ref<Nutrient | null>(null)

  const updateCache = (items: Nutrient[]) => {
    nutrientsCache = [...items]
    nutrientsCacheAt = Date.now()
  }

  const loadNutrients = async (force = false) => {
    const hasValidCache = !force && nutrientsCache && Date.now() - nutrientsCacheAt < cacheTtlMs
    if (hasValidCache && nutrientsCache) {
      nutrients.value = [...nutrientsCache]
      return
    }

    const result = await run(() => getNutrients())
    if (result) {
      nutrients.value = result
      updateCache(result)
    }
  }

  const loadUnits = async () => {
    if (unitsCache && Date.now() - unitsCacheAt < cacheTtlMs) {
      unitsOfMeasures.value = [...unitsCache]
      return
    }

    const result = await run(() => getUnitsOfMeasures())
    if (result) {
      unitsOfMeasures.value = result
      unitsCache = [...result]
      unitsCacheAt = Date.now()
    }
  }

  const startCreateNutrient = () => {
    formMode.value = 'create'
    selectedNutrient.value = null
    isEditing.value = true
  }

  const startEditNutrient = async (nutrient: Nutrient) => {
    formMode.value = 'edit'

    const result = await run(async () => {
      await loadUnits()
      return getNutrientById(nutrient.id)
    })

    if (result) {
      selectedNutrient.value = result
      isEditing.value = true
    }
  }

  const cancelEdit = () => {
    selectedNutrient.value = null
    isEditing.value = false
  }

  const submitNutrient = async (payload: { name: string } | Nutrient) => {
    if (formMode.value === 'edit' && selectedNutrient.value) {
      const updated = payload as Nutrient
      const result = await run(async () => {
        await updateNutrient(updated)
        return updated
      })

      if (result) {
        const index = nutrients.value.findIndex((n) => n.id === result.id)
        if (index >= 0) {
          nutrients.value[index] = result
        }
        updateCache(nutrients.value)
        selectedNutrient.value = null
        isEditing.value = false
      }
      return
    }

    const created = await run(() => createNutrient({ name: (payload as { name: string }).name }))
    if (created) {
      nutrients.value.push(created)
      updateCache(nutrients.value)
      isEditing.value = false
    }
  }

  const removeNutrient = async (nutrient: Nutrient) => {
    const removed = await run(async () => {
      await deleteNutrient(nutrient.id)
      return nutrient.id
    })

    if (removed) {
      nutrients.value = nutrients.value.filter((n) => n.id !== removed)
      updateCache(nutrients.value)
    }
  }

  return {
    nutrients,
    unitsOfMeasures,
    isEditing,
    formMode,
    selectedNutrient,
    isLoading,
    errorMessage,
    loadNutrients,
    startCreateNutrient,
    startEditNutrient,
    cancelEdit,
    submitNutrient,
    removeNutrient
  }
}
