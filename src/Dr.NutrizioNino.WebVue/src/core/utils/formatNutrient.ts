export function formatNutrient(v: number, decimals = 3): string {
  return v === 0 ? '—' : v % 1 === 0 ? String(v) : v.toFixed(decimals).replace(/\.?0+$/, '')
}
