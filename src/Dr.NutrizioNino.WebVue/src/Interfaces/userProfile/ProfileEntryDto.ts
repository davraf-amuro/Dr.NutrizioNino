/** Misurazione di profilo registrata dall'utente. */
export interface ProfileEntryDto {
  id: string
  recordedAt: string
  weightKg: number | null
  idealWeightKg: number | null
  heightCm: number | null
  sex: string | null
  job: string | null
}

/** Body di creazione di una nuova misurazione: ogni campo è opzionale. */
export interface AddProfileEntryRequest {
  weightKg: number | null
  idealWeightKg: number | null
  heightCm: number | null
  sex: string | null
  job: string | null
}
