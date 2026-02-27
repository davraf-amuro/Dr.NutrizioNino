<script setup lang="ts">
import { onMounted } from 'vue'
import foodList from '../components/Foods/FoodsList.vue'
import foodDetail from '../components/Foods/FoodDetail.vue'
import { useFoods } from '@/modules/foods/composables/useFoods'

const {
  dashboard,
  selectedFood,
  brands,
  unitsOfMeasures,
  isCreating,
  isLoading,
  errorMessage,
  loadDashboard,
  loadLookups,
  startCreateFood,
  completeCreateFood,
  cancelCreateFood
} = useFoods()

onMounted(async () => {
  await Promise.all([loadDashboard(), loadLookups()])
})
</script>

<template>
  <div class="foods">
    <h1>Alimenti</h1>
    <p v-if="errorMessage">{{ errorMessage }}</p>
    <br />
    <button @click="startCreateFood" v-if="!isCreating" :disabled="isLoading">Nuovo</button>
    <br v-if="!isCreating" />
    <foodList v-if="!isCreating" :foods="dashboard" />

    <foodDetail
      v-if="isCreating && selectedFood"
      @cancel="cancelCreateFood"
      @complete="completeCreateFood"
      :food="selectedFood"
      :brands="brands"
      :units-of-measures="unitsOfMeasures"
      :is-submitting="isLoading"
    />
  </div>
</template>
