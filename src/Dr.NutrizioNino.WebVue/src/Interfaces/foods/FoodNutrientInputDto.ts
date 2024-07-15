import type { FoodNutrient } from './FoodNutrient'
import type { UnitOfMeasureDto } from '../UnitOfMeasureDto'

export interface FoodNutrientInputDto {
  foodNutrient: FoodNutrient
  unitsOfMeasures: UnitOfMeasureDto[]
  unitOfMeasureSelectedId: string
}
