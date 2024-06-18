<template>
  <div>
    <h3>Elenco degli Alimenti</h3>
    <br />
    <n-data-table :columns="columns" :data="dashboard?.data"></n-data-table>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'
import type { FoodDashboardDto } from '@/Interfaces/FoodDashboardDto'
import type { ApiResponseDto } from '@/Interfaces/ApiResponseDto'
import { NDataTable, type DataTableColumns } from 'naive-ui'

const columns: DataTableColumns = [
  { title: 'Nome', key: 'name' },
  { title: 'Kcal', key: 'calorie' },
  { title: 'Quantità', key: 'quantity' },
  { title: 'UdM', key: 'abbreviation' },
  { title: 'Marca', key: 'brandDescription' }
]

const dashboard = ref<ApiResponseDto<FoodDashboardDto>>()
axios.get('https://localhost:44360/foods/dashboard').then(function (response) {
  dashboard.value = response.data
})
</script>

<style scoped></style>
