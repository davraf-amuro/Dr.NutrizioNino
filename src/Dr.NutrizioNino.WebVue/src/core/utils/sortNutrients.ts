/**
 * Ordina i nutrienti per positionOrder ascendente (0 = non classificato, va in fondo),
 * poi alfabeticamente per nome come fallback.
 */
export function sortNutrients<T extends { name: string; positionOrder?: number }>(nutrients: T[]): T[] {
  return [...nutrients].sort((a, b) => {
    const pa = a.positionOrder ?? 0
    const pb = b.positionOrder ?? 0
    if (pa !== pb) {
      if (pa === 0) return 1
      if (pb === 0) return -1
      return pa - pb
    }
    return a.name.localeCompare(b.name, 'it')
  })
}
