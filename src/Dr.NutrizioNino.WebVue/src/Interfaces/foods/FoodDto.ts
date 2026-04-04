import type { FoodNutrientDto } from './FoodNutrientDto'

export interface FoodDto {
  id: string
  name: string
  quantity: number
  unitOfMeasureId: string | null
  barcode: string | null
  brandId: string | null
  nutrients: FoodNutrientDto[]
  supermarketIds: string[] | null
  categoryIds: string[] | null
}
