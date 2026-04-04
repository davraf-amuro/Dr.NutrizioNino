<template>
  <n-space vertical>
    <n-input
      v-model:value="searchQuery"
      placeholder="Cerca per nome..."
      clearable
      aria-label="Cerca per nome"
    />
    <n-data-table
      :columns="columns"
      :data="filteredData"
      :row-key="(row: FoodDashboardDto) => row.id"
      :single-line="false"
      :bordered="true"
      :pagination="false"
      aria-label="Lista piatti"
    />
  </n-space>
</template>

<script setup lang="ts">
import { h } from 'vue'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import { NButton, NDataTable, NInput, NSpace, NTag, type DataTableColumns } from 'naive-ui'
import { useTableSearch } from '@/core/composables/useTableSearch'

const props = defineProps<{ dishes: FoodDashboardDto[] }>()
const emit = defineEmits<{
  delete: [dish: FoodDashboardDto]
  detail: [dish: FoodDashboardDto]
}>()

const { searchQuery, filteredData } = useTableSearch(() => props.dishes, 'name')

const columns: DataTableColumns<FoodDashboardDto> = [
  {
    title: 'Prop.',
    key: 'isOwner',
    width: 60,
    render: (row) => row.isOwner
      ? h(NTag, { size: 'small', type: 'success', round: true }, { default: () => '★' })
      : h('span', { style: 'color:#ccc' }, '—')
  },
  { title: 'Nome', key: 'name', sorter: 'default' },
  { title: 'Kcal', key: 'calorie', width: 90, sorter: (a, b) => (a.calorie ?? 0) - (b.calorie ?? 0) },
  { title: 'Qta', key: 'quantity', width: 90, render: (row) => `${row.quantity} ${row.abbreviation ?? 'g'}` },
  {
    title: 'Azioni',
    key: 'actions',
    width: 180,
    render: (row) =>
      h(NSpace, { size: 'small' }, {
        default: () => [
          h(NButton, { size: 'small', secondary: true, onClick: () => emit('detail', row) }, { default: () => 'Dettaglio' }),
          h(NButton, { size: 'small', type: 'error', tertiary: true, onClick: () => emit('delete', row) }, { default: () => 'Elimina' })
        ]
      })
  }
]
</script>
