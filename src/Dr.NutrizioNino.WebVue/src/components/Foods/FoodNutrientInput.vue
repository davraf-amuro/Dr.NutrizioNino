<template>
  <div class="container">
    <div class="column-1-4">
      {{ prop.foodNutrientDto?.name }}
    </div>
    <div class="column-2-4">
      <n-select
        v-model:value="prop.foodNutrientDto.unitOfMeasureId"
        :options="unitOfMeasureOptions"
        size="tiny"
      ></n-select>
    </div>
    <div class="column-1-4">
      <n-input-number
        v-model:value="prop.foodNutrientDto.quantity"
        :min="0"
        :max="9999"
        :parse="parseFloat"
        :show-button="false"
        :default-value="0.0"
        size="tiny"
      ></n-input-number>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { NSelect, NInputNumber } from 'naive-ui'
import type { SelectOption } from 'naive-ui'
import type { FoodNutrientDto } from '@/Interfaces/foods/FoodNutrientDto'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'

const prop = defineProps<{
  foodNutrientDto: FoodNutrientDto
  unitsOfMeasures: UnitOfMeasureDto[]
}>()

const unitOfMeasureOptions = computed<SelectOption[]>(() =>
  prop.unitsOfMeasures.map((unit) => ({
    label: unit.name,
    value: unit.id
  }))
)
</script>

<style scoped></style>
