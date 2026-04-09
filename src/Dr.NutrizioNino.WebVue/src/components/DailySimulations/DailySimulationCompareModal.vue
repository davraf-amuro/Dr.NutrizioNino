<template>
  <n-modal
    :show="show"
    @update:show="$emit('close')"
    preset="card"
    title="Confronto simulazioni"
    style="width: 90%; max-width: 900px"
  >
    <n-spin :show="loading">
      <div v-if="!loading && compareData">
        <div class="compare-wrapper">
          <table class="compare-table">
            <thead>
              <tr>
                <th class="nutrient-col">Nutriente</th>
                <th>{{ compareData.sim1Name }}</th>
                <th>{{ compareData.sim2Name }}</th>
                <th>Delta</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in sortedNutrients" :key="row.name">
                <td class="nutrient-col">
                  {{ row.name }}
                  <span class="unit-abbr">({{ row.unitAbbreviation }})</span>
                </td>
                <td :class="cellClass(row, 'sim1')">
                  <span v-if="row.sim1Quantity !== null">{{ formatQty(row.sim1Quantity) }}</span>
                  <span v-else class="na">—</span>
                </td>
                <td :class="cellClass(row, 'sim2')">
                  <span v-if="row.sim2Quantity !== null">{{ formatQty(row.sim2Quantity) }}</span>
                  <span v-else class="na">—</span>
                </td>
                <td :class="deltaClass(row)">
                  <span v-if="row.sim1Quantity !== null && row.sim2Quantity !== null">
                    {{ formatDelta(row) }}
                  </span>
                  <span v-else class="na">—</span>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
      <div v-else-if="!loading" style="text-align:center; padding: 24px">
        Nessun dato da confrontare.
      </div>
    </n-spin>
  </n-modal>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NModal, NSpin, useThemeVars } from 'naive-ui'
const themeVars = useThemeVars()
const cellMaxBg = computed(() => themeVars.value.successColor + '33')
const cellMinBg = computed(() => themeVars.value.errorColor + '33')
import type { DailySimulationCompareDto, SimulationCompareNutrientDto } from '@/Interfaces/dailySimulations/DailySimulationDto'
import { compareSimulations } from '@/modules/dailySimulations/api/dailySimulations.api'
import { sortNutrients } from '@/core/utils/sortNutrients'

const props = defineProps<{
  show: boolean
  sim1Id: string
  sim2Id: string
}>()

defineEmits<{ close: [] }>()

const loading = ref(false)
const compareData = ref<DailySimulationCompareDto | null>(null)

const sortedNutrients = computed<SimulationCompareNutrientDto[]>(() => {
  if (!compareData.value) return []
  return sortNutrients(compareData.value.nutrients)
})

watch(
  () => props.show,
  async (visible) => {
    if (!visible || !props.sim1Id || !props.sim2Id) return
    loading.value = true
    compareData.value = null
    try {
      compareData.value = await compareSimulations(props.sim1Id, props.sim2Id)
    } finally {
      loading.value = false
    }
  }
)

function formatQty(v: number): string {
  return v % 1 === 0 ? String(v) : v.toFixed(2)
}

function formatDelta(row: SimulationCompareNutrientDto): string {
  const delta = (row.sim2Quantity ?? 0) - (row.sim1Quantity ?? 0)
  const sign = delta > 0 ? '+' : ''
  return `${sign}${delta.toFixed(2)}`
}

function cellClass(row: SimulationCompareNutrientDto, which: 'sim1' | 'sim2'): string {
  const v1 = row.sim1Quantity
  const v2 = row.sim2Quantity
  if (v1 === null || v2 === null) return ''
  if (which === 'sim1') return v1 > v2 ? 'cell-max' : v1 < v2 ? 'cell-min' : ''
  return v2 > v1 ? 'cell-max' : v2 < v1 ? 'cell-min' : ''
}

function deltaClass(row: SimulationCompareNutrientDto): string {
  if (row.sim1Quantity === null || row.sim2Quantity === null) return ''
  const delta = row.sim2Quantity - row.sim1Quantity
  if (delta > 0) return 'cell-max'
  if (delta < 0) return 'cell-min'
  return ''
}
</script>

<style scoped>
.compare-wrapper { overflow-x: auto; }

.compare-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.compare-table th,
.compare-table td {
  padding: 8px 12px;
  border: 1px solid v-bind('themeVars.dividerColor');
  text-align: right;
  color: v-bind('themeVars.textColor1');
  background: v-bind('themeVars.tableColor');
}

.compare-table th {
  background: v-bind('themeVars.tableHeaderColor');
  font-weight: 600;
  text-align: center;
}

.nutrient-col {
  text-align: left !important;
  white-space: nowrap;
  min-width: 160px;
}

.unit-abbr { color: v-bind('themeVars.textColor3'); font-size: 11px; margin-left: 4px; }
.cell-max { background: v-bind(cellMaxBg); }
.cell-min { background: v-bind(cellMinBg); }
.na { color: v-bind('themeVars.textColor3'); }
</style>
