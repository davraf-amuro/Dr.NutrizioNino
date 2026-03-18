import { ref } from 'vue'
import type { Nutrient } from '@/Interfaces/Nutrients/Nutrient'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import { useAsyncState } from '@/core/composables/useAsyncState'
import { createListDetail } from '@/core/composables/useListDetail'
import {
  createNutrient,
  deleteNutrient,
  getNutrientById,
  getNutrients,
  updateNutrient,
  type CreateNutrientRequest
} from '@/modules/nutrients/api/nutrients.api'
import { getUnitsOfMeasures } from '@/modules/units/api/units.api'

const cacheTtlMs = 60_000

// Units cache shared across nutrient composable instances
let unitsCache: UnitOfMeasureDto[] | null = null
let unitsCacheAt = 0

const useNutrientsBase = createListDetail<Nutrient, CreateNutrientRequest>({
  getAll: getNutrients,
  getById: getNutrientById,
  create: createNutrient,
  update: updateNutrient,
  remove: deleteNutrient
})

export const useNutrients = () => {
  const base = useNutrientsBase()
  const { run } = useAsyncState()
  const unitsOfMeasures = ref<UnitOfMeasureDto[]>([])

  const loadUnits = async (force = false) => {
    if (!force && unitsCache && Date.now() - unitsCacheAt < cacheTtlMs) {
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

  const startEditNutrient = async (nutrient: Nutrient) => {
    await loadUnits()
    await base.startEdit(nutrient)
  }

  return {
    nutrients: base.items,
    unitsOfMeasures,
    isEditing: base.isEditing,
    formMode: base.formMode,
    selectedNutrient: base.selectedItem,
    isLoading: base.isLoading,
    errorMessage: base.errorMessage,
    loadNutrients: base.load,
    startCreateNutrient: base.startCreate,
    startEditNutrient,
    cancelEdit: base.cancelEdit,
    submitNutrient: base.submit,
    removeNutrient: base.remove
  }
}
