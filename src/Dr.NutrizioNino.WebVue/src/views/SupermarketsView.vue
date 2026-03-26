<script setup lang="ts">
import { onMounted } from 'vue'
import { NAlert, NButton, NCard, NSpace, useDialog, useMessage } from 'naive-ui'
import SupermarketsList from '@/components/Supermarkets/SupermarketsList.vue'
import SupermarketDetail from '@/components/Supermarkets/SupermarketDetail.vue'
import { useSupermarkets } from '@/modules/supermarkets/composables/useSupermarkets'
import { isSupermarketInUse } from '@/modules/supermarkets/api/supermarkets.api'
import type { Supermarket } from '@/Interfaces/Supermarket'

const {
  supermarkets,
  isCreating,
  formMode,
  selectedSupermarket,
  isLoading,
  errorMessage,
  loadSupermarkets,
  startCreateSupermarket,
  startEditSupermarket,
  cancelCreateSupermarket,
  submitSupermarket,
  removeSupermarket
} = useSupermarkets()

const message = useMessage()
const dialog = useDialog()

const handleDeleteSupermarket = async (supermarket: Supermarket) => {
  const inUse = await isSupermarketInUse(supermarket.id)
  if (inUse) {
    message.warning(`Il supermercato "${supermarket.name}" è collegato a uno o più alimenti e non può essere eliminato.`)
    return
  }

  dialog.warning({
    title: 'Conferma eliminazione',
    content: `Eliminare il supermercato "${supermarket.name}"?`,
    positiveText: 'Elimina',
    negativeText: 'Annulla',
    onPositiveClick: () => removeSupermarket(supermarket)
  })
}

onMounted(async () => {
  await loadSupermarkets()
})
</script>

<template>
  <n-space vertical size="large" class="supermarkets-page">
    <n-card title="Supermercati" size="large">
      <template #header-extra>
        <n-button
          v-if="!isCreating"
          type="primary"
          @click="startCreateSupermarket"
          :loading="isLoading"
        >
          Nuovo supermercato
        </n-button>
      </template>

      <n-space vertical size="medium">
        <n-alert v-if="errorMessage" type="error" :show-icon="true" :bordered="false">
          {{ errorMessage }}
        </n-alert>

        <SupermarketsList
          v-if="!isCreating"
          :supermarkets="supermarkets"
          @edit="startEditSupermarket"
          @delete="handleDeleteSupermarket"
        />

        <SupermarketDetail
          v-else
          :mode="formMode"
          :supermarket="selectedSupermarket"
          :is-submitting="isLoading"
          @save="submitSupermarket"
          @cancel="cancelCreateSupermarket"
        />
      </n-space>
    </n-card>
  </n-space>
</template>

<style scoped>
.supermarkets-page {
  width: 100%;
}
</style>
