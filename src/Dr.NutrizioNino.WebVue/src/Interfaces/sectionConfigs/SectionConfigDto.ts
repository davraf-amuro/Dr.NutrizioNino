export interface SimulationSectionDto {
  id: string
  name: string
  displayOrder: number
  isActive: boolean
}

export interface SimulationSectionReorderItem {
  id: string
  displayOrder: number
}
