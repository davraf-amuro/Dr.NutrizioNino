<template>
  <n-space vertical size="medium">
    <n-h3 style="margin: 0">{{ recipe.name }}</n-h3>

    <n-divider title-placement="left">Ingredienti</n-divider>

    <n-data-table
      :columns="ingredientColumns"
      :data="recipe.ingredients"
      :row-key="(row) => row.foodId"
      size="small"
      :single-line="false"
      :bordered="true"
      :pagination="false"
      :summary="() => ({ foodName: { value: 'Totale', colSpan: 1 }, quantityGrams: { value: recipe.weightGrams + ' g' } })"
      aria-label="Ingredienti della ricetta"
    />

    <n-divider title-placement="left">Nutrienti</n-divider>

    <RecipeNutritionPreview
      :nutrients="recipe.nutrients"
      title="Valori totali della ricetta"
    />

    <n-space justify="end" style="margin-top: 8px">
      <n-button @click="emit('close')">Chiudi</n-button>
    </n-space>
  </n-space>
</template>

<script setup lang="ts">
import { NButton, NDivider, NDataTable, NH3, NSpace, type DataTableColumns } from 'naive-ui'
import type { RecipeDetailDto, RecipeDetailIngredientDto } from '@/Interfaces/recipes/RecipeDetailDto'
import RecipeNutritionPreview from './RecipeNutritionPreview.vue'

defineProps<{ recipe: RecipeDetailDto }>()
const emit = defineEmits<{ close: [] }>()

const ingredientColumns: DataTableColumns<RecipeDetailIngredientDto> = [
  { title: 'Alimento', key: 'foodName' },
  { title: 'Quantità (g)', key: 'quantityGrams', width: 130 }
]
</script>
