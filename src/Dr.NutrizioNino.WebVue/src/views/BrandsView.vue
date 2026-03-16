<script setup lang="ts">
import { onMounted } from 'vue'
import { NAlert, NButton, NCard, NSpace, useDialog, useMessage } from 'naive-ui'
import list from '../components/Brands/BrandsList.vue'
import detail from '../components/Brands/BrandDetail.vue'
import { useBrands } from '@/modules/brands/composables/useBrands'
import { isBrandInUse } from '@/modules/brands/api/brands.api'
import type { Brand } from '@/Interfaces/Brand'

const {
  brands,
  isCreating,
  formMode,
  selectedBrand,
  isLoading,
  errorMessage,
  loadBrands,
  startCreateBrand,
  startEditBrand,
  cancelCreateBrand,
  submitCreateBrand,
  removeBrand
} = useBrands()

const message = useMessage()
const dialog = useDialog()

const handleDeleteBrand = async (brand: Brand) => {
  const inUse = await isBrandInUse(brand.id)
  if (inUse) {
    message.warning(`La marca "${brand.name}" è in uso da uno o più alimenti e non può essere eliminata.`)
    return
  }

  dialog.warning({
    title: 'Conferma eliminazione',
    content: `Eliminare la marca "${brand.name}"?`,
    positiveText: 'Elimina',
    negativeText: 'Annulla',
    onPositiveClick: () => removeBrand(brand)
  })
}

onMounted(async () => {
  await loadBrands()
})
</script>

<template>
  <n-space vertical size="large" class="brands-page">
    <n-card title="Marche" size="large">
      <template #header-extra>
        <n-button
          v-if="!isCreating"
          type="primary"
          @click="startCreateBrand"
          :loading="isLoading"
        >
          Nuova marca
        </n-button>
      </template>

      <n-space vertical size="medium">
        <n-alert v-if="errorMessage" type="error" :show-icon="true" :bordered="false">
          {{ errorMessage }}
        </n-alert>

        <list
          v-if="!isCreating"
          :brands="brands"
          @edit="startEditBrand"
          @delete="handleDeleteBrand"
        />

        <detail
          v-else
          :mode="formMode"
          :brand="selectedBrand"
          :is-submitting="isLoading"
          @save="submitCreateBrand"
          @cancel="cancelCreateBrand"
        />
      </n-space>
    </n-card>
  </n-space>
</template>

<style scoped>
.brands-page {
  width: 100%;
}
</style>
