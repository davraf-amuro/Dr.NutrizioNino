import type { Nutrient } from '../Nutrient'

export interface Food {
  id: string
  name: string
  quantity: number
  basrcode: string
  brandId: string
  calorie: number
  unitOfMeasureDescription: string
  nutrients: Nutrient[]
}
