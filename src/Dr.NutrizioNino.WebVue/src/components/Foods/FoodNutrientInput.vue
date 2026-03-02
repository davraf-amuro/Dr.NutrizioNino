<template>
  <div class="container">
    <div class="column-1-4">
      {{ prop.foodNutrientDto?.name }}
    </div>
    <div class="column-2-4">
      <n-select
        v-model:value="selectedUnitOfMeasureId"
        :options="unitOfMeasureOptions"
        size="tiny"
      ></n-select>
    </div>
    <div class="column-1-4">
      <n-input-number
        v-model:value="quantity"
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
import { computed, ref, watch } from 'vue'
import { NSelect, NInputNumber } from 'naive-ui'
import type { SelectOption } from 'naive-ui'
import type { FoodNutrientDto } from '@/Interfaces/foods/FoodNutrientDto'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'

const prop = defineProps<{
  foodNutrientDto: FoodNutrientDto
  unitsOfMeasures: UnitOfMeasureDto[]
}>()

const emit = defineEmits<{
  update: [foodNutrient: FoodNutrientDto]
}>()

const selectedUnitOfMeasureId = ref<string>(prop.foodNutrientDto.unitOfMeasureId)
const quantity = ref<number>(prop.foodNutrientDto.quantity)

const unitOfMeasureOptions = computed<SelectOption[]>(() =>
  prop.unitsOfMeasures.map((unit) => ({
    label: unit.name,
    value: unit.id
  }))
)

watch(
  () => prop.foodNutrientDto.unitOfMeasureId,
  (unitOfMeasureId) => {
    selectedUnitOfMeasureId.value = unitOfMeasureId
  },
  { immediate: true }
)

watch(
  () => prop.foodNutrientDto.quantity,
  (newQuantity) => {
    quantity.value = newQuantity
  },
  { immediate: true }
)

watch([selectedUnitOfMeasureId, quantity], () => {
  emit('update', {
    ...prop.foodNutrientDto,
    unitOfMeasureId: selectedUnitOfMeasureId.value,
    quantity: quantity.value
  })
})
</script>

<style scoped></style>
