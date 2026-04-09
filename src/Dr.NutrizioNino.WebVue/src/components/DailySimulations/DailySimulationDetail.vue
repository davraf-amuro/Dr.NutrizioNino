<template>
  <n-space vertical size="medium">
    <!-- Header: nome modificabile + azioni -->
    <div class="sim-header">
      <n-input
        v-model:value="editableName"
        :loading="isSavingName"
        style="font-size: 1.1em; font-weight: 600"
        @blur="handleSaveName"
        @keyup.enter="handleSaveName"
      />
      <n-space size="small" style="flex-shrink: 0">
        <n-button size="small" @click="emit('compare')">Confronta</n-button>
        <n-button size="small" @click="showChart = true">Grafico</n-button>
        <n-button size="small" @click="emit('close')">Chiudi</n-button>
      </n-space>
    </div>

    <!-- Aggiungi voce (IN ALTO) -->
    <n-card size="small" title="Aggiungi voce">
      <n-grid :cols="2" :x-gap="12" :y-gap="8">
        <n-gi :span="2">
          <n-form-item label="Sezione">
            <n-select v-model:value="newEntry.sectionId" :options="sectionOptions" />
          </n-form-item>
        </n-gi>
        <n-gi>
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
          <n-form-item label="Tipo">
            <n-radio-group v-model:value="newEntry.sourceType">
              <n-radio :value="0">Alimento</n-radio>
              <n-radio :value="1">Piatto</n-radio>
            </n-radio-group>
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
    </n-card>

    <!-- Griglia padre-figlio -->
    <div class="sim-table-wrapper">
      <table class="sim-table">
        <thead>
          <tr>
            <th class="col-name">Nome</th>
            <th class="col-qty">Qtà (g)</th>
            <th v-for="col in nutrientColumns" :key="col.name" class="col-nutrient">
              {{ col.name }}<br /><small>{{ col.unit }}</small>
            </th>
            <th class="col-actions">Azioni</th>
          </tr>
        </thead>
        <tbody>
          <template v-for="section in simulation.sections" :key="section.sectionId">
            <!-- Riga sezione (padre) -->
            <tr class="section-row" @click="toggleSection(section.sectionId)">
              <td :colspan="2 + nutrientColumns.length">
                <span class="section-toggle">{{ collapsed[section.sectionId] ? '▶' : '▼' }}</span>
                <strong>{{ section.sectionName }}</strong>
                <span class="section-count">({{ section.entries.length }})</span>
              </td>
              <td class="col-actions">
                <n-button
                  size="tiny"
                  type="primary"
                  tertiary
                  :aria-label="`Aggiungi voce in ${section.sectionName}`"
                  @click.stop="selectSection(section.sectionId)"
                >+</n-button>
              </td>
            </tr>

            <!-- Righe figlie (entries) -->
            <template v-if="!collapsed[section.sectionId]">
              <tr v-for="entry in section.entries" :key="entry.id" class="entry-row">
                <td>{{ entry.sourceName }}</td>
                <td class="col-qty">
                  <n-input-number
                    :value="entry.quantityGrams"
                    :min="0.1"
                    :precision="1"
                    size="small"
                    :show-button="false"
                    style="width: 70px"
                    @update:value="(v) => v && handleUpdateQuantity(entry.id, v)"
                  />
                </td>
                <td v-for="col in nutrientColumns" :key="col.name" class="col-nutrient">
                  {{ formatNutrient(getNutrientValue(entry, col.name)) }}
                </td>
                <td class="col-actions">
                  <n-button size="tiny" type="error" tertiary @click="handleDeleteEntry(entry.id)">✕</n-button>
                </td>
              </tr>

              <!-- Footer sezione -->
              <tr class="section-footer-row">
                <td><em>Totale {{ section.sectionName }}</em></td>
                <td class="col-qty">{{ formatNutrient(sectionQtyTotal(section)) }}</td>
                <td v-for="col in nutrientColumns" :key="col.name" class="col-nutrient">
                  {{ formatNutrient(sectionNutrientTotal(section, col.name)) }}
                </td>
                <td></td>
              </tr>
            </template>
          </template>

          <!-- Totale giornata -->
          <tr class="daily-total-row">
            <td><strong>Totale giornata</strong></td>
            <td class="col-qty"><strong>{{ formatNutrient(dailyQtyTotal) }}</strong></td>
            <td v-for="col in nutrientColumns" :key="col.name" class="col-nutrient">
              <strong>{{ formatNutrient(dailyNutrientTotal(col.name)) }}</strong>
            </td>
            <td></td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Modal grafico -->
    <DailySimulationChart
      v-if="showChart"
      v-model:show="showChart"
      :simulation="simulation"
    />
  </n-space>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import {
  NButton, NCard, NFormItem, NGi, NGrid, NInput, NInputNumber,
  NRadio, NRadioGroup, NSelect, NSpace, useThemeVars, type SelectOption
} from 'naive-ui'
import type { DailySimulationDetailDto, DailySimulationEntryDto, DailySimulationSectionDto } from '@/Interfaces/dailySimulations/DailySimulationDto'
import { getFoodsDashboard } from '@/modules/foods/api/foods.api'
import { getDishesDashboard } from '@/modules/dishes/api/dishes.api'
import { addEntry, updateEntryQuantity, deleteEntry, renameSimulation } from '@/modules/dailySimulations/api/dailySimulations.api'
import type { AddSimulationEntryRequest } from '@/Interfaces/dailySimulations/DailySimulationDto'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import { sortNutrients } from '@/core/utils/sortNutrients'
import { useSectionConfigs } from '@/modules/sectionConfigs/composables/useSectionConfigs'
import DailySimulationChart from './DailySimulationChart.vue'

const themeVars = useThemeVars()

const props = defineProps<{ simulation: DailySimulationDetailDto }>()
const emit = defineEmits<{
  close: []
  refresh: []
  compare: []
}>()

// ── Nome modificabile ───────────────────────────────────────
const editableName = ref(props.simulation.name)
const isSavingName = ref(false)

watch(() => props.simulation.name, (v) => { editableName.value = v })

const handleSaveName = async () => {
  const name = editableName.value.trim()
  if (!name || name === props.simulation.name) return
  isSavingName.value = true
  try {
    await renameSimulation(props.simulation.id, name)
    emit('refresh')
  } finally {
    isSavingName.value = false
  }
}

// ── Aggiunta voce ────────────────────────────────────────────
const isAdding = ref(false)
const showChart = ref(false)

const newEntry = reactive<AddSimulationEntryRequest>({
  sectionId: '',
  sourceType: 0,
  sourceId: '',
  quantityGrams: 100
})

const { sectionOptions, loadSectionConfigs } = useSectionConfigs()

const foodData = ref<FoodDashboardDto[]>([])
const dishData = ref<FoodDashboardDto[]>([])
const foodOptions = ref<SelectOption[]>([])
const dishOptions = ref<SelectOption[]>([])

const sourceOptions = computed<SelectOption[]>(() =>
  newEntry.sourceType === 0 ? foodOptions.value : dishOptions.value
)

onMounted(async () => {
  const [foods, dishes] = await Promise.all([getFoodsDashboard(), getDishesDashboard(), loadSectionConfigs()])
  foodData.value = foods
  dishData.value = dishes
  foodOptions.value = foods.map((f) => ({ label: f.name, value: f.id }))
  dishOptions.value = dishes.map((d) => ({ label: d.name, value: d.id }))
  // Pre-seleziona la prima sezione attiva
  if (!newEntry.sectionId && sectionOptions.value.length > 0) {
    newEntry.sectionId = String(sectionOptions.value[0].value ?? '')
  }
})

// Auto-popola la quantità default quando si seleziona un alimento o piatto
watch(() => newEntry.sourceId, (id) => {
  if (!id) return
  const data = newEntry.sourceType === 0 ? foodData.value : dishData.value
  const item = data.find((d) => d.id === id)
  if (item?.quantity) newEntry.quantityGrams = item.quantity
})

// Pulisce la selezione quando si cambia tipo
watch(() => newEntry.sourceType, () => {
  newEntry.sourceId = ''
  newEntry.quantityGrams = 100
})

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

// Pulsante + sulla riga sezione: pre-seleziona la sezione nel form
const selectSection = (sectionId: string) => {
  newEntry.sectionId = sectionId
  // Porta il focus al form "Aggiungi voce" scrollando in cima
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

// ── Collapse sezioni ─────────────────────────────────────────
const collapsed = reactive<Record<string, boolean>>({})

const toggleSection = (sectionId: string) => {
  collapsed[sectionId] = !collapsed[sectionId]
}

// ── Colonne nutrienti dinamiche ──────────────────────────────
interface NutrientColumn { name: string; positionOrder: number; unit: string }

const nutrientColumns = computed<NutrientColumn[]>(() => {
  const seen = new Map<string, NutrientColumn>()
  for (const section of props.simulation.sections) {
    for (const entry of section.entries) {
      for (const n of entry.nutrients) {
        if (!seen.has(n.name)) {
          seen.set(n.name, { name: n.name, positionOrder: n.positionOrder, unit: n.unitAbbreviation })
        }
      }
    }
  }
  return sortNutrients([...seen.values()])
})

// ── Helpers calcolo ──────────────────────────────────────────
const getNutrientValue = (entry: DailySimulationEntryDto, nutrientName: string): number => {
  const n = entry.nutrients.find((x) => x.name === nutrientName)
  return n?.quantity ?? 0
}

const formatNutrient = (v: number): string =>
  v === 0 ? '—' : v % 1 === 0 ? String(v) : v.toFixed(2).replace(/\.?0+$/, '')

const sectionQtyTotal = (section: DailySimulationSectionDto): number =>
  section.entries.reduce((sum, e) => sum + e.quantityGrams, 0)

const sectionNutrientTotal = (section: DailySimulationSectionDto, nutrientName: string): number =>
  section.entries.reduce((sum, e) => sum + getNutrientValue(e, nutrientName), 0)

const dailyQtyTotal = computed(() =>
  props.simulation.sections.reduce((sum, s) => sum + sectionQtyTotal(s), 0)
)

const dailyNutrientTotal = (nutrientName: string): number =>
  props.simulation.sections.reduce((sum, s) => sum + sectionNutrientTotal(s, nutrientName), 0)

// ── Handlers entry ────────────────────────────────────────────
const handleUpdateQuantity = async (entryId: string, quantityGrams: number) => {
  await updateEntryQuantity(props.simulation.id, entryId, quantityGrams)
  emit('refresh')
}

const handleDeleteEntry = async (entryId: string) => {
  await deleteEntry(props.simulation.id, entryId)
  emit('refresh')
}
</script>

<style scoped>
.sim-table-wrapper {
  overflow-x: auto;
  border-radius: 6px;
  border: 1px solid v-bind('themeVars.dividerColor');
}

.sim-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
  background: v-bind('themeVars.tableColor');
  color: v-bind('themeVars.textColor1');
}

.sim-table th,
.sim-table td {
  padding: 6px 10px;
  border-bottom: 1px solid v-bind('themeVars.dividerColor');
  white-space: nowrap;
}

.sim-table thead th {
  background: v-bind('themeVars.tableHeaderColor');
  color: v-bind('themeVars.textColor1');
  font-weight: 600;
  text-align: left;
  position: sticky;
  top: 0;
  z-index: 1;
}

.col-qty { text-align: right; min-width: 90px; }
.col-nutrient { text-align: right; min-width: 80px; }
.col-actions { text-align: center; min-width: 60px; }
.col-name { min-width: 160px; }

.section-row {
  background: v-bind('themeVars.actionColor');
  cursor: pointer;
  user-select: none;
}
.section-row:hover { filter: brightness(0.97); }
.section-toggle { margin-right: 8px; font-size: 11px; }
.section-count { margin-left: 6px; color: v-bind('themeVars.textColor3'); font-size: 12px; }

.entry-row:hover td { background: v-bind('themeVars.hoverColor'); }

.section-footer-row td {
  background: v-bind('themeVars.tableHeaderColor');
  color: v-bind('themeVars.textColor2');
  font-style: italic;
  border-top: 1px solid v-bind('themeVars.dividerColor');
}

.daily-total-row td {
  background: v-bind('themeVars.actionColor');
  color: v-bind('themeVars.textColor1');
  border-top: 2px solid v-bind('themeVars.dividerColor');
}

.sim-header {
  display: flex;
  align-items: center;
  gap: 12px;
}

.sim-header .n-input {
  flex: 1;
  min-width: 0;
}
</style>
