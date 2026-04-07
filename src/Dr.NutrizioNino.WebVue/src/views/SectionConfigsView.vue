<template>
  <n-card title="Sezioni simulazione">
    <n-space vertical size="medium">
      <!-- Form aggiungi nuova sezione -->
      <n-card size="small" title="Nuova sezione">
        <div class="add-row">
          <n-input
            v-model:value="newName"
            placeholder="Nome sezione (es. Pre-allenamento)"
            :disabled="isAdding"
            @keyup.enter="handleAdd"
          />
          <n-button type="primary" :loading="isAdding" :disabled="!newName.trim()" @click="handleAdd">
            Aggiungi
          </n-button>
        </div>
      </n-card>

      <!-- Lista drag & drop con azioni inline -->
      <n-text depth="3" style="font-size: 12px">
        Trascina ⠿ per riordinare · ↑↓ per accessibilità da tastiera · Salva per confermare l'ordine
      </n-text>

      <VueDraggable v-model="localItems" handle=".drag-handle" :animation="150">
        <div
          v-for="(item, index) in localItems"
          :key="item.id"
          class="section-row"
          :class="{ inactive: !item.isActive }"
        >
          <span class="drag-handle" title="Trascina per riordinare" aria-hidden="true">⠿</span>

          <!-- Nome: in modalità modifica mostra input, altrimenti testo -->
          <template v-if="editingId === item.id">
            <n-input
              v-model:value="editingName"
              size="small"
              style="flex: 1"
              @keyup.enter="handleSaveEdit(item.id)"
              @keyup.escape="cancelEdit"
            />
            <n-space size="small" class="row-actions">
              <n-button size="tiny" type="primary" @click="handleSaveEdit(item.id)">✓</n-button>
              <n-button size="tiny" @click="cancelEdit">✕</n-button>
            </n-space>
          </template>
          <template v-else>
            <span class="item-name" :style="!item.isActive ? 'text-decoration: line-through; opacity: 0.5' : ''">
              {{ item.name }}
            </span>
            <n-space size="small" class="row-actions">
              <n-button size="tiny" :disabled="index === 0" :aria-label="`Sposta ${item.name} su`" @click="moveUp(index)">↑</n-button>
              <n-button size="tiny" :disabled="index === localItems.length - 1" :aria-label="`Sposta ${item.name} giù`" @click="moveDown(index)">↓</n-button>
              <n-button size="tiny" @click="startEdit(item)">Modifica</n-button>
              <n-button
                v-if="item.isActive"
                size="tiny"
                type="error"
                tertiary
                :aria-label="`Elimina ${item.name}`"
                @click="handleDelete(item.id, item.name)"
              >Elimina</n-button>
            </n-space>
          </template>
        </div>
      </VueDraggable>

      <n-space justify="end">
        <n-button type="primary" :loading="isSaving" @click="handleSaveOrder">Salva ordine</n-button>
      </n-space>
    </n-space>
  </n-card>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { NButton, NCard, NInput, NSpace, NText, useDialog, useNotification } from 'naive-ui'
import { VueDraggable } from 'vue-draggable-plus'
import { useSectionConfigs } from '@/modules/sectionConfigs/composables/useSectionConfigs'
import type { SimulationSectionDto } from '@/Interfaces/sectionConfigs/SectionConfigDto'

const { sections, loadSectionConfigs, createSection, updateSection, deleteSection, reorderSections } = useSectionConfigs()
const notification = useNotification()
const dialog = useDialog()

const localItems = ref<SimulationSectionDto[]>([])
const newName = ref('')
const isAdding = ref(false)
const isSaving = ref(false)
const editingId = ref<string | null>(null)
const editingName = ref('')

onMounted(async () => {
  await loadSectionConfigs()
  localItems.value = [...sections.value]
})

// ── Aggiungi ──────────────────────────────────────────────
const handleAdd = async () => {
  const name = newName.value.trim()
  if (!name) return
  isAdding.value = true
  try {
    await createSection(name)
    newName.value = ''
    localItems.value = [...sections.value]
    notification.success({ title: 'Sezione aggiunta', duration: 2000 })
  } finally {
    isAdding.value = false
  }
}

// ── Modifica ──────────────────────────────────────────────
const startEdit = (item: SimulationSectionDto) => {
  editingId.value = item.id
  editingName.value = item.name
}

const cancelEdit = () => {
  editingId.value = null
  editingName.value = ''
}

const handleSaveEdit = async (id: string) => {
  const name = editingName.value.trim()
  if (!name) return
  await updateSection(id, name)
  localItems.value = [...sections.value]
  cancelEdit()
  notification.success({ title: 'Sezione aggiornata', duration: 2000 })
}

// ── Elimina ───────────────────────────────────────────────
const handleDelete = (id: string, name: string) => {
  dialog.warning({
    title: 'Elimina sezione',
    content: `Eliminare "${name}"? Le voci già inserite in questa sezione rimarranno visibili.`,
    positiveText: 'Elimina',
    negativeText: 'Annulla',
    onPositiveClick: async () => {
      await deleteSection(id)
      localItems.value = [...sections.value]
      notification.success({ title: 'Sezione eliminata', duration: 2000 })
    }
  })
}

// ── Riordina ──────────────────────────────────────────────
const moveUp = (index: number) => {
  if (index === 0) return
  const arr = [...localItems.value]
  ;[arr[index - 1], arr[index]] = [arr[index], arr[index - 1]]
  localItems.value = arr
}

const moveDown = (index: number) => {
  if (index === localItems.value.length - 1) return
  const arr = [...localItems.value]
  ;[arr[index], arr[index + 1]] = [arr[index + 1], arr[index]]
  localItems.value = arr
}

const handleSaveOrder = async () => {
  isSaving.value = true
  try {
    const items = localItems.value.map((item, idx) => ({ id: item.id, displayOrder: idx + 1 }))
    await reorderSections(items)
    notification.success({ title: 'Ordine salvato', duration: 2500 })
  } finally {
    isSaving.value = false
  }
}
</script>

<style scoped>
.add-row {
  display: flex;
  gap: 8px;
  align-items: center;
}

.section-row {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 8px 4px;
  border-bottom: 1px solid var(--n-border-color);
  cursor: default;
}

.drag-handle {
  cursor: grab;
  font-size: 18px;
  color: #aaa;
  user-select: none;
  flex-shrink: 0;
}

.drag-handle:active {
  cursor: grabbing;
}

.item-name {
  flex: 1;
  font-size: 14px;
}

.row-actions {
  flex-shrink: 0;
}
</style>
