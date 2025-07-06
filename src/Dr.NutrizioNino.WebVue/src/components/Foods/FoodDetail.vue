<template>
  <div>
    <span v-if="waiting">caricamento...</span>
    <div class="container">
      <div class="column-1-4">
        <b>Nome:</b>
      </div>
      <div class="column-3-4">
        <n-input :minlength="3" :maxlength="50" size="tiny" :value="food?.name"></n-input>
      </div>
    </div>

    <div class="container">
      <div class="column-1-4">
        <b>Marca:</b>
      </div>
      <div class="column-3-4">
        <n-select
          :options="brands"
          value-field="id"
          label-field="name"
          size="tiny"
          v-model:value="food.brandId"
        ></n-select>
      </div>
    </div>
    <!-- {{ food.brandId } -->
    <div class="container">
      <div class="column-1-4">
        <b>Unità di misura</b>
      </div>
      <div class="column-2-4">
        <n-select
          :options="uom"
          value-field="id"
          label-field="name"
          size="tiny"
          v-model:value="food.unitOfMeasureId"
        ></n-select>
      </div>
      <div class="column-1-4">
        <n-input-number
          :min="0"
          :max="9999"
          :parse="parseFloat"
          :show-button="false"
          :default-value="0.0"
          size="tiny"
          v-model:value="food.quantity"
        ></n-input-number>
      </div>
    </div>
    <br />

    <h3>Nutrienti:</h3>
    <div v-for="fnu in food.nutrients" :key="fnu.nutrientId">
      <foodnutrientinput
        :foodNutrientDto="fnu"
        :unitsOfMeasures="uom"
        :unitOfMeasureSelectedId="fnu.unitOfMeasureId"
      ></foodnutrientinput>
    </div>
  </div>
  <br />

  <n-flex justify="space-between">
    <n-button type="error" @click="cancelHandler">Annulla</n-button>
    <n-button type="primary" @click="completeHandler">Salva</n-button>
  </n-flex>
</template>

<script setup lang="ts">
import { ref, defineProps } from 'vue'
import axios from 'axios'
import { NSelect, NInput, NInputNumber, NFlex, NButton } from 'naive-ui'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'
import type { BrandsApiResponse } from '@/Interfaces/BrandsApiResponse'
import type { ApiResponseMultipleDto } from '@/Interfaces/ApiResponseMultipleDto'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import foodnutrientinput from '../Foods/FoodNutrientInput.vue'

const waiting = ref(true)
const brands = ref()
const uom = ref<UnitOfMeasureDto[]>([])
const props = defineProps<{ food: FoodDto }>()

const emit = defineEmits<{
  cancel: any
  complete: any
}>()

axios.get('https://localhost:44360/brands').then(function (response) {
  const casted: BrandsApiResponse = response.data
  brands.value = casted.data
})

//carico la collezione di unità di misura per le combo
axios.get('https://localhost:44360/unitsOfMeasures').then(function (response) {
  const casted: ApiResponseMultipleDto<UnitOfMeasureDto> = response.data
  uom.value = casted.data
})

const cancelHandler = () => {
  emit('cancel')
}
const completeHandler = () => {
  emit('complete')
}
waiting.value = false
</script>

<style scoped></style>
