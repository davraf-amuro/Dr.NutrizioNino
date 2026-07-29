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
import annotationPlugin from 'chartjs-plugin-annotation'
import { Bar } from 'vue-chartjs'
import type { DailySimulationDetailDto } from '@/Interfaces/dailySimulations/DailySimulationDto'
import type { NutritionalTargetDto } from '@/Interfaces/nutritionalTarget/NutritionalTargetDto'
import { getTargetValue } from '@/modules/nutritionalTarget/targetMapping'
import { sortNutrients } from '@/core/utils/sortNutrients'
import { getChartPreferences, updateChartPreferences } from '@/modules/userPreferences/api/userPreferences.api'

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend, annotationPlugin)

const props = defineProps<{ simulation: DailySimulationDetailDto; target: NutritionalTargetDto | null }>()
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

// ── Visibilità nutrienti (preferenze utente, fallback ai 5 default) ─────────
const DEFAULT_NUTRIENTS = ['Energia', 'Grassi', 'Carboidrati', 'Fibre', 'Proteine']
const visibleNutrients = ref<Set<string>>(new Set())

// Inizializza dai default locali (senza attendere API)
const initFromDefaults = () => {
  const available = new Set(allNutrientColumns.value.map((c) => c.name))
  const defaults = DEFAULT_NUTRIENTS.filter((n) => available.has(n))
  visibleNutrients.value = new Set(
    defaults.length > 0 ? defaults : allNutrientColumns.value.map((c) => c.name)
  )
}

// Carica preferenze dal backend e aggiorna il set
const loadPreferences = async () => {
  try {
    const saved = await getChartPreferences()
    const available = new Set(allNutrientColumns.value.map((c) => c.name))
    const matching = saved.filter((n) => available.has(n))
    if (matching.length > 0) {
      visibleNutrients.value = new Set(matching)
    } else {
      initFromDefaults()
    }
  } catch {
    initFromDefaults()
  }
}

// Chiamato quando il modal viene aperto (immediate perché il componente è montato con v-if già true)
watch(() => show.value, async (val) => {
  if (!val) return
  initFromDefaults()
  await loadPreferences()
}, { immediate: true })

const toggleNutrient = async (name: string) => {
  const s = new Set(visibleNutrients.value)
  s.has(name) ? s.delete(name) : s.add(name)
  visibleNutrients.value = s
  try {
    await updateChartPreferences([...s])
  } catch {
    // Salvataggio non bloccante
  }
}

// ── Dati grafico ─────────────────────────────────────────────
const visibleCols = computed(() => allNutrientColumns.value.filter((c) => visibleNutrients.value.has(c.name)))

// Etichette asse X = nutrienti selezionati; ogni barra impila i pasti (somma giornaliera)
const labels = computed(() => visibleCols.value.map((c) => `${c.name} (${c.unit})`))

// Palette colori ciclica — un colore fisso per pasto, coerente su tutti i nutrienti
const COLORS = [
  '#4e79a7', '#f28e2b', '#e15759', '#76b7b2', '#59a14f',
  '#edc948', '#b07aa1', '#ff9da7', '#9c755f', '#bab0ac'
]

const chartData = computed(() => ({
  labels: labels.value,
  datasets: props.simulation.sections.map((section, i) => ({
    label: section.sectionName,
    backgroundColor: COLORS[i % COLORS.length] + 'cc',
    borderColor: COLORS[i % COLORS.length],
    borderWidth: 1,
    data: visibleCols.value.map((col) => {
      return section.entries.reduce((sum, entry) => {
        const n = entry.nutrients.find((x) => x.name === col.name)
        return sum + (n?.quantity ?? 0)
      }, 0)
    })
  }))
}))

// Tacca soglia per nutriente con fabbisogno impostato: linea corta sopra la barra corrispondente,
// non un'area — kcal e grammi hanno scale troppo diverse per una banda continua leggibile.
const targetAnnotations = computed(() => {
  const annotations: Record<string, object> = {}
  visibleCols.value.forEach((col, index) => {
    const value = getTargetValue(props.target, col.name)
    if (value === null) return
    annotations[`target-${col.name}`] = {
      type: 'line',
      xMin: index - 0.4,
      xMax: index + 0.4,
      yMin: value,
      yMax: value,
      borderColor: '#666',
      borderWidth: 2,
      borderDash: [4, 3]
    }
  })
  return annotations
})

const chartOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: { position: 'bottom' as const },
    title: { display: false },
    annotation: { annotations: targetAnnotations.value }
  },
  scales: {
    x: { stacked: true },
    y: { stacked: true, beginAtZero: true }
  }
}))

</script>
