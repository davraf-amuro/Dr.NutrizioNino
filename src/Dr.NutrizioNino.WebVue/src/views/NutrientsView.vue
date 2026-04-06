<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { NAlert, NButton, NCard, NSpace, useDialog, useMessage } from 'naive-ui'
import NutrientsList from '../components/Nutrients/NutrientsList.vue'
import NutrientDetail from '../components/Nutrients/NutrientDetail.vue'
import NutrientsReorderModal from '../components/Nutrients/NutrientsReorderModal.vue'
import { useNutrients } from '@/modules/nutrients/composables/useNutrients'
import type { Nutrient } from '@/Interfaces/Nutrients/Nutrient'
import { reorderNutrients, type NutrientReorderItem } from '@/modules/nutrients/api/nutrients.api'

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
const showReorderModal = ref(false)

const handleDeleteNutrient = (nutrient: Nutrient) => {
  dialog.warning({
    title: 'Conferma eliminazione',
    content: `Eliminare il nutriente "${nutrient.name}"?`,
    positiveText: 'Elimina',
    negativeText: 'Annulla',
    onPositiveClick: () => removeNutrient(nutrient)
  })
}

const handleReorderConfirm = async (items: NutrientReorderItem[]) => {
  try {
    await reorderNutrients(items)
    message.success('Ordine aggiornato')
    await loadNutrients()
  } catch {
    message.error('Errore durante il salvataggio dell\'ordine')
  }
}

onMounted(async () => {
  await loadNutrients()
})
</script>

<template>
  <n-space vertical size="large" class="nutrients-page">
    <n-card title="Nutrienti" size="large">
      <template #header-extra>
        <n-space size="small">
          <n-button
            v-if="!isEditing"
            @click="showReorderModal = true"
          >
            Ordina
          </n-button>
          <n-button
            v-if="!isEditing"
            type="primary"
            @click="startCreateNutrient"
            :loading="isLoading"
          >
            Nuovo nutriente
          </n-button>
        </n-space>
      </template>

      <n-space vertical size="medium">
        <n-alert v-if="errorMessage" type="error" :show-icon="true" :bordered="false">
          {{ errorMessage }}
        </n-alert>

        <NutrientsList
          v-if="!isEditing"
          :nutrients="nutrients"
          :units-of-measures="unitsOfMeasures"
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

  <NutrientsReorderModal
    v-model:show="showReorderModal"
    :nutrients="nutrients"
    @confirm="handleReorderConfirm"
  />
</template>

<style scoped>
.nutrients-page {
  width: 100%;
}
</style>
