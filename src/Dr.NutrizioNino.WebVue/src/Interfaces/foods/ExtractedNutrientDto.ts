export type ExtractionStatus = 'Matched' | 'IncompleteMatch' | 'Unrecognized'

export interface ExtractedNutrientDto {
  name: string
  value: number
  unit: string
  convertedValue: number | null
  canonicalUnit: string | null
  matchedNutrientId: string | null
  confidenceScore: number
  status: ExtractionStatus
}

export interface ExtractionResultDto {
  nutrients: ExtractedNutrientDto[]
  rawJson: string
}
