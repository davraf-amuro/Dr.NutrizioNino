<template>
  <n-card size="small" title="Valori per 100g (anteprima)">
    <n-space :size="4" align="center" style="margin-bottom: 8px">
      <n-text strong>Kcal: {{ calorie }}</n-text>
    </n-space>
    <n-data-table
      v-if="nutrients.length > 0"
      :columns="columns"
      :data="nutrients"
      :row-key="(row) => row.nutrientId"
      size="small"
      :single-line="false"
      :pagination="false"
    />
    <n-text v-else depth="3">Aggiungi ingredienti per vedere l'anteprima.</n-text>
  </n-card>
</template>

<script setup lang="ts">
import { NCard, NDataTable, NSpace, NText, type DataTableColumns } from 'naive-ui'
import type { CalculatedNutrient } from '@/core/composables/useDishCalculator'

defineProps<{
  nutrients: CalculatedNutrient[]
  calorie: number
}>()

const columns: DataTableColumns<CalculatedNutrient> = [
  { title: 'Nutriente', key: 'name' },
  { title: 'Quantità', key: 'quantity', width: 100 }
]
</script>
