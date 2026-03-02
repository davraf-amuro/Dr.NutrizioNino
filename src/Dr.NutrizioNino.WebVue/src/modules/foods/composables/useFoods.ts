import { ref } from 'vue'
import type { Brand } from '@/Interfaces/Brand'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'
import { useAsyncState } from '@/core/composables/useAsyncState'
import { getBrands } from '@/modules/brands/api/brands.api'
import { createFood, getFoodDashboardRow, getFoodsDashboard, getNewFood } from '@/modules/foods/api/foods.api'
import { getUnitsOfMeasures } from '@/modules/units/api/units.api'

const cacheTtlMs = 60_000
let dashboardCache: FoodDashboardDto[] | null = null
let dashboardCacheAt = 0
let brandsCache: Brand[] | null = null
let unitsCache: UnitOfMeasureDto[] | null = null
let lookupsCacheAt = 0

export const useFoods = () => {
  const { isLoading, errorMessage, run } = useAsyncState()
  const dashboard = ref<FoodDashboardDto[]>([])
  const selectedFood = ref<FoodDto | null>(null)
  const brands = ref<Brand[]>([])
  const unitsOfMeasures = ref<UnitOfMeasureDto[]>([])
  const isCreating = ref(false)

  const updateDashboardCache = (items: FoodDashboardDto[]) => {
    dashboardCache = [...items]
    dashboardCacheAt = Date.now()
  }

  const updateLookupsCache = (newBrands: Brand[], newUnits: UnitOfMeasureDto[]) => {
    brandsCache = [...newBrands]
    unitsCache = [...newUnits]
    lookupsCacheAt = Date.now()
  }

  const loadDashboard = async (force = false) => {
    const hasValidCache = !force && dashboardCache && Date.now() - dashboardCacheAt < cacheTtlMs
    if (hasValidCache && dashboardCache) {
      dashboard.value = [...dashboardCache]
      return
    }

    const data = await run(() => getFoodsDashboard())
    if (data) {
      dashboard.value = data
      updateDashboardCache(data)
    }
  }

  const loadLookups = async (force = false) => {
    const hasValidCache = !force && brandsCache && unitsCache && Date.now() - lookupsCacheAt < cacheTtlMs
    if (hasValidCache && brandsCache && unitsCache) {
      brands.value = [...brandsCache]
      unitsOfMeasures.value = [...unitsCache]
      return
    }

    const [brandsData, unitsData] = await Promise.all([
      run(() => getBrands()),
      run(() => getUnitsOfMeasures())
    ])

    if (brandsData) {
      brands.value = brandsData
    }

    if (unitsData) {
      unitsOfMeasures.value = unitsData
    }

    if (brandsData && unitsData) {
      updateLookupsCache(brandsData, unitsData)
    }
  }

  const startCreateFood = async () => {
    const result = await run(async () => {
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
    const result = await run(async () => {
      const id = await createFood(updatedFood)
      return getFoodDashboardRow(id)
    })

    if (result) {
      dashboard.value.push(result)
      updateDashboardCache(dashboard.value)
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
