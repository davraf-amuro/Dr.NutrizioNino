import type { FoodNutrient } from './FoodNutrient'

export interface CreatingFoodDto {
  id: string
  name: string
  quantity: number
  unitOfMeasureId: string
  barcode: string
  brandId: string
  calorie: number
  nutrients: FoodNutrient[]
}
