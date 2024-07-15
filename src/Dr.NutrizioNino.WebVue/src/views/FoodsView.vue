<script setup lang="ts">
import { ref } from 'vue'
import foodList from '../components/Foods/FoodsList.vue'
import foodDetail from '../components/Foods/FoodDetail.vue'
import { FoodsService } from '@/Infrastructure/FoodsServices'
import type { ApiResponseDto } from '@/Interfaces/ApiResponseDto'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'

const isNew = ref(false)
const dashboard = ref<ApiResponseDto<FoodDashboardDto>>()

const foodsService = new FoodsService()
//carica i dati da FoodsService.GetDashboard
foodsService.GetDashboard().then((response) => {
  dashboard.value = response
})
</script>

<template>
  <div class="foods">
    <h1>Alimenti</h1>
    <br />
    <button @click="isNew = !isNew" v-if="!isNew">Nuovo</button>
    <br v-if="!isNew" />
    <foodList v-if="!isNew" :foods="dashboard?.data || []"></foodList>
    <!-- <foodDetail v-if="isNew" @cancel="isNew = false" @complete="completeHandler" :food="food"></foodDetail> -->
    <!-- {{ dashboard?.data }} -->
  </div>
</template>
