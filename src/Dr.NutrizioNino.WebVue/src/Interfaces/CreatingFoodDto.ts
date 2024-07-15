import type { FoodNutrient } from './foods/FoodNutrient'

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
