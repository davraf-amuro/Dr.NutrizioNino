<template>
  <n-card title="Scheda Sciaudone" size="large">
    <n-space v-if="card" vertical size="medium">
      <n-grid :cols="2" :x-gap="12" :y-gap="12">
        <n-gi v-for="value in values" :key="value.label">
          <div class="sciaudone-value">
            <n-text depth="3" class="sciaudone-label">{{ value.label }}</n-text>
            <n-text class="sciaudone-number">{{ formatNumber(value.amount) }} {{ value.unit }}</n-text>
          </div>
        </n-gi>
      </n-grid>

      <n-text depth="3" class="sciaudone-meta">
        Calcolata il {{ formatDate(card.computedAt) }} su peso {{ formatNumber(card.weightKg) }} kg
        e peso ideale {{ formatNumber(card.idealWeightKg) }} kg.
      </n-text>

      <n-alert type="warning" :show-icon="true" :bordered="false">
        Stima generica calcolata con formule fisse: non sostituisce una consulenza nutrizionale.
      </n-alert>
    </n-space>

    <n-text v-else depth="3">
      Nessuna scheda disponibile. Registra una misurazione con peso attuale e peso ideale per generarla.
    </n-text>
  </n-card>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { NAlert, NCard, NGi, NGrid, NSpace, NText } from 'naive-ui'
import type { SciaudoneCardDto } from '@/Interfaces/userProfile/SciaudoneCardDto'

const props = defineProps<{
  card: SciaudoneCardDto | null
}>()

/** I cinque valori della scheda nell'ordine della specifica originale. */
const values = computed(() =>
  props.card
    ? [
        { label: 'Calorie', amount: props.card.kcal, unit: 'kcal' },
        { label: 'Proteine', amount: props.card.proteinG, unit: 'g' },
        { label: 'Grassi', amount: props.card.fatG, unit: 'g' },
        { label: 'Fibre', amount: props.card.fiberG, unit: 'g' },
        { label: 'Carboidrati', amount: props.card.carbsG, unit: 'g' }
      ]
    : []
)

/** Una cifra decimale, come i valori persistiti a database. */
const formatNumber = (value: number): string => value.toLocaleString('it-IT', { maximumFractionDigits: 1 })

const formatDate = (value: string): string => new Date(value).toLocaleDateString('it-IT')
</script>

<style scoped>
.sciaudone-value {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.sciaudone-label {
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.sciaudone-number {
  font-size: 18px;
  font-weight: 600;
}

.sciaudone-meta {
  font-size: 12px;
}
</style>
