<template>
  <n-space vertical size="medium">
    <n-h3 style="margin: 0">{{ isEditMode ? 'Modifica unità di misura' : 'Aggiungi nuova unità di misura' }}</n-h3>

    <n-form>
      <n-form-item label="Nome">
        <n-input v-model:value="localName" :maxlength="50" :disabled="isSubmitting" />
      </n-form-item>

      <n-form-item label="Abbreviazione">
        <n-input v-model:value="localAbbreviation" :maxlength="5" :disabled="isSubmitting" style="width: 120px" />
      </n-form-item>
    </n-form>

    <n-space justify="end" size="small">
      <n-button @click="cancel" :disabled="isSubmitting">Annulla</n-button>
      <n-button
        type="primary"
        @click="save"
        :disabled="!localName.trim() || !localAbbreviation.trim()"
        :loading="isSubmitting"
      >
        {{ isEditMode ? 'Aggiorna' : 'Salva' }}
      </n-button>
    </n-space>
  </n-space>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NButton, NForm, NFormItem, NH3, NInput, NSpace } from 'naive-ui'
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

const isEditMode = computed(() => props.mode === 'edit')

const localName = ref('')
const localAbbreviation = ref('')

watch(
  () => props.unit,
  (u) => {
    localName.value = u?.name ?? ''
    localAbbreviation.value = u?.abbreviation ?? ''
  },
  { immediate: true }
)

function save() {
  emit('save', {
    id: props.unit?.id ?? '',
    name: localName.value.trim(),
    abbreviation: localAbbreviation.value.trim()
  })
}

function cancel() {
  emit('cancel')
}
</script>
