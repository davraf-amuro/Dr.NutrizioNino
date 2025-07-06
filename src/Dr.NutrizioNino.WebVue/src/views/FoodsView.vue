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
  dashboard.value = response
})

foodsService.FoodFactoryGetNew().then((response) => {
  fullFood.value = response
})

// Definisci una nuova funzione per creare un nuovo cibo
function createNewFood() {
  foodsService.FoodFactoryGetNew().then((response) => {
    fullFood.value = response
    isNew.value = true // Assicurati che isNew sia reattivo se necessario
  })
}

async function completeHandler() {
  try {
    // Implementa la logica per gestire il completamento
    //TODO: manda l'oggetto fullFood.value.data al db
    //TODO: fatti restituire l'id del nuovo oggetto
    //TODO: api per recuperare l'oggetto row da aggiungere alla dashboard
    var row = await foodsService.GetDashboardRow('37a5b00b-b6ed-45a4-92c5-779803498ed6')
    dashboard.value.data.push(row.data) // Access the 'data' property of 'row'

    isNew.value = false
  } catch (error) {
    console.error('Errore durante il recupero della riga della dashboard:', error)
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
