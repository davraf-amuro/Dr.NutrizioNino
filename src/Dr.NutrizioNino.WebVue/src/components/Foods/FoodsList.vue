<template>
  <n-space vertical>
    <n-space justify="space-between" align="center">
      <n-input
        v-model:value="searchQuery"
        placeholder="Cerca per nome..."
        clearable
        aria-label="Cerca per nome"
        style="width: 300px"
      />
      <n-button
        type="info"
        :disabled="checkedRowKeys.length < 2"
        @click="onCompare"
      >
        Confronta ({{ checkedRowKeys.length }})
      </n-button>
    </n-space>
    <n-data-table
      :columns="columns"
      :data="filteredData"
      :row-key="(row: FoodDashboardDto) => row.id"
      :row-props="rowProps"
      :checked-row-keys="checkedRowKeys"
      :single-line="false"
      :bordered="true"
      :pagination="false"
      aria-label="Lista alimenti"
      @update:checked-row-keys="checkedRowKeys = $event as string[]"
    />
  </n-space>
</template>

<script setup lang="ts">
import { h, ref } from 'vue'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import { NButton, NDataTable, NInput, NSpace, NTag, type DataTableColumns } from 'naive-ui'
import { useTableSearch } from '@/core/composables/useTableSearch'

const props = defineProps<{
  foods: FoodDashboardDto[]
  highlightId?: string | null
}>()

const emit = defineEmits<{
  edit: [food: FoodDashboardDto]
  delete: [food: FoodDashboardDto]
  compare: [foods: FoodDashboardDto[]]
  clone: [id: string]
}>()

const { searchQuery, filteredData } = useTableSearch(() => props.foods, 'name')

const checkedRowKeys = ref<string[]>([])

function onCompare() {
  const selected = props.foods.filter((f) => checkedRowKeys.value.includes(f.id))
  emit('compare', selected)
}

const rowProps = (row: FoodDashboardDto) => ({
  'data-food-id': row.id,
  class: row.id === props.highlightId ? 'row-highlight' : undefined
})

const columns: DataTableColumns<FoodDashboardDto> = [
  { type: 'selection' },
  {
    title: 'Prop.',
    key: 'isOwner',
    width: 60,
    render: (row) => row.isOwner
      ? h(NTag, { size: 'small', type: 'success', round: true }, { default: () => '★' })
      : h('span', { style: 'color:#ccc' }, '—')
  },
  { title: 'Nome', key: 'name', sorter: 'default' },
  { title: 'Marca', key: 'brandDescription', sorter: 'default' },
  { title: 'UdM', key: 'abbreviation', width: 80, sorter: 'default' },
  { title: 'Quantità', key: 'quantity', width: 100, sorter: (a, b) => (a.quantity ?? 0) - (b.quantity ?? 0) },
  {
    title: 'Supermercati',
    key: 'supermarketsText',
    render: (row) => {
      if (!row.supermarketsText) return null
      const names = row.supermarketsText.split(', ')
      return h(NSpace, { size: 4, wrap: true }, () =>
        names.map((name) => h(NTag, { size: 'small', type: 'info' }, { default: () => name }))
      )
    }
  },
  {
    title: 'Azioni',
    key: 'actions',
    width: 240,
    render: (row) =>
      h(NSpace, { size: 'small' }, () => [
        h(NButton, { size: 'small', type: 'primary', onClick: () => emit('edit', row) }, { default: () => 'Modifica' }),
        h(NButton, { size: 'small', onClick: () => emit('clone', row.id) }, { default: () => 'Clona' }),
        h(NButton, { size: 'small', type: 'error', tertiary: true, onClick: () => emit('delete', row) }, { default: () => 'Elimina' })
      ])
  }
]
</script>

<style>
/* Global: Naive UI non usa scoped per le classi delle righe */
tr.row-highlight td {
  animation: row-flash 2.5s ease forwards;
}

@keyframes row-flash {
  0%   { background-color: rgba(99, 226, 183, 0.45); }
  70%  { background-color: rgba(99, 226, 183, 0.20); }
  100% { background-color: transparent; }
}
</style>
