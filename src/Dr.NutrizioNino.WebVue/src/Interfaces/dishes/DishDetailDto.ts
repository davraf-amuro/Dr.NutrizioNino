export interface DishDetailNutrientDto {
  nutrientId: string
  name: string
  positionOrder: number
  unitOfMeasureId: string
  quantity: number
}

export interface DishDetailIngredientDto {
  foodId: string
  foodName: string
  quantityGrams: number
}

export interface DishDetailDto {
  id: string
  name: string
  weightGrams: number
  ingredients: DishDetailIngredientDto[]
  nutrients: DishDetailNutrientDto[]
}
