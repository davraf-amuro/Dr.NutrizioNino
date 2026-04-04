<template>
  <n-space vertical size="medium">
    <n-h3 style="margin: 0">Nuovo piatto</n-h3>

    <n-spin :show="isLoading">
      <n-form :model="form" label-placement="left" label-width="120" label-align="right">
        <n-form-item label="Nome piatto" path="name" :rule="{ required: true, min: 3, message: 'Nome obbligatorio (min 3 caratteri)', trigger: 'blur' }">
          <n-input v-model:value="form.name" :maxlength="50" placeholder="Inserisci il nome del piatto" />
        </n-form-item>
      </n-form>

      <n-divider title-placement="left">Ingredienti</n-divider>

      <DishIngredientList
        :ingredients="ingredients"
        :available-foods="availableFoods"
        :is-loading="isLoading"
        @add-food="onAddFood"
        @update-quantity="onUpdateQuantity"
        @remove="onRemoveIngredient"
      />

      <n-divider title-placement="left">Anteprima nutrienti</n-divider>

      <DishNutritionPreview :nutrients="nutrients" title="Valori totali (anteprima)" />
    </n-spin>

    <n-space justify="space-between" style="margin-top: 8px">
      <n-button type="error" :disabled="isLoading" @click="emit('cancel')">Annulla</n-button>
      <n-button
        type="primary"
        :loading="isLoading"
        :disabled="!canSave"
        @click="handleSave"
      >Salva piatto</n-button>
    </n-space>
  </n-space>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import {
  NButton, NDivider, NForm, NFormItem, NH3, NInput, NSpace, NSpin
} from 'naive-ui'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'
import { getFoodById } from '@/modules/foods/api/foods.api'
import { useDishCalculator, type DishIngredient } from '@/core/composables/useDishCalculator'
import DishIngredientList from './DishIngredientList.vue'
import DishNutritionPreview from './DishNutritionPreview.vue'
import type { CreateDishRequest } from '@/modules/dishes/api/dishes.api'

const props = defineProps<{
  availableFoods: FoodDashboardDto[]
  isLoading?: boolean
}>()

const emit = defineEmits<{
  cancel: []
  save: [dto: CreateDishRequest]
}>()

const form = ref({ name: '' })
const ingredients = ref<DishIngredient[]>([])
const foodCache = new Map<string, FoodDto>()

const { nutrients } = useDishCalculator(ingredients)

const canSave = computed(
  () => form.value.name.trim().length >= 3 && ingredients.value.length > 0
)

const onAddFood = async (foodId: string) => {
  if (ingredients.value.some((i) => i.food.id === foodId)) return
  let food = foodCache.get(foodId)
  if (!food) {
    food = await getFoodById(foodId)
    foodCache.set(foodId, food)
  }
  ingredients.value.push({ food, quantityGrams: 100 })
}

const onUpdateQuantity = (foodId: string, quantity: number) => {
  const item = ingredients.value.find((i) => i.food.id === foodId)
  if (item) item.quantityGrams = quantity
}

const onRemoveIngredient = (foodId: string) => {
  ingredients.value = ingredients.value.filter((i) => i.food.id !== foodId)
}

const handleSave = () => {
  emit('save', {
    name: form.value.name.trim(),
    ingredients: ingredients.value.map((i) => ({
      foodId: i.food.id,
      quantityGrams: i.quantityGrams
    }))
  })
}
</script>
