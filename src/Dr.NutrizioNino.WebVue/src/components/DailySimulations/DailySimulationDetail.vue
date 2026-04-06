<template>
  <n-space vertical size="medium">
    <!-- Header: nome + azioni -->
    <n-space align="center" justify="space-between">
      <n-h3 style="margin: 0">{{ simulation.name }}</n-h3>
      <n-space size="small">
        <n-button size="small" @click="showCompareSelect = !showCompareSelect">Confronta</n-button>
        <n-button size="small" @click="emit('close')">Chiudi</n-button>
      </n-space>
    </n-space>

    <!-- Totale nutrienti -->
    <n-card size="small" title="Totale giornata">
      <n-data-table
        v-if="totalNutrients.length > 0"
        :columns="totalColumns"
        :data="totalNutrients"
        :row-key="(row) => row.name"
        size="small"
        :single-line="false"
        :pagination="false"
      />
      <n-text v-else depth="3">Aggiungi voci per vedere i totali.</n-text>
    </n-card>

    <!-- Sezioni -->
    <div v-for="section in simulation.sections" :key="section.sectionType">
      <n-divider title-placement="left">{{ section.sectionName }}</n-divider>
      <n-data-table
        :columns="entryColumns(section.sectionType)"
        :data="section.entries"
        :row-key="(row) => row.id"
        size="small"
        :single-line="false"
        :pagination="false"
      />
    </div>

    <!-- Pannello aggiunta voce -->
    <n-divider title-placement="left">Aggiungi voce</n-divider>
    <n-grid :cols="2" :x-gap="12" :y-gap="8">
      <n-gi>
        <n-form-item label="Sezione">
          <n-select v-model:value="newEntry.sectionType" :options="sectionOptions" />
        </n-form-item>
      </n-gi>
      <n-gi>
        <n-form-item label="Tipo">
          <n-radio-group v-model:value="newEntry.sourceType">
            <n-radio :value="0">Alimento</n-radio>
            <n-radio :value="1">Piatto</n-radio>
          </n-radio-group>
        </n-form-item>
      </n-gi>
      <n-gi :span="2">
        <n-form-item label="Cerca">
          <n-select
            v-model:value="newEntry.sourceId"
            :options="sourceOptions"
            filterable
            clearable
            placeholder="Seleziona alimento o piatto..."
          />
        </n-form-item>
      </n-gi>
      <n-gi>
        <n-form-item label="Quantità (g)">
          <n-input-number
            v-model:value="newEntry.quantityGrams"
            :min="0.1"
            :precision="1"
            :show-button="false"
            style="width: 100%"
          />
        </n-form-item>
      </n-gi>
      <n-gi style="display: flex; align-items: flex-end; padding-bottom: 24px">
        <n-button
          type="primary"
          :loading="isAdding"
          :disabled="!newEntry.sourceId || !newEntry.quantityGrams"
          @click="handleAddEntry"
        >
          Aggiungi
        </n-button>
      </n-gi>
    </n-grid>
  </n-space>
</template>

<script setup lang="ts">
import { computed, h, onMounted, reactive, ref } from 'vue'
import {
  NButton, NCard, NDataTable, NDivider, NFormItem, NGi, NGrid,
  NH3, NInputNumber, NRadio, NRadioGroup, NSelect, NSpace, NText,
  type DataTableColumns, type SelectOption
} from 'naive-ui'
import type { DailySimulationDetailDto, DailySimulationEntryDto } from '@/Interfaces/dailySimulations/DailySimulationDto'
import { sortNutrients } from '@/core/utils/sortNutrients'
import { getFoodsDashboard } from '@/modules/foods/api/foods.api'
import { getDishesDashboard } from '@/modules/dishes/api/dishes.api'
import { addEntry, updateEntryQuantity, deleteEntry } from '@/modules/dailySimulations/api/dailySimulations.api'
import type { AddSimulationEntryRequest } from '@/Interfaces/dailySimulations/DailySimulationDto'

const props = defineProps<{ simulation: DailySimulationDetailDto }>()
const emit = defineEmits<{
  close: []
  refresh: []
  compare: []
}>()

const showCompareSelect = ref(false)
const isAdding = ref(false)

const newEntry = reactive<AddSimulationEntryRequest>({
  sectionType: 0,
  sourceType: 0,
  sourceId: '',
  quantityGrams: 100
})

// ── Totale nutrienti aggregato ──────────────────────────────
interface TotalNutrient { name: string; positionOrder: number; quantity: number; unitAbbreviation: string }

const totalNutrients = computed<TotalNutrient[]>(() => {
  const map = new Map<string, TotalNutrient>()
  for (const section of props.simulation.sections) {
    for (const entry of section.entries) {
      for (const n of entry.nutrients) {
        const existing = map.get(n.name)
        if (existing) {
          existing.quantity = Math.round((existing.quantity + n.quantity) * 10000) / 10000
        } else {
          map.set(n.name, { name: n.name, positionOrder: n.positionOrder, quantity: n.quantity, unitAbbreviation: n.unitAbbreviation })
        }
      }
    }
  }
  return sortNutrients([...map.values()])
})

const totalColumns: DataTableColumns<TotalNutrient> = [
  { title: 'Nutriente', key: 'name' },
  {
    title: 'Quantità',
    key: 'quantity',
    width: 140,
    render: (row) => `${row.quantity} ${row.unitAbbreviation}`
  }
]

// ── Colonne entry per sezione ───────────────────────────────
const entryColumns = (sectionType: number): DataTableColumns<DailySimulationEntryDto> => [
  { title: 'Nome', key: 'sourceName' },
  { title: 'Quantità (g)', key: 'quantityGrams', width: 120 },
  {
    title: 'Azioni',
    key: 'actions',
    width: 200,
    render: (row) =>
      h(NSpace, { size: 'small' }, {
        default: () => [
          h(NInputNumber, {
            value: row.quantityGrams,
            min: 0.1,
            precision: 1,
            size: 'small',
            showButton: false,
            style: 'width: 80px',
            onUpdateValue: (v: number | null) => v && handleUpdateQuantity(row.id, v)
          }),
          h(NButton, {
            size: 'small', type: 'error', tertiary: true,
            onClick: () => handleDeleteEntry(row.id)
          }, { default: () => '✕' })
        ]
      })
  }
]

// ── Selezione sorgente ──────────────────────────────────────
const sectionOptions: SelectOption[] = [
  { label: 'Colazione', value: 0 },
  { label: 'Pranzo', value: 1 },
  { label: 'Cena', value: 2 },
  { label: 'Spuntino', value: 3 },
  { label: 'Merenda', value: 4 },
  { label: 'Altro', value: 5 }
]

const foodOptions = ref<SelectOption[]>([])
const dishOptions = ref<SelectOption[]>([])

const sourceOptions = computed<SelectOption[]>(() =>
  newEntry.sourceType === 0 ? foodOptions.value : dishOptions.value
)

onMounted(async () => {
  const [foods, dishes] = await Promise.all([getFoodsDashboard(), getDishesDashboard()])
  foodOptions.value = foods.map((f) => ({ label: f.name, value: f.id }))
  dishOptions.value = dishes.map((d) => ({ label: d.name, value: d.id }))
})

// ── Handlers ────────────────────────────────────────────────
const handleAddEntry = async () => {
  if (!newEntry.sourceId || !newEntry.quantityGrams) return
  isAdding.value = true
  try {
    await addEntry(props.simulation.id, { ...newEntry })
    newEntry.sourceId = ''
    newEntry.quantityGrams = 100
    emit('refresh')
  } finally {
    isAdding.value = false
  }
}

const handleUpdateQuantity = async (entryId: string, quantityGrams: number) => {
  await updateEntryQuantity(props.simulation.id, entryId, quantityGrams)
  emit('refresh')
}

const handleDeleteEntry = async (entryId: string) => {
  await deleteEntry(props.simulation.id, entryId)
  emit('refresh')
}
</script>
