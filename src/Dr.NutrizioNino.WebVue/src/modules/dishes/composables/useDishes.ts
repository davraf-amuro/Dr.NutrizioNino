import { ref } from 'vue'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import { useAsyncState } from '@/core/composables/useAsyncState'
import { createDish, deleteDish, getDishesDashboard, type CreateDishRequest } from '@/modules/dishes/api/dishes.api'
import { getFoodsDashboard } from '@/modules/foods/api/foods.api'

const cacheTtlMs = 60_000
let dishesCache: FoodDashboardDto[] | null = null
let dishesCacheAt = 0
let foodsCache: FoodDashboardDto[] | null = null
let foodsCacheAt = 0

export const useDishes = () => {
  const { isLoading, errorMessage, run } = useAsyncState()
  const dishes = ref<FoodDashboardDto[]>([])
  const availableFoods = ref<FoodDashboardDto[]>([])
  const isCreating = ref(false)

  const loadDishes = async (force = false) => {
    const hasValidCache = !force && dishesCache && Date.now() - dishesCacheAt < cacheTtlMs
    if (hasValidCache && dishesCache) {
      dishes.value = [...dishesCache]
      return
    }
    const data = await run(() => getDishesDashboard())
    if (data) {
      dishes.value = data
      dishesCache = [...data]
      dishesCacheAt = Date.now()
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

  const completeDish = async (dto: CreateDishRequest) => {
    const result = await run(() => createDish(dto))
    if (result) {
      isCreating.value = false
      await loadDishes(true)
    }
  }

  const removeDish = async (dish: FoodDashboardDto) => {
    const removed = await run(async () => {
      await deleteDish(dish.id)
      return dish.id
    })
    if (removed) {
      dishes.value = dishes.value.filter((d) => d.id !== removed)
      dishesCache = [...dishes.value]
    }
  }

  return {
    dishes,
    availableFoods,
    isCreating,
    isLoading,
    errorMessage,
    loadDishes,
    loadAvailableFoods,
    startCreate,
    cancelCreate,
    completeDish,
    removeDish
  }
}
