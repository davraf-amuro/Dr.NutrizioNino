<script setup lang="ts">
import { ref } from 'vue'
import foodList from '../components/Foods/FoodsList.vue'
import foodDetail from '../components/Foods/FoodDetail.vue'
import { FoodsService } from '@/Infrastructure/FoodsServices'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'

const isNew = ref(false)
const dashboard = ref<FoodDashboardDto[]>([])
const fullFood = ref<FoodDto>()

const foodsService = new FoodsService()
//carica i dati da FoodsService.GetDashboard
foodsService.GetDashboard().then((response) => {
  console.log('GetDashboard è stata chiamata')
  dashboard.value = response
})

async function completeHandler(updatedFood?: FoodDto) {
  try {
    if (updatedFood) {
      fullFood.value = updatedFood
    }

    if (!fullFood.value) {
      return
    }
    console.log('Dati in fullFood prima di inviare:', fullFood.value)

    const id = await foodsService.PostNewFood(fullFood.value!)

    const row = await foodsService.GetDashboardRow(id)

    dashboard.value.push(row)

    isNew.value = false
  } catch (error) {
    console.error('Errore durante il completamento del nuovo cibo:', error)
  }
}

async function createNewFood() {
  try {
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
    <foodList v-if="!isNew" :foods="dashboard" />

    <foodDetail
      v-if="isNew"
      @cancel="isNew = false"
      @complete="completeHandler"
      :food="fullFood!"
    />
  </div>
</template>
