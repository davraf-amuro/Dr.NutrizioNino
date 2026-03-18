<template>
  <n-data-table
    :columns="columns"
    :data="brands"
    :row-key="(row: Brand) => row.id"
    :single-line="false"
    :bordered="true"
    :pagination="false"
    aria-label="Lista marche"
  />
</template>

<script setup lang="ts">
import { h } from 'vue'
import type { Brand } from '@/Interfaces/Brand'
import { NButton, NDataTable, NSpace, type DataTableColumns } from 'naive-ui'

const props = defineProps<{
  brands: Brand[]
}>()

const emit = defineEmits<{
  edit: [brand: Brand]
  delete: [brand: Brand]
}>()

const columns: DataTableColumns<Brand> = [
  {
    title: 'Nome',
    key: 'name'
  },
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
