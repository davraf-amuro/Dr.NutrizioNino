<template>
  <n-space vertical size="medium">
    <n-h3 style="margin: 0">{{ dish.name }}</n-h3>

    <n-divider title-placement="left">Ingredienti</n-divider>

    <n-data-table
      :columns="ingredientColumns"
      :data="dish.ingredients"
      :row-key="(row) => row.foodId"
      size="small"
      :single-line="false"
      :bordered="true"
      :pagination="false"
      aria-label="Ingredienti del piatto"
    />
    <n-text depth="3" style="font-size: 13px">
      Peso totale: {{ dish.weightGrams }}g
    </n-text>

    <n-divider title-placement="left">Nutrienti</n-divider>

    <DishNutritionPreview
      :nutrients="dish.nutrients"
      :calorie="dish.calorie"
      title="Valori totali del piatto"
    />

    <n-space justify="end" style="margin-top: 8px">
      <n-button @click="emit('close')">Chiudi</n-button>
    </n-space>
  </n-space>
</template>

<script setup lang="ts">
import { NButton, NDivider, NDataTable, NH3, NSpace, NText, type DataTableColumns } from 'naive-ui'
import type { DishDetailDto, DishDetailIngredientDto } from '@/Interfaces/dishes/DishDetailDto'
import DishNutritionPreview from './DishNutritionPreview.vue'

defineProps<{ dish: DishDetailDto }>()
const emit = defineEmits<{ close: [] }>()

const ingredientColumns: DataTableColumns<DishDetailIngredientDto> = [
  { title: 'Alimento', key: 'foodName' },
  { title: 'Quantità (g)', key: 'quantityGrams', width: 130 }
]
</script>
