import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import { createListDetail } from '@/core/composables/useListDetail'
import {
  createUnitOfMeasure,
  deleteUnitOfMeasure,
  getUnitOfMeasureById,
  getUnitsOfMeasures,
  updateUnitOfMeasure,
  type CreateUnitOfMeasureRequest
} from '@/modules/units/api/units.api'

const useUnitsBase = createListDetail<UnitOfMeasureDto, CreateUnitOfMeasureRequest>({
  getAll: getUnitsOfMeasures,
  getById: getUnitOfMeasureById,
  create: createUnitOfMeasure,
  update: updateUnitOfMeasure,
  remove: deleteUnitOfMeasure
})

export const useUnits = () => {
  const { items, selectedItem, startCreate, startEdit, submit, remove, ...rest } = useUnitsBase()

  return {
    ...rest,
    units: items,
    selectedUnit: selectedItem,
    loadUnits: rest.load,
    startCreateUnit: startCreate,
    startEditUnit: startEdit,
    submitUnit: submit,
    removeUnit: remove
  }
}
