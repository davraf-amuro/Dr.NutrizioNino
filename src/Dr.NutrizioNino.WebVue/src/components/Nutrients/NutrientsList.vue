<template>
  <n-space vertical>
    <n-input
      v-model:value="searchQuery"
      placeholder="Cerca per nome..."
      clearable
      aria-label="Cerca per nome"
    />
    <n-data-table
      :columns="columns"
      :data="filteredData"
      :row-key="(row: Nutrient) => row.id"
      :single-line="false"
      :bordered="true"
      :pagination="false"
      aria-label="Lista nutrienti"
    />
  </n-space>
</template>

<script setup lang="ts">
import { h } from 'vue'
import type { Nutrient } from '@/Interfaces/Nutrients/Nutrient'
import { NButton, NDataTable, NInput, NSpace, type DataTableColumns } from 'naive-ui'
import { useTableSearch } from '@/core/composables/useTableSearch'

const props = defineProps<{
  nutrients: Nutrient[]
}>()

const emit = defineEmits<{
  edit: [nutrient: Nutrient]
  delete: [nutrient: Nutrient]
}>()

const { searchQuery, filteredData } = useTableSearch(() => props.nutrients, 'name')

const columns: DataTableColumns<Nutrient> = [
  { title: 'Ordine', key: 'positionOrder', width: 90, sorter: (a, b) => a.positionOrder - b.positionOrder },
  { title: 'Nome', key: 'name', sorter: 'default' },
  { title: 'Qty default', key: 'defaultQuantity', width: 120, sorter: (a, b) => a.defaultQuantity - b.defaultQuantity },
  {
    title: 'Azioni',
    key: 'actions',
    width: 180,
    render: (row) =>
      h(NSpace, { size: 'small' }, () => [
        h(NButton, { size: 'small', type: 'primary', onClick: () => emit('edit', row) }, { default: () => 'Modifica' }),
        h(NButton, { size: 'small', type: 'error', tertiary: true, onClick: () => emit('delete', row) }, { default: () => 'Elimina' })
      ])
  }
]
</script>
