<template>
  <n-modal
    v-model:show="showModal"
    preset="card"
    title="Aggiungi nuovo nutriente"
    style="width: 400px"
    :closable="!isSubmitting"
    :mask-closable="!isSubmitting"
  >
    <n-form ref="formRef" :model="formModel" :rules="rules">
      <n-form-item label="Nome" path="name">
        <n-input v-model:value="formModel.name" :maxlength="100" :disabled="isSubmitting" />
      </n-form-item>
      <n-form-item label="Unità di misura" path="unitOfMeasureId">
        <n-select
          v-model:value="formModel.unitOfMeasureId"
          :options="unitOptions"
          placeholder="-seleziona-"
          filterable
          :disabled="isSubmitting"
          aria-label="Unità di misura del nutriente"
        />
      </n-form-item>
    </n-form>

    <n-alert v-if="errorMessage" type="error" :bordered="false" style="margin-top: 8px">
      {{ errorMessage }}
    </n-alert>

    <template #footer>
      <n-space justify="end">
        <n-button @click="close" :disabled="isSubmitting">Annulla</n-button>
        <n-button type="primary" @click="save" :loading="isSubmitting">Salva</n-button>
      </n-space>
    </template>
  </n-modal>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import {
  NAlert,
  NButton,
  NForm,
  NFormItem,
  NInput,
  NModal,
  NSelect,
  NSpace,
  type FormInst,
  type FormRules,
  type SelectOption
} from 'naive-ui'
import type { Nutrient } from '@/Interfaces/Nutrients/Nutrient'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import { createNutrient } from '@/modules/nutrients/api/nutrients.api'
import { ApiError } from '@/core/http/ApiError'

const props = defineProps<{
  show: boolean
  unitsOfMeasures: UnitOfMeasureDto[]
  suggestedName?: string
  suggestedUnit?: string
}>()

const emit = defineEmits<{
  'update:show': [value: boolean]
  created: [payload: { nutrient: Nutrient; unitOfMeasureId: string }]
}>()

const showModal = computed({
  get: () => props.show,
  set: (val) => emit('update:show', val)
})

const unitOptions = computed<SelectOption[]>(() =>
  [...props.unitsOfMeasures]
    .sort((a, b) => a.name.localeCompare(b.name, 'it'))
    .map((u) => ({ label: `${u.name} (${u.abbreviation})`, value: u.id }))
)

const formRef = ref<FormInst | null>(null)
const formModel = reactive<{ name: string; unitOfMeasureId: string | null }>({ name: '', unitOfMeasureId: null })
const isSubmitting = ref(false)
const errorMessage = ref<string | null>(null)

const rules: FormRules = {
  name: [{ required: true, message: 'Il nome è obbligatorio', trigger: ['blur', 'input'] }],
  unitOfMeasureId: [{ required: true, message: "L'unità di misura è obbligatoria", trigger: ['blur', 'change'] }]
}

// Tenta di preselezionare la UoM confrontando per sigla (abbreviation), non per nome
const matchUnitByAbbreviation = (unit?: string): string | null => {
  if (!unit) return null
  const target = unit.trim().toLowerCase()
  const found = props.unitsOfMeasures.find((u) => u.abbreviation?.toLowerCase() === target)
  return found?.id ?? null
}

watch(
  () => props.show,
  (val) => {
    if (val) {
      formModel.name = props.suggestedName?.trim() ?? ''
      formModel.unitOfMeasureId = matchUnitByAbbreviation(props.suggestedUnit)
      errorMessage.value = null
    }
  }
)

async function save() {
  formRef.value?.validate(async (errors) => {
    if (errors) return
    isSubmitting.value = true
    errorMessage.value = null
    try {
      const created = await createNutrient({
        name: formModel.name.trim(),
        defaultUnitOfMeasureId: formModel.unitOfMeasureId
      })
      emit('created', { nutrient: created, unitOfMeasureId: formModel.unitOfMeasureId! })
      emit('update:show', false)
    } catch (e) {
      if (e instanceof ApiError && e.status === 409) {
        errorMessage.value = `Il nutriente "${formModel.name.trim()}" esiste già.`
      } else {
        errorMessage.value = 'Errore durante il salvataggio. Riprova.'
      }
    } finally {
      isSubmitting.value = false
    }
  })
}

function close() {
  emit('update:show', false)
}
</script>
