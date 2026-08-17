export interface CompareRecipeItem {
  recipeId: string
  quantityGrams: number
}

export interface CompareRecipesRequest {
  items: CompareRecipeItem[]
}

export interface RecipeComparisonNutrientDto {
  nutrientId: string
  name: string
  positionOrder: number
  unitOfMeasureId: string
  quantity: number
}

export interface RecipeComparisonItemDto {
  recipeId: string
  name: string
  baseWeightGrams: number
  quantityGrams: number
  isNutritionStale: boolean
  nutrients: RecipeComparisonNutrientDto[]
}

export interface RecipeComparisonDto {
  items: RecipeComparisonItemDto[]
}
