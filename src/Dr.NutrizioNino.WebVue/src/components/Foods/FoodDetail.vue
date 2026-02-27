<template>
  <div>
    <span v-if="isSubmitting">caricamento...</span>
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
          :options="brandOptions"
          placeholder="-seleziona-"
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
          :options="unitOptions"
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
        :unitsOfMeasures="unitsOfMeasures"
        @update="updateNutrient"
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
import { computed, ref, watch } from 'vue'
import { NSelect, NInput, NInputNumber, NFlex, NButton, type SelectOption } from 'naive-ui'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import type { Brand } from '@/Interfaces/Brand'
import foodnutrientinput from '../Foods/FoodNutrientInput.vue'

const props = defineProps<{
  food: FoodDto
  brands: Brand[]
  unitsOfMeasures: UnitOfMeasureDto[]
  isSubmitting?: boolean
}>()
const emptyGuid = '00000000-0000-0000-0000-000000000000'

const emit = defineEmits<{
  cancel: []
  complete: [food: FoodDto]
}>()

const isSubmitting = computed(() => props.isSubmitting ?? false)

const brandOptions = computed<SelectOption[]>(() =>
  props.brands.map((brand) => ({
    label: brand.name,
    value: brand.id
  }))
)

const unitOptions = computed<SelectOption[]>(() =>
  props.unitsOfMeasures.map((unit) => ({
    label: unit.name,
    value: unit.id
  }))
)

const cloneFood = (food: FoodDto): FoodDto => ({
  ...food,
  nutrients: food.nutrients.map((nutrient) => ({ ...nutrient }))
})

const ensureBrandSelection = () => {
  if (!props.brands.length) {
    return
  }

  const brandId = localFood.value.brandId
  const hasValidBrand =
    !!brandId && brandId !== emptyGuid && props.brands.some((brand) => brand.id === brandId)

  if (hasValidBrand) {
    return
  }

  localFood.value.brandId = null
}

const localFood = ref(cloneFood(props.food))

// Sincronizza localFood quando la prop food cambia
watch(
  () => props.food,
  (newFood) => {
    localFood.value = cloneFood(newFood)
    ensureBrandSelection()
  },
  { deep: true }
)

watch(
  () => props.brands,
  () => {
    ensureBrandSelection()
  },
  { deep: true }
)

// Aggiorna il valore originale quando necessario
const cancelHandler = () => {
  emit('cancel')
}
const completeHandler = () => {
  emit('complete', cloneFood(localFood.value))
}

const updateNutrient = (updatedNutrient: FoodDto['nutrients'][number]) => {
  const nutrientIndex = localFood.value.nutrients.findIndex(
    (nutrient) => nutrient.nutrientId === updatedNutrient.nutrientId
  )

  if (nutrientIndex >= 0) {
    localFood.value.nutrients[nutrientIndex] = { ...updatedNutrient }
  }
}
</script>

<style scoped></style>
