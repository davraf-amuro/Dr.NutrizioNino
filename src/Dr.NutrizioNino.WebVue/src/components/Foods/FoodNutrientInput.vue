<template>
  <div class="nutrient-row">
    <n-text class="nutrient-label">{{ props.foodNutrientDto.name }}</n-text>
    <n-select
      v-model:value="selectedUnitOfMeasureId"
      :options="unitOfMeasureOptions"
      size="small"
      class="nutrient-uom"
    />
    <n-input-number
      v-model:value="quantity"
      :min="0"
      :max="9999"
      :precision="2"
      :show-button="false"
      size="small"
      class="nutrient-qty"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NInputNumber, NSelect, NText, type SelectOption } from 'naive-ui'
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

let debounceTimer: ReturnType<typeof setTimeout> | null = null

watch([selectedUnitOfMeasureId, quantity], () => {
  if (debounceTimer) clearTimeout(debounceTimer)
  debounceTimer = setTimeout(() => {
    emit('update', {
      ...props.foodNutrientDto,
      unitOfMeasureId: selectedUnitOfMeasureId.value,
      quantity: quantity.value
    })
  }, 300)
})
</script>

<style scoped>
.nutrient-row {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 4px;
}

.nutrient-label {
  width: 140px;
  min-width: 140px;
  text-align: right;
  font-size: 14px;
}

.nutrient-uom {
  width: 160px;
  min-width: 160px;
}

.nutrient-qty {
  width: 100px;
  min-width: 100px;
}
</style>
