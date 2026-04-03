import { ref } from 'vue'
import type { Brand } from '@/Interfaces/Brand'
import type { Supermarket } from '@/Interfaces/Supermarket'
import type { Category } from '@/Interfaces/Category'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'
import { useAsyncState } from '@/core/composables/useAsyncState'
import { getBrands } from '@/modules/brands/api/brands.api'
import { getSupermarkets } from '@/modules/supermarkets/api/supermarkets.api'
import { getCategories } from '@/modules/categories/api/categories.api'
import {
  createFood,
  deleteFood,
  getFoodById,
  getFoodDashboardRow,
  getFoodsDashboard,
  getNewFood,
  updateFood
} from '@/modules/foods/api/foods.api'
import { getUnitsOfMeasures } from '@/modules/units/api/units.api'

const cacheTtlMs = 60_000
let dashboardCache: FoodDashboardDto[] | null = null
let dashboardCacheAt = 0
let brandsCache: Brand[] | null = null
let unitsCache: UnitOfMeasureDto[] | null = null
let supermarketsCache: Supermarket[] | null = null
let categoriesCache: Category[] | null = null
let lookupsCacheAt = 0
type FoodFormMode = 'create' | 'edit'

export const useFoods = () => {
  const { isLoading, errorMessage, run } = useAsyncState()
  const dashboard = ref<FoodDashboardDto[]>([])
  const selectedFood = ref<FoodDto | null>(null)
  const brands = ref<Brand[]>([])
  const unitsOfMeasures = ref<UnitOfMeasureDto[]>([])
  const supermarkets = ref<Supermarket[]>([])
  const categories = ref<Category[]>([])
  const isCreating = ref(false)
  const formMode = ref<FoodFormMode>('create')

  const updateDashboardCache = (items: FoodDashboardDto[]) => {
    dashboardCache = [...items]
    dashboardCacheAt = Date.now()
  }

  const updateLookupsCache = (newBrands: Brand[], newUnits: UnitOfMeasureDto[], newSupermarkets: Supermarket[], newCategories: Category[]) => {
    brandsCache = [...newBrands]
    unitsCache = [...newUnits]
    supermarketsCache = [...newSupermarkets]
    categoriesCache = [...newCategories]
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
    const hasValidCache = !force && brandsCache && unitsCache && supermarketsCache && categoriesCache && Date.now() - lookupsCacheAt < cacheTtlMs
    if (hasValidCache && brandsCache && unitsCache && supermarketsCache && categoriesCache) {
      brands.value = [...brandsCache]
      unitsOfMeasures.value = [...unitsCache]
      supermarkets.value = [...supermarketsCache]
      categories.value = [...categoriesCache]
      return
    }

    const [brandsData, unitsData, supermarketsData, categoriesData] = await Promise.all([
      run(() => getBrands()),
      run(() => getUnitsOfMeasures()),
      run(() => getSupermarkets()),
      run(() => getCategories())
    ])

    if (brandsData) brands.value = brandsData
    if (unitsData) unitsOfMeasures.value = unitsData
    if (supermarketsData) supermarkets.value = supermarketsData
    if (categoriesData) categories.value = categoriesData

    if (brandsData && unitsData && supermarketsData && categoriesData) {
      updateLookupsCache(brandsData, unitsData, supermarketsData, categoriesData)
    }
  }

  const startCreateFood = async () => {
    formMode.value = 'create'

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

  const startEditFood = async (foodId: string) => {
    formMode.value = 'edit'

    const result = await run(async () => {
      if (!brands.value.length || !unitsOfMeasures.value.length) {
        await loadLookups()
      }

      return getFoodById(foodId)
    })

    if (result) {
      selectedFood.value = result
      isCreating.value = true
    }
  }

  const completeCreateFood = async (updatedFood: FoodDto) => {
    if (formMode.value === 'edit') {
      const result = await run(async () => {
        await updateFood(updatedFood)
        return getFoodDashboardRow(updatedFood.id)
      })

      if (result) {
        const index = dashboard.value.findIndex((food) => food.id === result.id)
        if (index >= 0) {
          dashboard.value[index] = result
        }

        updateDashboardCache(dashboard.value)
        isCreating.value = false
        selectedFood.value = null
      }

      return
    }

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

  const addBrandLookup = (brand: Brand) => {
    brands.value.push(brand)
    if (brandsCache) brandsCache.push(brand)
  }

  const addUnitLookup = (unit: UnitOfMeasureDto) => {
    unitsOfMeasures.value.push(unit)
    if (unitsCache) unitsCache.push(unit)
  }

  const addSupermarketLookup = (supermarket: Supermarket) => {
    supermarkets.value.push(supermarket)
    if (supermarketsCache) supermarketsCache.push(supermarket)
  }

  const addCategoryLookup = (category: Category) => {
    categories.value.push(category)
    if (categoriesCache) categoriesCache.push(category)
  }

  const removeFood = async (food: FoodDashboardDto) => {
    const removed = await run(async () => {
      await deleteFood(food.id)
      return food.id
    })

    if (removed) {
      dashboard.value = dashboard.value.filter((item) => item.id !== removed)
      updateDashboardCache(dashboard.value)
    }
  }

  return {
    dashboard,
    selectedFood,
    brands,
    unitsOfMeasures,
    supermarkets,
    categories,
    isCreating,
    formMode,
    isLoading,
    errorMessage,
    loadDashboard,
    loadLookups,
    startCreateFood,
    startEditFood,
    completeCreateFood,
    cancelCreateFood,
    removeFood,
    addBrandLookup,
    addUnitLookup,
    addSupermarketLookup,
    addCategoryLookup
  }
}
