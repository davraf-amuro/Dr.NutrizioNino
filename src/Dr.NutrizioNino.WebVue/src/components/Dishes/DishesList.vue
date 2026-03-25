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
import { NButton, NDataTable, NInput, NSpace, type DataTableColumns } from 'naive-ui'
import { useTableSearch } from '@/core/composables/useTableSearch'

const props = defineProps<{ dishes: FoodDashboardDto[] }>()
const emit = defineEmits<{ delete: [dish: FoodDashboardDto] }>()

const { searchQuery, filteredData } = useTableSearch(() => props.dishes, 'name')

const columns: DataTableColumns<FoodDashboardDto> = [
  { title: 'Nome', key: 'name', sorter: 'default' },
  { title: 'Kcal/100g', key: 'calorie', width: 110, sorter: (a, b) => (a.calorie ?? 0) - (b.calorie ?? 0) },
  {
    title: 'Azioni',
    key: 'actions',
    width: 90,
    render: (row) =>
      h(
        NButton,
        { size: 'small', type: 'error', onClick: () => emit('delete', row) },
        { default: () => 'Elimina' }
      )
  }
]
</script>
