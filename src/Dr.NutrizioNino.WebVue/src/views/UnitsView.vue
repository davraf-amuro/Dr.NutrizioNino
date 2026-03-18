<script setup lang="ts">
import { onMounted } from 'vue'
import { NAlert, NButton, NCard, NSpace, useDialog } from 'naive-ui'
import UnitsList from '../components/Units/UnitsList.vue'
import UnitDetail from '../components/Units/UnitDetail.vue'
import { useUnits } from '@/modules/units/composables/useUnits'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'

const {
  units,
  isEditing,
  formMode,
  selectedUnit,
  isLoading,
  errorMessage,
  loadUnits,
  startCreateUnit,
  startEditUnit,
  cancelEdit,
  submitUnit,
  removeUnit
} = useUnits()

const dialog = useDialog()

const handleDeleteUnit = (unit: UnitOfMeasureDto) => {
  dialog.warning({
    title: 'Conferma eliminazione',
    content: `Eliminare l'unità di misura "${unit.name} (${unit.abbreviation})"?`,
    positiveText: 'Elimina',
    negativeText: 'Annulla',
    onPositiveClick: () => removeUnit(unit)
  })
}

onMounted(async () => {
  await loadUnits()
})
</script>

<template>
  <n-space vertical size="large" class="units-page">
    <n-card title="Unità di misura" size="large">
      <template #header-extra>
        <n-button
          v-if="!isEditing"
          type="primary"
          @click="startCreateUnit"
          :loading="isLoading"
        >
          Nuova unità di misura
        </n-button>
      </template>

      <n-space vertical size="medium">
        <n-alert v-if="errorMessage" type="error" :show-icon="true" :bordered="false">
          {{ errorMessage }}
        </n-alert>

        <UnitsList
          v-if="!isEditing"
          :units="units"
          @edit="startEditUnit"
          @delete="handleDeleteUnit"
        />

        <UnitDetail
          v-else
          :mode="formMode"
          :unit="selectedUnit"
          :is-submitting="isLoading"
          @save="submitUnit"
          @cancel="cancelEdit"
        />
      </n-space>
    </n-card>
  </n-space>
</template>

<style scoped>
.units-page {
  width: 100%;
}
</style>
