<template>
  <n-modal :show="show" @update:show="$emit('close')" preset="card" title="Confronto alimenti" style="width: 90%; max-width: 1000px">
    <n-spin :show="loading">
      <div v-if="!loading && rows.length === 0" style="text-align:center; padding: 24px">
        Nessun nutriente da confrontare.
      </div>
      <div v-else class="compare-wrapper">
        <table class="compare-table">
          <thead>
            <tr>
              <th class="nutrient-col">Nutriente</th>
              <th v-for="food in foodDetails" :key="food.id">{{ food.name }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in rows" :key="row.nutrientId">
              <td class="nutrient-col">
                {{ row.name }}
                <span v-if="row.unitAbbreviation" class="unit-abbr">({{ row.unitAbbreviation }})</span>
              </td>
              <td
                v-for="food in foodDetails"
                :key="food.id"
                :class="cellClass(row, food.id)"
              >
                <span v-if="getValue(row, food.id) !== null">
                  {{ getValue(row, food.id) }}
                  <span class="arrow" v-if="isMax(row, food.id)">▲</span>
                  <span class="arrow arrow-min" v-else-if="isMin(row, food.id)">▼</span>
                </span>
                <span v-else class="na">—</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </n-spin>
  </n-modal>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { NModal, NSpin } from 'naive-ui'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'
import { getFoodById } from '@/modules/foods/api/foods.api'
import { getUnitsOfMeasures } from '@/modules/units/api/units.api'
import { sortNutrients } from '@/core/utils/sortNutrients'

interface NutrientRow {
  nutrientId: string
  name: string
  positionOrder: number
  unitAbbreviation: string
  values: Record<string, number | null>
}

const props = defineProps<{
  show: boolean
  foods: FoodDashboardDto[]
}>()

defineEmits<{ close: [] }>()

const loading = ref(false)
const foodDetails = ref<FoodDto[]>([])
const rows = ref<NutrientRow[]>([])

watch(
  () => props.show,
  async (visible) => {
    if (!visible) return
    loading.value = true
    foodDetails.value = []
    rows.value = []

    const [details, units] = await Promise.all([
      Promise.all(props.foods.map((f) => getFoodById(f.id))),
      getUnitsOfMeasures()
    ])
    foodDetails.value = details

    const unitMap = new Map(units.map((u) => [u.id, u.abbreviation]))
    const nutrientMap = new Map<string, NutrientRow>()

    for (const food of details) {
      for (const n of food.nutrients ?? []) {
        if (!nutrientMap.has(n.nutrientId)) {
          nutrientMap.set(n.nutrientId, {
            nutrientId: n.nutrientId,
            name: n.name,
            positionOrder: n.positionOrder,
            unitAbbreviation: unitMap.get(n.unitOfMeasureId) ?? '',
            values: {}
          })
        }
        nutrientMap.get(n.nutrientId)!.values[food.id] = n.quantity
      }
    }

    for (const row of nutrientMap.values()) {
      for (const food of details) {
        if (!(food.id in row.values)) row.values[food.id] = null
      }
    }

    rows.value = sortNutrients([...nutrientMap.values()])
    loading.value = false
  }
)

function getValue(row: NutrientRow, foodId: string): number | null {
  return row.values[foodId] ?? null
}

function allValues(row: NutrientRow): number[] {
  return Object.values(row.values).filter((v): v is number => v !== null)
}

function isMax(row: NutrientRow, foodId: string): boolean {
  const v = getValue(row, foodId)
  if (v === null) return false
  const vals = allValues(row)
  if (vals.length < 2) return false
  return v === Math.max(...vals)
}

function isMin(row: NutrientRow, foodId: string): boolean {
  const v = getValue(row, foodId)
  if (v === null) return false
  const vals = allValues(row)
  if (vals.length < 2) return false
  return v === Math.min(...vals)
}

function cellClass(row: NutrientRow, foodId: string): string {
  if (isMax(row, foodId)) return 'cell-max'
  if (isMin(row, foodId)) return 'cell-min'
  return ''
}
</script>

<style scoped>
.compare-wrapper {
  overflow-x: auto;
}

.compare-table {
  width: 100%;
  border-collapse: collapse;
  font-size: 13px;
}

.compare-table th,
.compare-table td {
  padding: 8px 12px;
  border: 1px solid #e0e0e0;
  text-align: right;
}

.compare-table th {
  background: #f5f5f5;
  font-weight: 600;
  text-align: center;
}

.nutrient-col {
  text-align: left !important;
  white-space: nowrap;
  min-width: 160px;
}

.unit-abbr {
  color: #999;
  font-size: 11px;
  margin-left: 4px;
}

.cell-max {
  background: #f0fff4;
}

.cell-min {
  background: #fff5f5;
}

.arrow {
  margin-left: 4px;
  color: #18a058;
  font-size: 11px;
}

.arrow-min {
  color: #d03050;
}

.na {
  color: #ccc;
}
</style>
