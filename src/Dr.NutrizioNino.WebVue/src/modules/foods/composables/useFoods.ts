import { ref } from 'vue'
import type { Brand } from '@/Interfaces/Brand'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'
import { getBrands } from '@/modules/brands/api/brands.api'
import { createFood, getFoodDashboardRow, getFoodsDashboard, getNewFood } from '@/modules/foods/api/foods.api'
import { getUnitsOfMeasures } from '@/modules/units/api/units.api'

export const useFoods = () => {
  const dashboard = ref<FoodDashboardDto[]>([])
  const selectedFood = ref<FoodDto | null>(null)
  const brands = ref<Brand[]>([])
  const unitsOfMeasures = ref<UnitOfMeasureDto[]>([])
  const isCreating = ref(false)
  const isLoading = ref(false)
  const errorMessage = ref<string | null>(null)

  const withErrorHandling = async <T>(operation: () => Promise<T>): Promise<T | null> => {
    isLoading.value = true
    errorMessage.value = null

    try {
      return await operation()
    } catch (error) {
      errorMessage.value = error instanceof Error ? error.message : 'Errore imprevisto.'
      return null
    } finally {
      isLoading.value = false
    }
  }

  const loadDashboard = async () => {
    const data = await withErrorHandling(() => getFoodsDashboard())
    if (data) {
      dashboard.value = data
    }
  }

  const loadLookups = async () => {
    const [brandsData, unitsData] = await Promise.all([
      withErrorHandling(() => getBrands()),
      withErrorHandling(() => getUnitsOfMeasures())
    ])

    if (brandsData) {
      brands.value = brandsData
    }

    if (unitsData) {
      unitsOfMeasures.value = unitsData
    }
  }

  const startCreateFood = async () => {
    const result = await withErrorHandling(async () => {
      if (!brands.value.length || !unitsOfMeasures.value.length) {
        await loadLookups()
      }

      return getNewFood()
    })

    if (result) {
      selectedFood.value = result
      isCreating.value = true
    }
  }

  const completeCreateFood = async (updatedFood: FoodDto) => {
    const result = await withErrorHandling(async () => {
      const id = await createFood(updatedFood)
      return getFoodDashboardRow(id)
    })

    if (result) {
      dashboard.value.push(result)
      isCreating.value = false
      selectedFood.value = null
    }
  }

  const cancelCreateFood = () => {
    isCreating.value = false
    selectedFood.value = null
  }

  return {
    dashboard,
    selectedFood,
    brands,
    unitsOfMeasures,
    isCreating,
    isLoading,
    errorMessage,
    loadDashboard,
    loadLookups,
    startCreateFood,
    completeCreateFood,
    cancelCreateFood
  }
}
