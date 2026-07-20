export interface NutritionalTargetDto {
  kcalTarget: number | null
  carbsTarget: number | null
  proteinTarget: number | null
  fatTarget: number | null
  updatedAt: string
}

export interface SetNutritionalTargetRequest {
  kcalTarget: number | null
  carbsTarget: number | null
  proteinTarget: number | null
  fatTarget: number | null
}
