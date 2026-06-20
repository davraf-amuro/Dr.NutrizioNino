<template>
  <div v-if="incompleteItems.length > 0 || unrecognizedItems.length > 0">

    <!-- Caso A: nutriente riconosciuto ma UoM/valore mancante -->
    <n-alert
      v-if="incompleteItems.length > 0"
      type="warning"
      title="Nutrienti con dati incompleti"
      style="margin-bottom: 8px; font-size: 12px"
      :show-icon="true"
      aria-live="polite"
    >
      <p v-for="item in incompleteItems" :key="item.name" style="margin: 2px 0; display: flex; align-items: center; gap: 6px; flex-wrap: wrap">
        ⚠️ <strong>{{ item.name }}</strong>:
        {{ item.value }} {{ item.unit }}
        <span v-if="!item.canonicalUnit"> — unità di misura non riconoscibile</span>
        <span v-else-if="item.value === 0"> — valore non estratto</span>
        <n-tag
          :type="confidenceTagType(item.confidenceScore)"
          size="small"
          :aria-label="`Confidenza ${confidenceLabel(item.confidenceScore)}: ${Math.round(item.confidenceScore * 100)}%`"
        >{{ Math.round(item.confidenceScore * 100) }}%</n-tag>
      </p>
    </n-alert>

    <!-- Caso B: nutriente non riconosciuto -->
    <n-alert
      v-if="unrecognizedItems.length > 0"
      type="error"
      title="Nutrienti non riconosciuti"
      style="margin-bottom: 8px; font-size: 12px"
      :show-icon="true"
      aria-live="polite"
    >
      <div
        v-for="item in unrecognizedItems"
        :key="item.name"
        style="display: flex; align-items: center; gap: 8px; margin: 4px 0; flex-wrap: wrap"
      >
        <span style="min-width: 140px; display: flex; align-items: center; gap: 6px">
          <strong>{{ item.name }}</strong> ({{ item.value }} {{ item.unit }})
          <n-tag
            :type="confidenceTagType(item.confidenceScore)"
            size="small"
            :aria-label="`Confidenza ${confidenceLabel(item.confidenceScore)}: ${Math.round(item.confidenceScore * 100)}%`"
          >{{ Math.round(item.confidenceScore * 100) }}%</n-tag>
        </span>
        <n-select
          v-model:value="selectedMappings[item.name]"
          :options="nutrientOptions"
          placeholder="Associa a nutriente..."
          size="small"
          filterable
          clearable
          style="min-width: 200px; flex: 1"
          :aria-label="`Associa ${item.name} a nutriente canonico`"
        />
        <n-button
          size="small"
          type="primary"
          :disabled="!selectedMappings[item.name]"
          :loading="savingAlias[item.name]"
          @click="confirmAlias(item)"
        >
          Conferma
        </n-button>
      </div>
    </n-alert>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive } from 'vue'
import { NAlert, NButton, NSelect, NTag, type SelectOption } from 'naive-ui'
import { useMessage } from 'naive-ui'
import type { ExtractedNutrientDto } from '@/Interfaces/foods/ExtractedNutrientDto'
import type { NutrientDto } from '@/Interfaces/Nutrients/NutrientDto'
import { saveNutrientAlias } from '@/modules/foods/api/foods.api'

const props = defineProps<{
  extractedNutrients: ExtractedNutrientDto[]
  availableNutrients: NutrientDto[]
}>()

const emit = defineEmits<{
  'alias-confirmed': [aiName: string, nutrientId: string]
}>()

const message = useMessage()

const confidenceTagType = (score: number) =>
  score >= 0.85 ? 'success' : score >= 0.60 ? 'warning' : 'error'

const confidenceLabel = (score: number) =>
  score >= 0.85 ? 'alta' : score >= 0.60 ? 'media' : 'bassa'

const incompleteItems = computed(() =>
  props.extractedNutrients.filter((n) => n.status === 'IncompleteMatch')
)

const unrecognizedItems = computed(() =>
  props.extractedNutrients.filter((n) => n.status === 'Unrecognized')
)

const nutrientOptions = computed<SelectOption[]>(() =>
  props.availableNutrients.map((n) => ({ label: n.name, value: n.id }))
)

const selectedMappings = reactive<Record<string, string | null>>({})
const savingAlias = reactive<Record<string, boolean>>({})

const confirmAlias = async (item: ExtractedNutrientDto) => {
  const nutrientId = selectedMappings[item.name]
  if (!nutrientId) return

  savingAlias[item.name] = true
  try {
    await saveNutrientAlias(item.name, nutrientId)
    emit('alias-confirmed', item.name, nutrientId)
    message.success(`"${item.name}" associato correttamente`)
  } catch {
    message.error(`Errore nel salvataggio alias per "${item.name}"`)
  } finally {
    savingAlias[item.name] = false
  }
}
</script>
