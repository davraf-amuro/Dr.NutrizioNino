export interface RecipeDetailNutrientDto {
  nutrientId: string
  name: string
  positionOrder: number
  unitOfMeasureId: string
  quantity: number
}

export interface RecipeDetailIngredientDto {
  foodId: string
  foodName: string
  quantityGrams: number
}

export interface RecipeDetailDto {
  id: string
  name: string
  weightGrams: number
  ingredients: RecipeDetailIngredientDto[]
  nutrients: RecipeDetailNutrientDto[]
}
