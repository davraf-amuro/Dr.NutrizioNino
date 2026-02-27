<template>
  <n-space vertical size="medium">
    <n-h3 style="margin: 0">{{ isEditMode ? 'Modifica marca' : 'Aggiungi nuova marca' }}</n-h3>

    <n-form>
      <n-form-item label="Nome">
        <n-input
          v-model:value="name"
          placeholder="Inserisci il nome della marca"
          clearable
          :disabled="isSubmitting"
        />
      </n-form-item>
    </n-form>

    <n-space justify="end" size="small">
      <n-button @click="cancel" :disabled="isSubmitting">Annulla</n-button>
      <n-button type="primary" @click="save" :disabled="!name.trim()" :loading="isSubmitting">
        {{ isEditMode ? 'Aggiorna' : 'Salva' }}
      </n-button>
    </n-space>
  </n-space>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NButton, NForm, NFormItem, NH3, NInput, NSpace } from 'naive-ui'
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

const name = ref('')
const isEditMode = computed(() => props.mode === 'edit')

watch(
  () => props.brand,
  (brand) => {
    name.value = brand?.name ?? ''
  },
  { immediate: true }
)

function save() {
  emit('save', name.value.trim())
  name.value = ''
}

function cancel() {
  emit('cancel')
  name.value = ''
}
</script>

<style scoped></style>
