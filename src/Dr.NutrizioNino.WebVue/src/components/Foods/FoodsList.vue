<template>
  <div>
    <h3>Elenco degli Alimenti</h3>
    <br />
    <n-data-table :columns="columns" :data="props.foods"></n-data-table>
  </div>
</template>

<script setup lang="ts">
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import { NDataTable, type DataTableColumns } from 'naive-ui'

const columns: DataTableColumns<FoodDashboardDto> = [
  { title: 'Nome', key: 'name' },
  { title: 'Kcal', key: 'calorie' },
  { title: 'Quantità', key: 'quantity' },
  { title: 'UdM', key: 'abbreviation' },
  { title: 'Marca', key: 'brandDescription' }
]

const props = defineProps<{
  foods: FoodDashboardDto[]
}>()
</script>

<style scoped></style>
