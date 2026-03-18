<template>
  <n-space vertical size="medium">
    <n-h3 style="margin: 0">{{ isEditMode ? 'Modifica marca' : 'Aggiungi nuova marca' }}</n-h3>

    <n-form ref="formRef" :model="formModel" :rules="rules">
      <n-form-item label="Nome" path="name">
        <n-input
          v-model:value="formModel.name"
          placeholder="Inserisci il nome della marca"
          clearable
          :disabled="isSubmitting"
        />
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
import type { Brand } from '@/Interfaces/Brand'

const props = defineProps<{
  mode?: 'create' | 'edit'
  brand?: Brand | null
  isSubmitting?: boolean
}>()

const emit = defineEmits<{
  save: [name: string]
  cancel: []
}>()

const formRef = ref<FormInst | null>(null)
const formModel = reactive({ name: '' })
const isEditMode = computed(() => props.mode === 'edit')

const rules: FormRules = {
  name: [{ required: true, message: 'Il nome è obbligatorio', trigger: ['blur', 'input'] }]
}

watch(
  () => props.brand,
  (brand) => {
    formModel.name = brand?.name ?? ''
  },
  { immediate: true }
)

function save() {
  formRef.value?.validate((errors) => {
    if (!errors) {
      emit('save', formModel.name.trim())
      formModel.name = ''
    }
  })
}

function cancel() {
  emit('cancel')
  formModel.name = ''
}
</script>

<style scoped></style>
