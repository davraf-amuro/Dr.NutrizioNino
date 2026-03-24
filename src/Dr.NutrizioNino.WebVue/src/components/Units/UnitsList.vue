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
      :row-key="(row: UnitOfMeasureDto) => row.id"
      :single-line="false"
      :bordered="true"
      :pagination="false"
      aria-label="Lista unità di misura"
    />
  </n-space>
</template>

<script setup lang="ts">
import { h } from 'vue'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import { NButton, NDataTable, NInput, NSpace, type DataTableColumns } from 'naive-ui'
import { useTableSearch } from '@/core/composables/useTableSearch'

const props = defineProps<{
  units: UnitOfMeasureDto[]
}>()

const emit = defineEmits<{
  edit: [unit: UnitOfMeasureDto]
  delete: [unit: UnitOfMeasureDto]
}>()

const { searchQuery, filteredData } = useTableSearch(() => props.units, 'name')

const columns: DataTableColumns<UnitOfMeasureDto> = [
  { title: 'Nome', key: 'name', sorter: 'default' },
  { title: 'Abbreviazione', key: 'abbreviation', width: 160, sorter: 'default' },
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
