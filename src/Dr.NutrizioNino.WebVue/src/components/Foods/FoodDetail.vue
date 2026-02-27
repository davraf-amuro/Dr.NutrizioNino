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
          placeholder="-seleziona-"
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
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import type { Brand } from '@/Interfaces/Brand'
import config from '@/config'
import foodnutrientinput from '../Foods/FoodNutrientInput.vue'

const waiting = ref(true)
const brands = ref<Brand[]>([])
const uom = ref<UnitOfMeasureDto[]>([])
const props = defineProps<{ food: FoodDto }>()
const emptyGuid = '00000000-0000-0000-0000-000000000000'

const emit = defineEmits<{ cancel: any; complete: any }>()

const ensureBrandSelection = () => {
  if (!brands.value.length) {
    return
  }

  const brandId = localFood.value.brandId
  const hasValidBrand =
    !!brandId && brandId !== emptyGuid && brands.value.some((brand) => brand.id === brandId)

  if (hasValidBrand) {
    return
  }

  localFood.value.brandId = null
}

axios.get(`${config.API_BASE_URL}/brands`).then(function (response) {
  brands.value = response.data
  ensureBrandSelection()
})

//carico la collezione di unità di misura per le combo
axios.get(`${config.API_BASE_URL}/unitsOfMeasures`).then(function (response) {
  uom.value = response.data
})

const localFood = ref({ ...props.food })

// Sincronizza localFood quando la prop food cambia
watch(
  () => props.food,
  (newFood) => {
    localFood.value = { ...newFood }
    ensureBrandSelection()
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
