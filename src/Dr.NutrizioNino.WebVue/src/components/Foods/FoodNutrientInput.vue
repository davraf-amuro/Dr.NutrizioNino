<template>
  <div class="container">
    <div class="column-1-4">
      {{ prop.foodNutrientDto?.name }}
    </div>
    <div class="column-2-4">
      <n-select
        :options="prop.unitsOfMeasures"
        value-field="id"
        label-field="name"
        size="tiny"
        v-model:value="selectedId"
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
        v-model:value="prop.foodNutrientDto.quantity"
      ></n-input-number>
    </div>
  </div>
</template>

<script setup lang="ts">
import { NSelect, NInputNumber } from 'naive-ui'
import { ref } from 'vue'
import type { FoodNutrientDto } from '@/Interfaces/foods/FoodNutrientDto'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'

const prop = defineProps<{
  foodNutrientDto: FoodNutrientDto
  unitsOfMeasures: UnitOfMeasureDto[]
  unitOfMeasureSelectedId: number
}>()

const selectedId = ref<number>(prop.unitOfMeasureSelectedId)
</script>

<style scoped></style>
