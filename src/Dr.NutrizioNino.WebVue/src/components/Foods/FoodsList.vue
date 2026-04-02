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
      aria-label="Lista alimenti"
    />
  </n-space>
</template>

<script setup lang="ts">
import { h } from 'vue'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import { NButton, NDataTable, NInput, NSpace, NTag, type DataTableColumns } from 'naive-ui'
import { useTableSearch } from '@/core/composables/useTableSearch'

const props = defineProps<{
  foods: FoodDashboardDto[]
}>()

const emit = defineEmits<{
  edit: [food: FoodDashboardDto]
  delete: [food: FoodDashboardDto]
}>()

const { searchQuery, filteredData } = useTableSearch(() => props.foods, 'name')

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
  { title: 'Marca', key: 'brandDescription', sorter: 'default' },
  { title: 'Kcal', key: 'calorie', width: 80, sorter: (a, b) => (a.calorie ?? 0) - (b.calorie ?? 0) },
  { title: 'UdM', key: 'abbreviation', width: 80, sorter: 'default' },
  { title: 'Quantità', key: 'quantity', width: 100, sorter: (a, b) => (a.quantity ?? 0) - (b.quantity ?? 0) },
  { title: 'Barcode', key: 'barcode', sorter: 'default' },
  {
    title: 'Supermercati',
    key: 'supermarketsText',
    render: (row) => {
      if (!row.supermarketsText) return null
      const names = row.supermarketsText.split(', ')
      return h(NSpace, { size: 4, wrap: true }, () =>
        names.map((name) => h(NTag, { size: 'small', type: 'info' }, { default: () => name }))
      )
    }
  },
  {
    title: 'Azioni',
    key: 'actions',
    width: 180,
    render: (row) =>
      h(NSpace, { size: 'small' }, () => [
        h(NButton, { size: 'small', type: 'primary', onClick: () => emit('edit', row) }, { default: () => 'Modifica' }),
        h(NButton, { size: 'small', type: 'error', tertiary: true, onClick: () => emit('delete', row) }, { default: () => 'Elimina' })
      ])
  }
]
</script>
