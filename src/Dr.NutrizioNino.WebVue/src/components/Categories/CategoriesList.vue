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
      :row-key="(row: Category) => row.id"
      :single-line="false"
      :bordered="true"
      :pagination="false"
      aria-label="Lista categorie"
    />
  </n-space>
</template>

<script setup lang="ts">
import { h } from 'vue'
import type { Category } from '@/Interfaces/Category'
import { NButton, NDataTable, NInput, NSpace, type DataTableColumns } from 'naive-ui'
import { useTableSearch } from '@/core/composables/useTableSearch'

const props = defineProps<{
  categories: Category[]
}>()

const emit = defineEmits<{
  edit: [category: Category]
  delete: [category: Category]
}>()

const { searchQuery, filteredData } = useTableSearch(() => props.categories, 'name')

const columns: DataTableColumns<Category> = [
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
