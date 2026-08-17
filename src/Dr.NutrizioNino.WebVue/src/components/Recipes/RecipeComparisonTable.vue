<template>
  <n-card size="small" title="Confronto nutrienti">
    <n-data-table
      v-if="rows.length > 0"
      :columns="tableColumns"
      :data="rows"
      :row-key="(row: ComparisonRow) => row.nutrientId"
      :aria-label="ariaLabel"
      size="small"
      :single-line="false"
      :pagination="false"
    />
    <n-text v-else depth="3">Scegli almeno due ricette e avvia il confronto.</n-text>
  </n-card>
</template>

<script setup lang="ts">
import { computed, h } from 'vue'
import { NCard, NDataTable, NText, type DataTableColumns } from 'naive-ui'
import type { ComparisonColumn, ComparisonRow } from '@/modules/recipes/composables/useRecipeComparison'

const props = defineProps<{
  columns: ComparisonColumn[]
  rows: ComparisonRow[]
}>()

const nutrientColumnWidth = 220

const ariaLabel = computed(
  () => `Confronto nutrienti tra ${props.columns.map((column) => column.name).join(', ')}`
)

/** Valore più alto della riga: null quando la riga non ha valori confrontabili o sono tutti pari. */
const rowMax = (row: ComparisonRow): number | null => {
  const values = Object.values(row.values).filter((value): value is number => value !== null)
  if (values.length < 2) return null

  const max = Math.max(...values)
  const isTie = values.every((value) => value === max)
  return isTie ? null : max
}

/** Cella vuota se il nutriente non è presente nella ricetta; il massimo è marcato anche testualmente. */
const renderCell = (row: ComparisonRow, recipeId: string) => {
  const value = row.values[recipeId]
  if (value === null || value === undefined) {
    return h(NText, { depth: 3, ariaLabel: 'Nutriente non presente' }, () => '—')
  }

  const isMax = value === rowMax(row)
  return h(
    NText,
    { type: isMax ? 'success' : undefined, strong: isMax },
    () => (isMax ? `${value} ▲ max` : `${value}`)
  )
}

const tableColumns = computed<DataTableColumns<ComparisonRow>>(() => [
  {
    title: 'Nutriente',
    key: 'name',
    width: nutrientColumnWidth
  },
  ...props.columns.map((column) => ({
    title: `${column.name} (${column.quantityGrams} g)`,
    key: column.recipeId,
    render: (row: ComparisonRow) => renderCell(row, column.recipeId)
  }))
])
</script>
