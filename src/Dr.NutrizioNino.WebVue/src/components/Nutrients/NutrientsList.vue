<template>
  <n-data-table
    :columns="columns"
    :data="nutrients"
    :row-key="(row: Nutrient) => row.id"
    :single-line="false"
    :bordered="true"
    :pagination="false"
    aria-label="Lista nutrienti"
  />
</template>

<script setup lang="ts">
import { h } from 'vue'
import type { Nutrient } from '@/Interfaces/Nutrients/Nutrient'
import { NButton, NDataTable, NSpace, type DataTableColumns } from 'naive-ui'

defineProps<{
  nutrients: Nutrient[]
}>()

const emit = defineEmits<{
  edit: [nutrient: Nutrient]
  delete: [nutrient: Nutrient]
}>()

const columns: DataTableColumns<Nutrient> = [
  { title: 'Ordine', key: 'positionOrder', width: 90 },
  { title: 'Nome', key: 'name' },
  { title: 'Qty default', key: 'defaultQuantity', width: 120 },
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
