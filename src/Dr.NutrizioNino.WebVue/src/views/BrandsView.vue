<script setup lang="ts">
import { onMounted } from 'vue'
import { NAlert, NButton, NCard, NSpace } from 'naive-ui'
import list from '../components/Brands/BrandsList.vue'
import detail from '../components/Brands/BrandDetail.vue'
import { useBrands } from '@/modules/brands/composables/useBrands'

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
          @delete="removeBrand"
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
