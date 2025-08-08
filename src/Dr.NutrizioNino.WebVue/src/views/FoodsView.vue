<script setup lang="ts">
import { ref } from 'vue'
import foodList from '../components/Foods/FoodsList.vue'
import foodDetail from '../components/Foods/FoodDetail.vue'
import { FoodsService } from '@/Infrastructure/FoodsServices'
import type { ApiResponseMultipleDto } from '@/Interfaces/ApiResponseMultipleDto'
import type { ApiResponseSingeDto } from '@/Interfaces/ApiResponseSingeDto'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'

const isNew = ref(false)
const dashboard = ref<ApiResponseMultipleDto<FoodDashboardDto>>()
const fullFood = ref<ApiResponseSingeDto<FoodDto>>()

const foodsService = new FoodsService()
//carica i dati da FoodsService.GetDashboard
foodsService.GetDashboard().then((response) => {
  console.log('GetDashboard è stata chiamata')
  dashboard.value = response
})

async function completeHandler() {
  try {
    // Posta il nuovo FoodDto alle API e ottieni l'ID
    const id = await foodsService.PostNewFood(fullFood.value!.data)

    // Recupera la riga della dashboard usando l'ID del FoodDto creato
    const row = await foodsService.GetDashboardRow(id)

    // Aggiungi la riga alla dashboard
    if (dashboard.value) {
      dashboard.value.data?.push(row.data)
    }

    isNew.value = false
  } catch (error) {
    console.error('Errore durante il completamento del nuovo cibo:', error)
  }
}

async function createNewFood() {
  try {
    // Recupera un nuovo FoodDto dal servizio
    const response = await foodsService.FoodFactoryGetNew()
    fullFood.value = response
    isNew.value = true
  } catch (error) {
    console.error('Errore durante la creazione del nuovo cibo:', error)
  }
}
</script>

<template>
  <div class="foods">
    <h1>Alimenti</h1>
    <br />
    <button @click="createNewFood" v-if="!isNew">Nuovo</button>
    <br v-if="!isNew" />
    <foodList v-if="!isNew" :foods="dashboard?.data || []"></foodList>

    <foodDetail
      v-if="isNew"
      @cancel="isNew = false"
      @complete="completeHandler"
      :food="fullFood!.data"
    ></foodDetail>
  </div>
</template>
