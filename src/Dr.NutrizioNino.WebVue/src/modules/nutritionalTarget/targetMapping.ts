import type { NutritionalTargetDto } from '@/Interfaces/nutritionalTarget/NutritionalTargetDto'

export type NumericTargetField = 'kcalTarget' | 'carbsTarget' | 'proteinTarget' | 'fatTarget'

// Nome nutriente (griglia/grafico) → campo target corrispondente
export const TARGET_FIELD_BY_NUTRIENT: Record<string, NumericTargetField> = {
  Energia: 'kcalTarget',
  Carboidrati: 'carbsTarget',
  Proteine: 'proteinTarget',
  Grassi: 'fatTarget'
}

// Valore target per il nome nutriente dato, null se non mappato o non impostato
export const getTargetValue = (target: NutritionalTargetDto | null, nutrientName: string): number | null => {
  const field = TARGET_FIELD_BY_NUTRIENT[nutrientName]
  const value = field ? target?.[field] : null
  return value ?? null
}
