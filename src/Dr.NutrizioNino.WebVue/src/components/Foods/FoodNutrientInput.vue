<template>
  <div class="container">
    <div class="column-1-4">
      {{ foodNutrient?.nutrientName }}
    </div>
    <div class="column-2-4">
      <n-select
        :options="uof"
        value-field="id"
        label-field="name"
        size="tiny"
        v-model:value="nutrient.unitOfMeasureId"
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
        v-model:value="nutrient.quantity"
      ></n-input-number>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { FoodNutrient } from '@/Interfaces/FoodNutrient'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import { NSelect, NInputNumber } from 'naive-ui'
import { ref } from 'vue'

const uof = ref<UnitOfMeasureDto[]>([])
const nutrient = ref<FoodNutrient>()

const pps = defineProps<{
  foodNutrient: FoodNutrient
  unitsOfMeasures: UnitOfMeasureDto[]
}>()

uof.value = pps.unitsOfMeasures
nutrient.value = pps.foodNutrient
</script>

<style scoped></style>
