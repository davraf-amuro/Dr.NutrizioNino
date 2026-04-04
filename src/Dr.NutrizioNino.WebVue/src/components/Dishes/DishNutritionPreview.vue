<template>
  <n-card size="small" :title="title">
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
import { NCard, NDataTable, NText, type DataTableColumns } from 'naive-ui'
import type { CalculatedNutrient } from '@/core/composables/useDishCalculator'

withDefaults(defineProps<{
  nutrients: CalculatedNutrient[]
  title?: string
}>(), {
  title: 'Valori nutrizionali'
})

const columns: DataTableColumns<CalculatedNutrient> = [
  { title: 'Nutriente', key: 'name' },
  { title: 'Quantità', key: 'quantity', width: 100 }
]
</script>
