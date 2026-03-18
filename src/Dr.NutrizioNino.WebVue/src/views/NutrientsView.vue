<script setup lang="ts">
import { onMounted } from 'vue'
import { NAlert, NButton, NCard, NSpace, useDialog, useMessage } from 'naive-ui'
import NutrientsList from '../components/Nutrients/NutrientsList.vue'
import NutrientDetail from '../components/Nutrients/NutrientDetail.vue'
import { useNutrients } from '@/modules/nutrients/composables/useNutrients'
import type { Nutrient } from '@/Interfaces/Nutrients/Nutrient'

const {
  nutrients,
  unitsOfMeasures,
  isEditing,
  formMode,
  selectedNutrient,
  isLoading,
  errorMessage,
  loadNutrients,
  startCreateNutrient,
  startEditNutrient,
  cancelEdit,
  submitNutrient,
  removeNutrient
} = useNutrients()

const message = useMessage()
const dialog = useDialog()

const handleDeleteNutrient = (nutrient: Nutrient) => {
  dialog.warning({
    title: 'Conferma eliminazione',
    content: `Eliminare il nutriente "${nutrient.name}"?`,
    positiveText: 'Elimina',
    negativeText: 'Annulla',
    onPositiveClick: () => removeNutrient(nutrient)
  })
}

onMounted(async () => {
  await loadNutrients()
})
</script>

<template>
  <n-space vertical size="large" class="nutrients-page">
    <n-card title="Nutrienti" size="large">
      <template #header-extra>
        <n-button
          v-if="!isEditing"
          type="primary"
          @click="startCreateNutrient"
          :loading="isLoading"
        >
          Nuovo nutriente
        </n-button>
      </template>

      <n-space vertical size="medium">
        <n-alert v-if="errorMessage" type="error" :show-icon="true" :bordered="false">
          {{ errorMessage }}
        </n-alert>

        <NutrientsList
          v-if="!isEditing"
          :nutrients="nutrients"
          @edit="startEditNutrient"
          @delete="handleDeleteNutrient"
        />

        <NutrientDetail
          v-else
          :mode="formMode"
          :nutrient="selectedNutrient"
          :units-of-measures="unitsOfMeasures"
          :is-submitting="isLoading"
          @save="submitNutrient"
          @cancel="cancelEdit"
        />
      </n-space>
    </n-card>
  </n-space>
</template>

<style scoped>
.nutrients-page {
  width: 100%;
}
</style>
