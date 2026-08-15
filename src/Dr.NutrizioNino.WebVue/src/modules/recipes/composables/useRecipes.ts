import { ref } from 'vue'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import type { RecipeDetailDto } from '@/Interfaces/recipes/RecipeDetailDto'
import { useAsyncState } from '@/core/composables/useAsyncState'
import { createRecipe, deleteRecipe, getRecipe, getRecipesDashboard, type CreateRecipeRequest } from '@/modules/recipes/api/recipes.api'
import { getFoodsDashboard } from '@/modules/foods/api/foods.api'

const cacheTtlMs = 60_000
let recipesCache: FoodDashboardDto[] | null = null
let recipesCacheAt = 0
let foodsCache: FoodDashboardDto[] | null = null
let foodsCacheAt = 0

export const useRecipes = () => {
  const { isLoading, errorMessage, run } = useAsyncState()
  const recipes = ref<FoodDashboardDto[]>([])
  const availableFoods = ref<FoodDashboardDto[]>([])
  const isCreating = ref(false)
  const recipeDetail = ref<RecipeDetailDto | null>(null)

  const loadRecipes = async (force = false) => {
    const hasValidCache = !force && recipesCache && Date.now() - recipesCacheAt < cacheTtlMs
    if (hasValidCache && recipesCache) {
      recipes.value = [...recipesCache]
      return
    }
    const data = await run(() => getRecipesDashboard())
    if (data) {
      recipes.value = data
      recipesCache = [...data]
      recipesCacheAt = Date.now()
    }
  }

  const loadAvailableFoods = async (force = false) => {
    const hasValidCache = !force && foodsCache && Date.now() - foodsCacheAt < cacheTtlMs
    if (hasValidCache && foodsCache) {
      availableFoods.value = [...foodsCache]
      return
    }
    const data = await run(() => getFoodsDashboard())
    if (data) {
      availableFoods.value = data
      foodsCache = [...data]
      foodsCacheAt = Date.now()
    }
  }

  const startCreate = () => {
    isCreating.value = true
  }

  const cancelCreate = () => {
    isCreating.value = false
  }

  const viewRecipe = async (recipe: FoodDashboardDto) => {
    const data = await run(() => getRecipe(recipe.id))
    if (data) recipeDetail.value = data
  }

  const closeDetail = () => {
    recipeDetail.value = null
  }

  const completeRecipe = async (dto: CreateRecipeRequest) => {
    const result = await run(() => createRecipe(dto))
    if (result) {
      isCreating.value = false
      await loadRecipes(true)
    }
  }

  const removeRecipe = async (recipe: FoodDashboardDto) => {
    const removed = await run(async () => {
      await deleteRecipe(recipe.id)
      return recipe.id
    })
    if (removed) {
      recipes.value = recipes.value.filter((r) => r.id !== removed)
      recipesCache = [...recipes.value]
    }
  }

  return {
    recipes,
    availableFoods,
    isCreating,
    recipeDetail,
    isLoading,
    errorMessage,
    loadRecipes,
    loadAvailableFoods,
    startCreate,
    cancelCreate,
    completeRecipe,
    removeRecipe,
    viewRecipe,
    closeDetail
  }
}
