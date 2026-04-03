<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { NAlert, NButton, NCard, NSpace, useDialog } from 'naive-ui'
import foodList from '../components/Foods/FoodsList.vue'
import foodDetail from '../components/Foods/FoodDetail.vue'
import FoodCompareModal from '../components/Foods/FoodCompareModal.vue'
import { useFoods } from '@/modules/foods/composables/useFoods'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'

const {
  dashboard,
  selectedFood,
  brands,
  unitsOfMeasures,
  supermarkets,
  categories,
  isCreating,
  formMode,
  isLoading,
  errorMessage,
  loadDashboard,
  loadLookups,
  startCreateFood,
  startEditFood,
  completeCreateFood,
  cancelCreateFood,
  removeFood,
  addBrandLookup,
  addUnitLookup,
  addSupermarketLookup,
  addCategoryLookup
} = useFoods()

const dialog = useDialog()

const compareShow = ref(false)
const compareFoods = ref<FoodDashboardDto[]>([])

const handleCompare = (foods: FoodDashboardDto[]) => {
  compareFoods.value = foods
  compareShow.value = true
}

const handleDeleteFood = (food: FoodDashboardDto) => {
  dialog.warning({
    title: 'Conferma eliminazione',
    content: `Eliminare l'alimento "${food.name}"?`,
    positiveText: 'Elimina',
    negativeText: 'Annulla',
    onPositiveClick: () => removeFood(food)
  })
}

onMounted(async () => {
  await Promise.all([loadDashboard(), loadLookups()])
})
</script>

<template>
  <n-space vertical size="large" class="foods-page">
    <n-card title="Alimenti" size="large">
      <template #header-extra>
        <n-button
          v-if="!isCreating"
          type="primary"
          @click="startCreateFood"
          :loading="isLoading"
        >
          Nuovo alimento
        </n-button>
      </template>

      <n-space vertical size="medium">
        <n-alert v-if="errorMessage" type="error" :show-icon="true" :bordered="false">
          {{ errorMessage }}
        </n-alert>

        <foodList
          v-if="!isCreating"
          :foods="dashboard"
          @edit="(food) => startEditFood(food.id)"
          @delete="handleDeleteFood"
          @compare="handleCompare"
        />

        <foodDetail
          v-if="isCreating && selectedFood"
          @cancel="cancelCreateFood"
          @complete="completeCreateFood"
          @brand-created="addBrandLookup"
          @unit-created="addUnitLookup"
          @supermarket-created="addSupermarketLookup"
          @category-created="addCategoryLookup"
          :food="selectedFood"
          :mode="formMode"
          :brands="brands"
          :units-of-measures="unitsOfMeasures"
          :supermarkets="supermarkets"
          :categories="categories"
          :is-submitting="isLoading"
        />
      </n-space>
    </n-card>
  </n-space>

  <FoodCompareModal
    :show="compareShow"
    :foods="compareFoods"
    @close="compareShow = false"
  />
</template>

<style scoped>
.foods-page {
  width: 100%;
}
</style>
