<template>
  <n-space vertical size="large" class="dishes-page">
    <n-card title="Piatti" size="large">
      <template #header-extra>
        <n-button v-if="!isCreating" type="primary" :loading="isLoading" @click="startCreate">
          Nuovo piatto
        </n-button>
      </template>

      <n-space vertical size="medium">
        <n-alert v-if="errorMessage" type="error" :show-icon="true" :bordered="false">
          {{ errorMessage }}
        </n-alert>

        <DishesList
          v-if="!isCreating"
          :dishes="dishes"
          @delete="handleDeleteDish"
        />

        <DishBuilder
          v-if="isCreating"
          :available-foods="availableFoods"
          :is-loading="isLoading"
          @cancel="cancelCreate"
          @save="completeDish"
        />
      </n-space>
    </n-card>
  </n-space>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { NAlert, NButton, NCard, NSpace, useDialog } from 'naive-ui'
import DishesList from '@/components/Dishes/DishesList.vue'
import DishBuilder from '@/components/Dishes/DishBuilder.vue'
import { useDishes } from '@/modules/dishes/composables/useDishes'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'

const {
  dishes,
  availableFoods,
  isCreating,
  isLoading,
  errorMessage,
  loadDishes,
  loadAvailableFoods,
  startCreate,
  cancelCreate,
  completeDish,
  removeDish
} = useDishes()

const dialog = useDialog()

const handleDeleteDish = (dish: FoodDashboardDto) => {
  dialog.warning({
    title: 'Conferma eliminazione',
    content: `Eliminare il piatto "${dish.name}"?`,
    positiveText: 'Elimina',
    negativeText: 'Annulla',
    onPositiveClick: () => removeDish(dish)
  })
}

onMounted(async () => {
  await Promise.all([loadDishes(), loadAvailableFoods()])
})
</script>

<style scoped>
.dishes-page {
  width: 100%;
}
</style>
