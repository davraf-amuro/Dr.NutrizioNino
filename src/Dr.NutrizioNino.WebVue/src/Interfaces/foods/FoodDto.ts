import type { FoodNutrientDto } from './FoodNutrientDto'

export interface FoodDto {
  id: string
  name: string
  quantity: number
  barcode: string | null
  brandId: string | null
  calorie: number
  nutrients: FoodNutrientDto[]
}
