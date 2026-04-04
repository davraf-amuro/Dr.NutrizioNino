import { computed, type Ref } from 'vue'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'
import { sortNutrients } from '@/core/utils/sortNutrients'

export interface DishIngredient {
  food: FoodDto
  quantityGrams: number
}

export interface CalculatedNutrient {
  nutrientId: string
  name: string
  positionOrder: number
  unitOfMeasureId: string
  quantity: number
}

export const useDishCalculator = (ingredients: Ref<DishIngredient[]>) => {
  const totalWeight = computed(() => ingredients.value.reduce((sum, i) => sum + i.quantityGrams, 0))

  const nutrients = computed<CalculatedNutrient[]>(() => {
    if (totalWeight.value === 0) return []

    const contributions = new Map<string, CalculatedNutrient & { total: number }>()

    for (const { food, quantityGrams } of ingredients.value) {
      for (const n of food.nutrients) {
        const key = `${n.nutrientId}__${n.unitOfMeasureId}`
        const contribution = n.quantity * (quantityGrams / 100)
        const existing = contributions.get(key)
        if (existing) {
          existing.total += contribution
        } else {
          contributions.set(key, {
            nutrientId: n.nutrientId,
            name: n.name,
            positionOrder: n.positionOrder ?? 0,
            unitOfMeasureId: n.unitOfMeasureId,
            quantity: 0,
            total: contribution
          })
        }
      }
    }

    return sortNutrients(
      Array.from(contributions.values()).map((v) => ({
        nutrientId: v.nutrientId,
        name: v.name,
        positionOrder: v.positionOrder,
        unitOfMeasureId: v.unitOfMeasureId,
        quantity: Math.round(v.total * 100) / 100
      }))
    )
  })

  return { nutrients, totalWeight }
}
