export interface ExtractedNutrientDto {
  name: string
  value: number
  unit: string
  convertedValue: number | null
  canonicalUnit: string | null
  matchedNutrientId: string | null
  confidenceScore: number
}
