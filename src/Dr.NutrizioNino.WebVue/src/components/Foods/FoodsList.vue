<template>
  <div>
    <h3>Elenco degli Alimenti</h3>
    <br />
    <n-data-table :columns="columns" :data="props.foods"></n-data-table>
  </div>
</template>

<script setup lang="ts">
import { h } from 'vue'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import { NDataTable, type DataTableColumns, NButton } from 'naive-ui'

const emit = defineEmits<{
  select: [food: FoodDashboardDto]
}>()

const columns: DataTableColumns<FoodDashboardDto> = [
  { title: 'Nome', key: 'name' },
  { title: 'Kcal', key: 'calorie' },
  { title: 'Quantità', key: 'quantity' },
  { title: 'UdM', key: 'abbreviation' },
  { title: 'Marca', key: 'brandDescription' },
  {
    title: '',
    key: 'actions',
    render(row) {
      return h(
        NButton,
        {
          strong: true,
          tertiary: true,
          size: 'small',
          onClick: () => emit('select', row)
        },
        { default: () => 'Dettaglio' }
      )
    }
  }
]

const props = defineProps<{
  foods: FoodDashboardDto[]
}>()
</script>

<style scoped></style>
