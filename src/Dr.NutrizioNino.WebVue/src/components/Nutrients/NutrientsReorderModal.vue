<template>
  <n-modal v-model:show="show" preset="card" title="Ordina nutrienti" style="width: 480px">
    <n-space vertical>
      <n-text depth="3">Trascina le righe per riordinare. Usa ↑ ↓ per accessibilità da tastiera.</n-text>
      <VueDraggable v-model="localItems" handle=".drag-handle" :animation="150">
        <div
          v-for="(item, index) in localItems"
          :key="item.id"
          class="reorder-row"
        >
          <span class="drag-handle" title="Trascina per riordinare" aria-hidden="true">⠿</span>
          <span class="item-name">{{ item.name }}</span>
          <n-space size="small" class="arrows">
            <n-button
              size="tiny"
              :disabled="index === 0"
              @click="moveUp(index)"
              :aria-label="`Sposta ${item.name} su`"
            >↑</n-button>
            <n-button
              size="tiny"
              :disabled="index === localItems.length - 1"
              @click="moveDown(index)"
              :aria-label="`Sposta ${item.name} giù`"
            >↓</n-button>
          </n-space>
        </div>
      </VueDraggable>
    </n-space>

    <template #footer>
      <n-space justify="end">
        <n-button @click="show = false">Annulla</n-button>
        <n-button type="primary" :loading="isSaving" @click="handleConfirm">Conferma</n-button>
      </n-space>
    </template>
  </n-modal>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import { NButton, NModal, NSpace, NText } from 'naive-ui'
import { VueDraggable } from 'vue-draggable-plus'
import type { Nutrient } from '@/Interfaces/Nutrients/Nutrient'
import type { NutrientReorderItem } from '@/modules/nutrients/api/nutrients.api'

interface ReorderEntry { id: string; name: string }

const props = defineProps<{ nutrients: Nutrient[] }>()

const emit = defineEmits<{
  confirm: [items: NutrientReorderItem[]]
}>()

const show = defineModel<boolean>('show', { default: false })
const isSaving = ref(false)
const localItems = ref<ReorderEntry[]>([])

watch(show, (val) => {
  if (val) {
    localItems.value = [...props.nutrients]
      .sort((a, b) => a.positionOrder - b.positionOrder)
      .map((n) => ({ id: n.id, name: n.name }))
  }
})

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

const handleConfirm = async () => {
  isSaving.value = true
  try {
    const items: NutrientReorderItem[] = localItems.value.map((item, idx) => ({
      id: item.id,
      positionOrder: idx + 1
    }))
    emit('confirm', items)
    show.value = false
  } finally {
    isSaving.value = false
  }
}
</script>

<style scoped>
.reorder-row {
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
}
.drag-handle:active {
  cursor: grabbing;
}
.item-name {
  flex: 1;
}
.arrows {
  flex-shrink: 0;
}
</style>
