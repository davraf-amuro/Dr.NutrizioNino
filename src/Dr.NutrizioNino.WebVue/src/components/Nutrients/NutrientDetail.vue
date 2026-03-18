<template>
  <n-space vertical size="medium">
    <n-h3 style="margin: 0">{{ isEditMode ? 'Modifica nutriente' : 'Aggiungi nuovo nutriente' }}</n-h3>

    <n-form>
      <n-form-item label="Nome" path="name">
        <n-input v-model:value="localName" :maxlength="50" :disabled="isSubmitting" />
      </n-form-item>

      <template v-if="isEditMode">
        <n-form-item label="Ordine" path="positionOrder">
          <n-input-number v-model:value="localPositionOrder" :min="0" :disabled="isSubmitting" style="width: 100%" />
        </n-form-item>

        <n-form-item label="Quantità default" path="defaultQuantity">
          <n-input-number v-model:value="localDefaultQuantity" :min="0" :precision="2" :disabled="isSubmitting" style="width: 100%" />
        </n-form-item>

        <n-form-item label="Unità di misura default" path="defaultUnitOfMeasureId">
          <n-select
            v-model:value="localDefaultUnitOfMeasureId"
            :options="unitOptions"
            :disabled="isSubmitting"
          />
        </n-form-item>
      </template>
    </n-form>

    <n-space justify="end" size="small">
      <n-button @click="cancel" :disabled="isSubmitting">Annulla</n-button>
      <n-button type="primary" @click="save" :disabled="!localName.trim()" :loading="isSubmitting">
        {{ isEditMode ? 'Aggiorna' : 'Salva' }}
      </n-button>
    </n-space>
  </n-space>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NButton, NForm, NFormItem, NH3, NInput, NInputNumber, NSelect, NSpace } from 'naive-ui'
import type { Nutrient } from '@/Interfaces/Nutrients/Nutrient'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'

const props = defineProps<{
  mode?: 'create' | 'edit'
  nutrient?: Nutrient | null
  unitsOfMeasures?: UnitOfMeasureDto[]
  isSubmitting?: boolean
}>()

const emit = defineEmits<{
  save: [payload: { name: string } | Nutrient]
  cancel: []
}>()

const isEditMode = computed(() => props.mode === 'edit')

const localName = ref('')
const localPositionOrder = ref(0)
const localDefaultQuantity = ref(0)
const localDefaultUnitOfMeasureId = ref<string | null>(null)

const unitOptions = computed(() =>
  (props.unitsOfMeasures ?? []).map((u) => ({ label: `${u.name} (${u.abbreviation})`, value: u.id }))
)

watch(
  () => props.nutrient,
  (n) => {
    localName.value = n?.name ?? ''
    localPositionOrder.value = n?.positionOrder ?? 0
    localDefaultQuantity.value = n?.defaultQuantity ?? 0
    localDefaultUnitOfMeasureId.value = n?.defaultUnitOfMeasureId ?? null
  },
  { immediate: true }
)

function save() {
  if (isEditMode.value && props.nutrient) {
    emit('save', {
      id: props.nutrient.id,
      name: localName.value.trim(),
      positionOrder: localPositionOrder.value,
      defaultQuantity: localDefaultQuantity.value,
      defaultUnitOfMeasureId: localDefaultUnitOfMeasureId.value ?? props.nutrient.defaultUnitOfMeasureId
    } as Nutrient)
  } else {
    emit('save', { name: localName.value.trim() })
  }
}

function cancel() {
  emit('cancel')
}
</script>
