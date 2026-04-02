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
      :row-key="(row: Brand) => row.id"
      :single-line="false"
      :bordered="true"
      :pagination="false"
      aria-label="Lista marche"
    />
  </n-space>
</template>

<script setup lang="ts">
import { h } from 'vue'
import type { Brand } from '@/Interfaces/Brand'
import { NButton, NDataTable, NInput, NSpace, NTag, type DataTableColumns } from 'naive-ui'
import { useTableSearch } from '@/core/composables/useTableSearch'

const props = defineProps<{
  brands: Brand[]
}>()

const emit = defineEmits<{
  edit: [brand: Brand]
  delete: [brand: Brand]
}>()

const { searchQuery, filteredData } = useTableSearch(() => props.brands, 'name')

const columns: DataTableColumns<Brand> = [
  {
    title: 'Prop.',
    key: 'isOwner',
    width: 60,
    render: (row) => row.isOwner
      ? h(NTag, { size: 'small', type: 'success', round: true }, { default: () => '★' })
      : h('span', { style: 'color:#ccc' }, '—')
  },
  { title: 'Nome', key: 'name', sorter: 'default' },
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
