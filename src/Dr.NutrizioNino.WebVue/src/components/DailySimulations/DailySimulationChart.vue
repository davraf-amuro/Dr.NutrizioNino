<template>
  <n-modal v-model:show="show" preset="card" title="Grafico nutrienti" style="width: min(95vw, 900px)">
    <!-- Selezione nutrienti da visualizzare -->
    <n-space vertical size="small">
      <n-space wrap size="small">
        <n-tag
          v-for="col in allNutrientColumns"
          :key="col.name"
          :type="visibleNutrients.has(col.name) ? 'primary' : 'default'"
          checkable
          :checked="visibleNutrients.has(col.name)"
          @update:checked="toggleNutrient(col.name)"
        >
          {{ col.name }}
        </n-tag>
      </n-space>

      <div style="position: relative; height: 380px">
        <Bar :data="chartData" :options="chartOptions" />
      </div>
    </n-space>

    <template #footer>
      <n-space justify="end">
        <n-button @click="show = false">Chiudi</n-button>
      </n-space>
    </template>
  </n-modal>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { NButton, NModal, NSpace, NTag } from 'naive-ui'
import {
  Chart as ChartJS,
  CategoryScale, LinearScale, BarElement,
  Title, Tooltip, Legend
} from 'chart.js'
import { Bar } from 'vue-chartjs'
import type { DailySimulationDetailDto } from '@/Interfaces/dailySimulations/DailySimulationDto'
import { sortNutrients } from '@/core/utils/sortNutrients'

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend)

const props = defineProps<{ simulation: DailySimulationDetailDto }>()
const show = defineModel<boolean>('show', { default: false })

// ── Colonne nutrienti ────────────────────────────────────────
interface NutrientColumn { name: string; positionOrder: number; unit: string }

const allNutrientColumns = computed<NutrientColumn[]>(() => {
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

// ── Visibilità nutrienti (tutti visibili di default) ─────────
const visibleNutrients = ref<Set<string>>(new Set())

// Inizializza con tutti i nutrienti alla prima apertura
const initVisible = () => {
  visibleNutrients.value = new Set(allNutrientColumns.value.map((c) => c.name))
}

// Chiamato quando il modal viene aperto
watch(() => show.value, (val) => { if (val) initVisible() })

const toggleNutrient = (name: string) => {
  const s = new Set(visibleNutrients.value)
  s.has(name) ? s.delete(name) : s.add(name)
  visibleNutrients.value = s
}

// ── Dati grafico ─────────────────────────────────────────────
// Etichette asse X = sezioni nell'ordine enum (già ordinate dal backend)
const labels = computed(() => props.simulation.sections.map((s) => s.sectionName))

// Palette colori ciclica
const COLORS = [
  '#4e79a7', '#f28e2b', '#e15759', '#76b7b2', '#59a14f',
  '#edc948', '#b07aa1', '#ff9da7', '#9c755f', '#bab0ac'
]

const chartData = computed(() => {
  const visibleCols = allNutrientColumns.value.filter((c) => visibleNutrients.value.has(c.name))
  return {
    labels: labels.value,
    datasets: visibleCols.map((col, i) => ({
      label: `${col.name} (${col.unit})`,
      backgroundColor: COLORS[i % COLORS.length] + 'cc',
      borderColor: COLORS[i % COLORS.length],
      borderWidth: 1,
      data: props.simulation.sections.map((section) => {
        return section.entries.reduce((sum, entry) => {
          const n = entry.nutrients.find((x) => x.name === col.name)
          return sum + (n?.quantity ?? 0)
        }, 0)
      })
    }))
  }
})

const chartOptions = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { position: 'bottom' as const },
    title: { display: false }
  },
  scales: {
    x: { stacked: false },
    y: { beginAtZero: true }
  }
}

</script>
