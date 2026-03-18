<template>
  <n-space vertical size="medium">
    <n-h3 style="margin: 0">{{ isEditMode ? 'Modifica unità di misura' : 'Aggiungi nuova unità di misura' }}</n-h3>

    <n-form ref="formRef" :model="formModel" :rules="rules">
      <n-form-item label="Nome" path="name">
        <n-input v-model:value="formModel.name" :maxlength="50" :disabled="isSubmitting" />
      </n-form-item>

      <n-form-item label="Abbreviazione" path="abbreviation">
        <n-input v-model:value="formModel.abbreviation" :maxlength="5" :disabled="isSubmitting" style="width: 120px" />
      </n-form-item>
    </n-form>

    <n-space justify="end" size="small">
      <n-button @click="cancel" :disabled="isSubmitting">Annulla</n-button>
      <n-button type="primary" @click="save" :loading="isSubmitting">
        {{ isEditMode ? 'Aggiorna' : 'Salva' }}
      </n-button>
    </n-space>
  </n-space>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { NButton, NForm, NFormItem, NH3, NInput, NSpace, type FormInst, type FormRules } from 'naive-ui'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'

const props = defineProps<{
  mode?: 'create' | 'edit'
  unit?: UnitOfMeasureDto | null
  isSubmitting?: boolean
}>()

const emit = defineEmits<{
  save: [payload: UnitOfMeasureDto]
  cancel: []
}>()

const formRef = ref<FormInst | null>(null)
const isEditMode = computed(() => props.mode === 'edit')

const formModel = reactive({ name: '', abbreviation: '' })

const rules: FormRules = {
  name: [{ required: true, message: 'Il nome è obbligatorio', trigger: ['blur', 'input'] }],
  abbreviation: [{ required: true, message: "L'abbreviazione è obbligatoria", trigger: ['blur', 'input'] }]
}

watch(
  () => props.unit,
  (u) => {
    formModel.name = u?.name ?? ''
    formModel.abbreviation = u?.abbreviation ?? ''
  },
  { immediate: true }
)

function save() {
  formRef.value?.validate((errors) => {
    if (!errors) {
      emit('save', {
        id: props.unit?.id ?? '',
        name: formModel.name.trim(),
        abbreviation: formModel.abbreviation.trim()
      })
    }
  })
}

function cancel() {
  emit('cancel')
}
</script>
