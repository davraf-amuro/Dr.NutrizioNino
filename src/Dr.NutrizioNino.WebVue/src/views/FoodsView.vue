<script setup lang="ts">
import { onMounted } from 'vue'
import { NAlert, NButton, NCard, NSpace } from 'naive-ui'
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
      </n-space>
    </n-card>
  </n-space>
</template>

<style scoped>
.foods-page {
  width: 100%;
}
</style>
