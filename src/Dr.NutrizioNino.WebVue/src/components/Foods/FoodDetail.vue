<template>
  <div>
    <span v-if="waiting">caricamento...</span>
    <div class="container">
      <div class="column-1-4">
        <b>Nome:</b>
      </div>
      <div class="column-3-4">
        <n-input
          v-model:value="localFood.name"
          :minlength="3"
          :maxlength="50"
          size="tiny"
        ></n-input>
      </div>
    </div>

    <div class="container">
      <div class="column-1-4">
        <b>Marca:</b>
      </div>
      <div class="column-3-4">
        <n-select
          v-model:value="localFood.brandId"
          :options="brands"
          value-field="id"
          label-field="name"
          size="tiny"
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
          v-model:value="localFood.unitOfMeasureId"
          :options="uom"
          value-field="id"
          label-field="name"
          size="tiny"
        ></n-select>
      </div>
      <div class="column-1-4">
        <n-input-number
          :min="0"
          :max="9999"
          :parse="parseFloat"
          :show-button="false"
          v-model:value="localFood.quantity"
          size="tiny"
        ></n-input-number>
      </div>
    </div>
    <br />

    <h3>Nutrienti:</h3>
    <div v-for="fnu in localFood.nutrients" :key="fnu.nutrientId">
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
import { ref, watch } from 'vue'
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

const emit = defineEmits<{ cancel: any; complete: any }>()

axios.get('https://localhost:7048/brands').then(function (response) {
  const casted: BrandsApiResponse = response.data
  brands.value = casted.data
})

//carico la collezione di unità di misura per le combo
axios.get('https://localhost:7048/unitsOfMeasures').then(function (response) {
  const casted: ApiResponseMultipleDto<UnitOfMeasureDto> = response.data
  uom.value = casted.data
})

const localFood = ref({ ...props.food })

// Sincronizza localFood quando la prop food cambia
watch(
  () => props.food,
  (newFood) => {
    localFood.value = { ...newFood }
  },
  { deep: true }
)

// Aggiorna il valore originale quando necessario
const cancelHandler = () => {
  emit('cancel')
}
const completeHandler = () => {
  // Sincronizza localFood con il prop originale prima di emettere
  emit('complete', { ...localFood.value })
}
waiting.value = false
</script>

<style scoped></style>
