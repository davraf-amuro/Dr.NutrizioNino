/** Scheda Sciaudone calcolata da una misurazione: stima generica, non una prescrizione nutrizionale. */
export interface SciaudoneCardDto {
  id: string
  profileEntryId: string
  computedAt: string
  weightKg: number
  idealWeightKg: number
  kcal: number
  proteinG: number
  fatG: number
  fiberG: number
  carbsG: number
}
