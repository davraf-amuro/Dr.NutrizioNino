<template>
  <div v-if="incompleteItems.length > 0 || unrecognizedItems.length > 0">

    <!-- Caso A: nutriente riconosciuto ma UoM/valore mancante o discordante -->
    <n-alert
      v-if="incompleteItems.length > 0"
      type="warning"
      title="Nutrienti con dati incompleti"
      style="margin-bottom: 8px; font-size: 12px"
      :show-icon="true"
      aria-live="polite"
    >
      <div
        v-for="item in incompleteItems"
        :key="item.name"
        style="display: flex; align-items: center; gap: 6px; flex-wrap: wrap; margin: 4px 0"
      >
        ⚠️ <strong>{{ item.name }}</strong>:
        {{ item.value }} {{ item.unit }}
        <span>— {{ discrepancyLabel(item) }}</span>
        <n-tag
          :type="confidenceTagType(item.confidenceScore)"
          size="small"
          :aria-label="`Confidenza ${confidenceLabel(item.confidenceScore)}: ${Math.round(item.confidenceScore * 100)}%`"
        >{{ Math.round(item.confidenceScore * 100) }}%</n-tag>
        <n-button
          v-if="item.unit && !unitExists(item.unit)"
          size="small"
          secondary
          @click="onAddUnit(item)"
          :aria-label="`Crea unità di misura '${item.unit}' per ${item.name}`"
        >
          + Crea unità "{{ item.unit }}"
        </n-button>
      </div>
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
        <n-button
          size="small"
          secondary
          @click="onAddNutrient(item)"
          :aria-label="`Aggiungi '${item.name}' come nuovo nutriente`"
        >
          + Nuovo
        </n-button>
      </div>
    </n-alert>

    <NutrientQuickAddModal
      v-model:show="showNutrientModal"
      :units-of-measures="unitsOfMeasures"
      :suggested-name="pendingNutrientName ?? undefined"
      :suggested-unit="pendingNutrientUnit ?? undefined"
      @created="onNutrientQuickCreated"
    />

    <UnitQuickAddModal
      v-model:show="showUnitModal"
      :suggested-abbreviation="pendingUnitAbbreviation ?? undefined"
      @created="onUnitQuickCreated"
    />
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { NAlert, NButton, NSelect, NTag, type SelectOption } from 'naive-ui'
import { useMessage } from 'naive-ui'
import type { ExtractedNutrientDto } from '@/Interfaces/foods/ExtractedNutrientDto'
import type { Nutrient } from '@/Interfaces/Nutrients/Nutrient'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import { saveNutrientAlias } from '@/modules/foods/api/foods.api'
import NutrientQuickAddModal from './NutrientQuickAddModal.vue'
import UnitQuickAddModal from './UnitQuickAddModal.vue'

const props = defineProps<{
  extractedNutrients: ExtractedNutrientDto[]
  availableNutrients: Nutrient[]
  unitsOfMeasures: UnitOfMeasureDto[]
}>()

const emit = defineEmits<{
  'alias-confirmed': [aiName: string, nutrientId: string]
  'nutrient-created': [nutrient: Nutrient, unitOfMeasureId: string]
  'unit-created': [unit: UnitOfMeasureDto, nutrientId: string]
}>()

const message = useMessage()

const showNutrientModal = ref(false)
const pendingNutrientName = ref<string | null>(null)
const pendingNutrientUnit = ref<string | null>(null)

const showUnitModal = ref(false)
const pendingUnitAbbreviation = ref<string | null>(null)
const pendingUnitForNutrientId = ref<string | null>(null)

const onAddNutrient = (item: ExtractedNutrientDto) => {
  pendingNutrientName.value = item.name
  pendingNutrientUnit.value = item.unit
  showNutrientModal.value = true
}

const onNutrientQuickCreated = (payload: { nutrient: Nutrient; unitOfMeasureId: string }) => {
  emit('nutrient-created', payload.nutrient, payload.unitOfMeasureId)
  if (pendingNutrientName.value) {
    selectedMappings[pendingNutrientName.value] = payload.nutrient.id
  }
  pendingNutrientName.value = null
  pendingNutrientUnit.value = null
}

const onAddUnit = (item: ExtractedNutrientDto) => {
  pendingUnitAbbreviation.value = item.unit
  pendingUnitForNutrientId.value = item.matchedNutrientId
  showUnitModal.value = true
}

const onUnitQuickCreated = (unit: UnitOfMeasureDto) => {
  if (pendingUnitForNutrientId.value) {
    emit('unit-created', unit, pendingUnitForNutrientId.value)
    message.success(`Unità "${unit.abbreviation}" creata e assegnata`)
  }
  pendingUnitAbbreviation.value = null
  pendingUnitForNutrientId.value = null
}

const confidenceTagType = (score: number) =>
  score >= 0.85 ? 'success' : score >= 0.60 ? 'warning' : 'error'

const confidenceLabel = (score: number) =>
  score >= 0.85 ? 'alta' : score >= 0.60 ? 'media' : 'bassa'

// Abbreviazione canonica attesa per il nutriente riconosciuto
const expectedAbbreviation = (item: ExtractedNutrientDto): string => {
  const nutrient = props.availableNutrients.find((n) => n.id === item.matchedNutrientId)
  if (!nutrient) return ''
  const uom = props.unitsOfMeasures.find((u) => u.id === nutrient.defaultUnitOfMeasureId)
  return uom?.abbreviation ?? ''
}

// Messaggio specifico sulla discrepanza: cosa manca o cosa non combacia
const discrepancyLabel = (item: ExtractedNutrientDto): string => {
  if (!item.unit) return 'unità di misura non riconoscibile'
  if (item.value === 0) return 'valore non estratto'
  const expected = expectedAbbreviation(item)
  if (expected) return `atteso: ${expected}, trovato: ${item.unit}`
  return `unità "${item.unit}" non riconosciuta`
}

const unitExists = (unit: string): boolean =>
  props.unitsOfMeasures.some((u) => u.abbreviation?.toLowerCase() === unit.trim().toLowerCase())

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
