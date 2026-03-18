export interface FoodDashboardDto {
  id: string
  name: string
  barcode: string | null
  quantity: number
  brandDescription: string
  calorie: number
  unitOfMeasureDescription: string
  abbreviation: string
}
