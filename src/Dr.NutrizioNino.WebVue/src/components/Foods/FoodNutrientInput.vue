<template>
  <n-grid :cols="4" :x-gap="8" :y-gap="0" align-items="center">
    <n-gi :span="2">
      <n-text>{{ props.foodNutrientDto.name }}</n-text>
    </n-gi>
    <n-gi>
      <n-select
        v-model:value="selectedUnitOfMeasureId"
        :options="unitOfMeasureOptions"
        size="small"
      />
    </n-gi>
    <n-gi>
      <n-input-number
        v-model:value="quantity"
        :min="0"
        :max="9999"
        :precision="2"
        :show-button="false"
        size="small"
        style="width: 100%"
      />
    </n-gi>
  </n-grid>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NGi, NGrid, NInputNumber, NSelect, NText, type SelectOption } from 'naive-ui'
import type { FoodNutrientDto } from '@/Interfaces/foods/FoodNutrientDto'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'

const props = defineProps<{
  foodNutrientDto: FoodNutrientDto
  unitsOfMeasures: UnitOfMeasureDto[]
}>()

const emit = defineEmits<{
  update: [foodNutrient: FoodNutrientDto]
}>()

const selectedUnitOfMeasureId = ref<string>(props.foodNutrientDto.unitOfMeasureId)
const quantity = ref<number>(props.foodNutrientDto.quantity)

const unitOfMeasureOptions = computed<SelectOption[]>(() =>
  props.unitsOfMeasures.map((unit) => ({ label: unit.name, value: unit.id }))
)

watch(() => props.foodNutrientDto.unitOfMeasureId, (v) => { selectedUnitOfMeasureId.value = v }, { immediate: true })
watch(() => props.foodNutrientDto.quantity, (v) => { quantity.value = v }, { immediate: true })

watch([selectedUnitOfMeasureId, quantity], () => {
  emit('update', {
    ...props.foodNutrientDto,
    unitOfMeasureId: selectedUnitOfMeasureId.value,
    quantity: quantity.value
  })
})
</script>
