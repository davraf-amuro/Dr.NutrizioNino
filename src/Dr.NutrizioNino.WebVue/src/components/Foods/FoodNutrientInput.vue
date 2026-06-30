<template>
  <div class="nutrient-row">
    <n-tooltip v-if="statusInfo" trigger="hover">
      <template #trigger>
        <span class="nutrient-status" :aria-label="statusInfo.label" role="img">{{ statusInfo.icon }}</span>
      </template>
      {{ statusInfo.label }}
    </n-tooltip>
    <span v-else class="nutrient-status" aria-hidden="true" />
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
import { NInputNumber, NSelect, NText, NTooltip, type SelectOption } from 'naive-ui'
import type { FoodNutrientDto } from '@/Interfaces/foods/FoodNutrientDto'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import type { ExtractionStatus } from '@/Interfaces/foods/ExtractedNutrientDto'

const props = defineProps<{
  foodNutrientDto: FoodNutrientDto
  unitsOfMeasures: UnitOfMeasureDto[]
  status?: ExtractionStatus
}>()

// Badge accessibile (icona + aria-label, mai colore-solo) per l'esito dell'estrazione AI sulla riga
const statusInfo = computed(() => {
  switch (props.status) {
    case 'Matched':
      return { icon: '✅', label: 'Corrispondenza esatta dall\'AI — verifica il valore' }
    case 'IncompleteMatch':
      return { icon: '⚠️', label: 'Corrispondenza parziale dall\'AI — controlla unità e valore' }
    case 'Unrecognized':
      return { icon: '⛔', label: 'Non riconosciuto dall\'AI' }
    default:
      return null
  }
})

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

.nutrient-status {
  width: 18px;
  min-width: 18px;
  font-size: 14px;
  text-align: center;
  cursor: default;
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
