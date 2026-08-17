import { computed, ref } from 'vue'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import type {
  RecipeComparisonDto,
  RecipeComparisonItemDto
} from '@/Interfaces/recipes/RecipeComparisonDto'
import { useAsyncState } from '@/core/composables/useAsyncState'
import { compareRecipes, getRecipesDashboard } from '@/modules/recipes/api/recipes.api'

export const maxSlots = 3
export const minSlots = 2
export const maxQuantityGrams = 10000

export interface ComparisonSlot {
  recipeId: string | null
  quantityGrams: number
}

export interface ComparisonColumn {
  recipeId: string
  name: string
  quantityGrams: number
  isNutritionStale: boolean
}

export interface ComparisonRow {
  nutrientId: string
  name: string
  positionOrder: number
  values: Record<string, number | null>
}

const createSlot = (): ComparisonSlot => ({ recipeId: null, quantityGrams: 100 })

export const useRecipeComparison = () => {
  const { isLoading, errorMessage, run } = useAsyncState()
  const availableRecipes = ref<FoodDashboardDto[]>([])
  const slots = ref<ComparisonSlot[]>([createSlot(), createSlot(), createSlot()])
  const comparison = ref<RecipeComparisonDto | null>(null)

  const filledSlots = computed(() =>
    slots.value.filter(
      (slot) =>
        slot.recipeId !== null &&
        slot.quantityGrams > 0 &&
        slot.quantityGrams <= maxQuantityGrams
    )
  )

  const hasDuplicates = computed(() => {
    const ids = filledSlots.value.map((slot) => slot.recipeId)
    return new Set(ids).size !== ids.length
  })

  const canCompare = computed(
    () => filledSlots.value.length >= minSlots && !hasDuplicates.value
  )

  const columns = computed<ComparisonColumn[]>(() =>
    (comparison.value?.items ?? []).map((item: RecipeComparisonItemDto) => ({
      recipeId: item.recipeId,
      name: item.name,
      quantityGrams: item.quantityGrams,
      isNutritionStale: item.isNutritionStale
    }))
  )

  // Unione dei nutrienti per nutrientId: una ricetta priva di quel nutriente lascia il valore a null,
  // che la tabella rende come cella vuota — non come zero.
  const mergedRows = computed<ComparisonRow[]>(() => {
    const items = comparison.value?.items ?? []
    const rows = new Map<string, ComparisonRow>()

    for (const item of items) {
      for (const nutrient of item.nutrients) {
        let row = rows.get(nutrient.nutrientId)
        if (!row) {
          row = {
            nutrientId: nutrient.nutrientId,
            name: nutrient.name,
            positionOrder: nutrient.positionOrder,
            values: Object.fromEntries(items.map((i) => [i.recipeId, null]))
          }
          rows.set(nutrient.nutrientId, row)
        }
        row.values[item.recipeId] = nutrient.quantity
      }
    }

    return [...rows.values()].sort((a, b) => a.positionOrder - b.positionOrder)
  })

  const staleRecipeNames = computed(() =>
    columns.value.filter((column) => column.isNutritionStale).map((column) => column.name)
  )

  const loadRecipes = async () => {
    const data = await run(() => getRecipesDashboard())
    if (data) availableRecipes.value = data
  }

  const compare = async () => {
    if (!canCompare.value) return

    const request = {
      items: filledSlots.value.map((slot) => ({
        recipeId: slot.recipeId as string,
        quantityGrams: slot.quantityGrams
      }))
    }

    const data = await run(() => compareRecipes(request))
    comparison.value = data
  }

  const clearSlot = (index: number) => {
    slots.value[index] = createSlot()
    comparison.value = null
  }

  const reset = () => {
    slots.value = [createSlot(), createSlot(), createSlot()]
    comparison.value = null
  }

  return {
    availableRecipes,
    slots,
    comparison,
    columns,
    mergedRows,
    staleRecipeNames,
    filledSlots,
    hasDuplicates,
    canCompare,
    isLoading,
    errorMessage,
    loadRecipes,
    compare,
    clearSlot,
    reset
  }
}
