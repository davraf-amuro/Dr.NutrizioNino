<template>
  <n-data-table :columns="columns" :data="props.foods" />
</template>

<script setup lang="ts">
import { h } from 'vue'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import { NButton, NDataTable, NSpace, type DataTableColumns } from 'naive-ui'

const props = defineProps<{
  foods: FoodDashboardDto[]
}>()

const emit = defineEmits<{
  edit: [food: FoodDashboardDto]
  delete: [food: FoodDashboardDto]
}>()

const columns: DataTableColumns<FoodDashboardDto> = [
  { title: 'Nome', key: 'name' },
  { title: 'Kcal', key: 'calorie', width: 80 },
  { title: 'Quantità', key: 'quantity', width: 100 },
  { title: 'UdM', key: 'abbreviation', width: 80 },
  { title: 'Marca', key: 'brandDescription' },
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
