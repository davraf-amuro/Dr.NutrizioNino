<template>
  <n-modal
    v-model:show="showModal"
    preset="card"
    title="Aggiungi nuova unità di misura"
    style="width: 400px"
    :closable="!isSubmitting"
    :mask-closable="!isSubmitting"
  >
    <n-form ref="formRef" :model="formModel" :rules="rules">
      <n-form-item label="Nome" path="name">
        <n-input v-model:value="formModel.name" :maxlength="50" :disabled="isSubmitting" />
      </n-form-item>
      <n-form-item label="Abbreviazione" path="abbreviation">
        <n-input
          v-model:value="formModel.abbreviation"
          :maxlength="5"
          :disabled="isSubmitting"
          style="width: 120px"
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
  NSpace,
  type FormInst,
  type FormRules
} from 'naive-ui'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import { createUnitOfMeasure } from '@/modules/units/api/units.api'
import { ApiError } from '@/core/http/ApiError'

const props = defineProps<{ show: boolean }>()

const emit = defineEmits<{
  'update:show': [value: boolean]
  created: [unit: UnitOfMeasureDto]
}>()

const showModal = computed({
  get: () => props.show,
  set: (val) => emit('update:show', val)
})

const formRef = ref<FormInst | null>(null)
const formModel = reactive({ name: '', abbreviation: '' })
const isSubmitting = ref(false)
const errorMessage = ref<string | null>(null)

const rules: FormRules = {
  name: [{ required: true, message: 'Il nome è obbligatorio', trigger: ['blur', 'input'] }],
  abbreviation: [{ required: true, message: "L'abbreviazione è obbligatoria", trigger: ['blur', 'input'] }]
}

watch(
  () => props.show,
  (val) => {
    if (val) {
      formModel.name = ''
      formModel.abbreviation = ''
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
      const created = await createUnitOfMeasure({
        name: formModel.name.trim(),
        abbreviation: formModel.abbreviation.trim()
      })
      emit('created', created)
      emit('update:show', false)
    } catch (e) {
      if (e instanceof ApiError && e.status === 409) {
        errorMessage.value = `L'unità di misura "${formModel.name.trim()}" esiste già.`
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
