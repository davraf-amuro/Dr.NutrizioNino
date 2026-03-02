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
import { NButton, NDataTable, NSpace, type DataTableColumns } from 'naive-ui'

const emit = defineEmits<{
  edit: [food: FoodDashboardDto]
}>()

const columns: DataTableColumns<FoodDashboardDto> = [
  { title: 'Nome', key: 'name' },
  { title: 'Kcal', key: 'calorie' },
  { title: 'Quantità', key: 'quantity' },
  { title: 'UdM', key: 'abbreviation' },
  { title: 'Marca', key: 'brandDescription' },
  {
    title: 'Azioni',
    key: 'actions',
    width: 160,
    render: (row) =>
      h(NSpace, { size: 'small' }, () => [
        h(
          NButton,
          {
            size: 'small',
            type: 'primary',
            onClick: () => emit('edit', row)
          },
          { default: () => 'Modifica' }
        )
      ])
  }
]

const props = defineProps<{
  foods: FoodDashboardDto[]
}>()
</script>

<style scoped></style>
