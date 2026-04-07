import { ref, computed } from 'vue'
import type { SelectOption } from 'naive-ui'
import { sectionConfigsApi } from '@/modules/sectionConfigs/api/sectionConfigs.api'
import type { SimulationSectionDto, SimulationSectionReorderItem } from '@/Interfaces/sectionConfigs/SectionConfigDto'

// singleton a livello modulo — condiviso da tutti i composable che chiamano useSectionConfigs()
const sections = ref<SimulationSectionDto[]>([])
let lastLoaded = 0
const CACHE_TTL_MS = 60_000

function invalidate() {
  lastLoaded = 0
}

export function useSectionConfigs() {
  const activeSections = computed(() => sections.value.filter((s) => s.isActive))

  const sectionOptions = computed<SelectOption[]>(() =>
    activeSections.value.map((s) => ({ label: s.name, value: s.id }))
  )

  async function loadSectionConfigs(): Promise<void> {
    if (Date.now() - lastLoaded < CACHE_TTL_MS && sections.value.length > 0) return
    sections.value = await sectionConfigsApi.getAll()
    lastLoaded = Date.now()
  }

  async function createSection(name: string): Promise<void> {
    await sectionConfigsApi.create(name)
    invalidate()
    await loadSectionConfigs()
  }

  async function updateSection(id: string, name: string): Promise<void> {
    await sectionConfigsApi.update(id, name)
    invalidate()
    await loadSectionConfigs()
  }

  async function deleteSection(id: string): Promise<void> {
    await sectionConfigsApi.delete(id)
    invalidate()
    await loadSectionConfigs()
  }

  async function reorderSections(items: SimulationSectionReorderItem[]): Promise<void> {
    await sectionConfigsApi.reorder(items)
    invalidate()
    await loadSectionConfigs()
  }

  return {
    sections,
    activeSections,
    sectionOptions,
    loadSectionConfigs,
    createSection,
    updateSection,
    deleteSection,
    reorderSections
  }
}
