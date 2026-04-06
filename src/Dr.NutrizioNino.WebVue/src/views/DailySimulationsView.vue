<template>
  <n-space vertical size="large">
    <n-card title="Simulazioni Piano Giornaliero" size="large">
      <template #header-extra>
        <n-button
          v-if="!currentDetail && !showCreate"
          type="primary"
          :loading="isLoading"
          @click="showCreate = true"
        >
          Nuova simulazione
        </n-button>
      </template>

      <n-space vertical size="medium">
        <n-alert v-if="errorMessage" type="error" :show-icon="true" :bordered="false">
          {{ errorMessage }}
        </n-alert>

        <!-- Form creazione -->
        <n-space v-if="showCreate" vertical size="small">
          <n-form-item label="Nome">
            <n-input
              v-model:value="newName"
              placeholder="Es. Lunedì palestra"
              :maxlength="100"
              @keyup.enter="handleCreate"
            />
          </n-form-item>
          <n-space size="small">
            <n-button type="primary" :loading="isLoading" :disabled="!newName.trim()" @click="handleCreate">Crea</n-button>
            <n-button @click="showCreate = false; newName = ''">Annulla</n-button>
          </n-space>
        </n-space>

        <!-- Lista simulazioni -->
        <DailySimulationsList
          v-if="!currentDetail && !showCreate"
          :simulations="simulations"
          @detail="handleOpenDetail"
          @clone="handleClone"
          @delete="handleDelete"
        />

        <n-spin v-if="!currentDetail && isLoading && !showCreate" :show="true" />

        <!-- Dettaglio simulazione -->
        <DailySimulationDetail
          v-if="currentDetail"
          :simulation="currentDetail"
          @close="closeDetail"
          @refresh="handleRefresh"
          @compare="showCompareSelect = true"
        />

        <!-- Selezione seconda simulazione per confronto -->
        <n-card v-if="currentDetail && showCompareSelect" size="small" title="Confronta con...">
          <n-space size="small">
            <n-select
              v-model:value="compareTargetId"
              :options="otherSimulations"
              placeholder="Seleziona simulazione..."
              style="width: 280px"
            />
            <n-button
              type="primary"
              :disabled="!compareTargetId"
              @click="handleCompare"
            >Confronta</n-button>
            <n-button @click="showCompareSelect = false; compareTargetId = null">Annulla</n-button>
          </n-space>
        </n-card>
      </n-space>
    </n-card>

    <!-- Modal confronto -->
    <DailySimulationCompareModal
      :show="showCompareModal"
      :sim1-id="currentDetail?.id ?? ''"
      :sim2-id="compareTargetId ?? ''"
      @close="showCompareModal = false; compareTargetId = null; showCompareSelect = false"
    />
  </n-space>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { NAlert, NButton, NCard, NFormItem, NInput, NSelect, NSpace, NSpin, useDialog, type SelectOption } from 'naive-ui'
import DailySimulationsList from '@/components/DailySimulations/DailySimulationsList.vue'
import DailySimulationDetail from '@/components/DailySimulations/DailySimulationDetail.vue'
import DailySimulationCompareModal from '@/components/DailySimulations/DailySimulationCompareModal.vue'
import { useDailySimulations } from '@/modules/dailySimulations/composables/useDailySimulations'
import type { DailySimulationListItemDto } from '@/Interfaces/dailySimulations/DailySimulationDto'

const {
  simulations,
  currentDetail,
  isLoading,
  errorMessage,
  loadSimulations,
  loadDetail,
  closeDetail,
  create,
  remove,
  clone
} = useDailySimulations()

const dialog = useDialog()

const showCreate = ref(false)
const newName = ref('')
const showCompareSelect = ref(false)
const compareTargetId = ref<string | null>(null)
const showCompareModal = ref(false)

const otherSimulations = computed<SelectOption[]>(() =>
  simulations.value
    .filter((s) => s.id !== currentDetail.value?.id)
    .map((s) => ({ label: s.name, value: s.id }))
)

onMounted(async () => {
  await loadSimulations()
})

const handleCreate = async () => {
  if (!newName.value.trim()) return
  const id = await create(newName.value)
  if (id) {
    showCreate.value = false
    newName.value = ''
    await loadDetail(id)
  }
}

const handleOpenDetail = async (sim: DailySimulationListItemDto) => {
  await loadDetail(sim.id)
}

const handleRefresh = async () => {
  if (currentDetail.value) await loadDetail(currentDetail.value.id)
}

const handleClone = async (sim: DailySimulationListItemDto) => {
  const id = await clone(sim.id)
  if (id) await loadDetail(id)
}

const handleDelete = (sim: DailySimulationListItemDto) => {
  dialog.warning({
    title: 'Conferma eliminazione',
    content: `Eliminare la simulazione "${sim.name}"?`,
    positiveText: 'Elimina',
    negativeText: 'Annulla',
    onPositiveClick: () => remove(sim.id)
  })
}

const handleCompare = () => {
  if (!compareTargetId.value) return
  showCompareModal.value = true
  showCompareSelect.value = false
}
</script>
