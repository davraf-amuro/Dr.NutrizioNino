import type { FoodNutrientDto } from './FoodNutrientDto'

export interface FoodDto {
  id: string
  name: string
  quantity: number
  unitOfMeasureId: string | null
  barcode: string | null
  brandId: string | null
  calorie: number
  nutrients: FoodNutrientDto[]
  supermarketIds: string[] | null
}
