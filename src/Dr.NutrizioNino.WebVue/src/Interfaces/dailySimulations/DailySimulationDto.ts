export interface DailySimulationListItemDto {
  id: string
  name: string
  createdAt: string
  entryCount: number
}

export interface DailySimulationEntryNutrientDto {
  name: string
  positionOrder: number
  quantity: number
  unitAbbreviation: string
}

export interface DailySimulationEntryDto {
  id: string
  sourceName: string
  sourceType: number // 0=Food 1=Dish
  quantityGrams: number
  nutrients: DailySimulationEntryNutrientDto[]
}

export interface DailySimulationSectionDto {
  sectionId: string
  sectionName: string
  entries: DailySimulationEntryDto[]
}

export interface DailySimulationDetailDto {
  id: string
  name: string
  createdAt: string
  sections: DailySimulationSectionDto[]
}

export interface SimulationCompareNutrientDto {
  name: string
  positionOrder: number
  sim1Quantity: number | null
  sim2Quantity: number | null
  unitAbbreviation: string
}

export interface DailySimulationCompareDto {
  sim1Id: string
  sim1Name: string
  sim2Id: string
  sim2Name: string
  nutrients: SimulationCompareNutrientDto[]
}

// ── Richieste ──────────────────────────────────────────────

export interface AddSimulationEntryRequest {
  sectionId: string
  sourceType: number
  sourceId: string
  quantityGrams: number
}
