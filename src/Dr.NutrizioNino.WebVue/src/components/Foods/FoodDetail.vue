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
          v-model:value="myFood.brandId"
        ></n-select>
      </div>
    </div>
    <!-- {{ myFood.brandId } -->
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
          v-model:value="myFood.unitOfMeasureId"
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
          v-model:value="myFood.quantity"
        ></n-input-number>
      </div>
    </div>
    <br />

    <h3>Nutrienti:</h3>
    <!-- <div v-for="fnu in foodNutrients" :key="fnu.nutrientId">
      <foodnutrientinput :food-nutrient="fnu" :units-of-measures="uom"></foodnutrientinput>
    </div> -->
    <div v-for="fnu in myFood.nutrients" :key="fnu.nutrientId">
      <foodnutrientinput
        :dto="{foodNutrient: fnu, unitsOfMeasures: uom, unitOfMeasureSelectedId: }"
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
import type { BrandsApiResponse } from '@/Interfaces/BrandsApiResponse'
import type { ApiResponseDto } from '@/Interfaces/ApiResponseDto'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import type { Nutrient } from '@/Interfaces/Nutrient'
import type { FoodNutrient } from '@/Interfaces/foods/FoodNutrient'
import type { CreatingFoodDto } from '@/Interfaces/CreatingFoodDto'
import foodnutrientinput from '../Foods/FoodNutrientInput.vue'
import FoodNutrientInput from '../Foods/FoodNutrientInput.vue'
import type { FoodNutrientInputDto } from '@/Interfaces/foods/FoodNutrientInputDto'

const waiting = ref(true)
const brands = ref()
const uom = ref()
const foodNutrients = ref<FoodNutrient[]>([])
const pps = defineProps<{ food: CreatingFoodDto }>()
const myFood = ref<CreatingFoodDto>(pps.food)

const emit = defineEmits<{
  cancel: any
  complete: any
}>()

axios.get('https://localhost:44360/brands').then(function (response) {
  const casted: BrandsApiResponse = response.data
  brands.value = casted.data
})

axios.get('https://localhost:44360/unitsOfMeasures').then(function (response) {
  const casted: ApiResponseDto<UnitOfMeasureDto> = response.data
  uom.value = casted.data
})

axios.get('https://localhost:44360/nutrients').then(function (response) {
  const casted: ApiResponseDto<Nutrient> = response.data
  casted.data.forEach(function (nutrient) {
    const fn: FoodNutrient = {
      nutrientId: nutrient.id,
      nutrientName: nutrient.name,
      foodid: '',
      unitOfMeasureId: '',
      quantity: 0
    }
    foodNutrients.value.push(fn)
  })
})

let dto: FoodNutrientInputDto = {
  foodNutrient: undefined,
  unitsOfMeasures: [],
  unitOfMeasureSelectedId: ''
}

const cancelHandler = () => {
  emit('cancel')
}
const completeHandler = () => {
  emit('complete')
}
waiting.value = false
</script>

<style scoped></style>
