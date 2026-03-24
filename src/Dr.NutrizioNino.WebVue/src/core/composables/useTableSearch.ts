import { computed, ref } from 'vue'

export function useTableSearch<T>(getData: () => T[], searchField: keyof T) {
  const searchQuery = ref('')
  const filteredData = computed(() => {
    const q = searchQuery.value.trim().toLowerCase()
    if (!q) return getData()
    return getData().filter(item =>
      String(item[searchField]).toLowerCase().includes(q)
    )
  })
  return { searchQuery, filteredData }
}
