<template>
  <n-select
    v-model:value="model"
    :options="options"
    :loading="loading"
    :disabled="disabled"
    placeholder="Seleziona LLM..."
    size="small"
    style="min-width: 180px"
    aria-label="Provider LLM per estrazione nutrienti"
  />
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { NSelect, type SelectOption } from 'naive-ui'
import { getVisionProviders, type VisionProviderDto } from '@/modules/foods/api/foods.api'

const props = defineProps<{
  modelValue: string
  disabled?: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const model = computed({
  get: () => props.modelValue,
  set: (v) => emit('update:modelValue', v)
})

const loading = ref(false)
const providers = ref<VisionProviderDto[]>([])

const options = computed<SelectOption[]>(() =>
  providers.value.map((p) => ({ label: p.label, value: p.key }))
)

onMounted(async () => {
  loading.value = true
  try {
    providers.value = await getVisionProviders()
    if (!props.modelValue && providers.value.length > 0)
      emit('update:modelValue', providers.value[0].key)
  } finally {
    loading.value = false
  }
})
</script>
