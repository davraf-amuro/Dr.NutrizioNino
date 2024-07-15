<template>
  <div class="container">
    <div class="column-1-4">
      {{ data?.foodNutrient?.nutrientName }}
    </div>
    <div class="column-2-4">
      <n-select
        :options="data.unitsOfMeasures"
        value-field="id"
        label-field="name"
        size="tiny"
        v-model:value="data.unitOfMeasureSelectedId"
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
        v-model:value="data.foodNutrient.quantity"
      ></n-input-number>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { FoodNutrientInputDto } from '@/Interfaces/foods/FoodNutrientInputDto'
import { NSelect, NInputNumber } from 'naive-ui'
import { ref } from 'vue'

const pps = defineProps<{
  dto: FoodNutrientInputDto
}>()
const data = ref<FoodNutrientInputDto>(pps.dto)
</script>

<style scoped></style>
