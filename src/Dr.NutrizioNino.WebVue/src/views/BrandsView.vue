<script setup lang="ts">
import { onMounted } from 'vue'
import list from '../components/Brands/BrandsList.vue'
import detail from '../components/Brands/BrandDetail.vue'
import { useBrands } from '@/modules/brands/composables/useBrands'
import type { Brand } from '@/Interfaces/Brand'

const {
  brands,
  isCreating,
  isLoading,
  errorMessage,
  loadBrands,
  startCreateBrand,
  cancelCreateBrand,
  submitCreateBrand
} = useBrands()

onMounted(async () => {
  await loadBrands()
})

function updateBrand(brand: Brand) {
  void brand
}

function deleteBrand(brand: Brand) {
  void brand
}
</script>

<template>
  <div class="brands">
    <h1>Marche</h1>
    <p v-if="errorMessage">{{ errorMessage }}</p>
    <br />
    <button @click="startCreateBrand" v-if="!isCreating" :disabled="isLoading">Nuovo</button>

    <br />
    <list
      v-if="!isCreating"
      :brands="brands"
      @update="updateBrand"
      @delete="deleteBrand"
    />
    <br />
    <detail
      v-if="isCreating"
      :is-submitting="isLoading"
      @save="submitCreateBrand"
      @cancel="cancelCreateBrand"
    />
  </div>
</template>
