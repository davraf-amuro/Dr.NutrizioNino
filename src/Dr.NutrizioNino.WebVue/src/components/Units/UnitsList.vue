<template>
  <n-data-table
    :columns="columns"
    :data="units"
    :row-key="(row: UnitOfMeasureDto) => row.id"
    :single-line="false"
    :bordered="true"
    :pagination="false"
  />
</template>

<script setup lang="ts">
import { h } from 'vue'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import { NButton, NDataTable, NSpace, type DataTableColumns } from 'naive-ui'

defineProps<{
  units: UnitOfMeasureDto[]
}>()

const emit = defineEmits<{
  edit: [unit: UnitOfMeasureDto]
  delete: [unit: UnitOfMeasureDto]
}>()

const columns: DataTableColumns<UnitOfMeasureDto> = [
  { title: 'Nome', key: 'name' },
  { title: 'Abbreviazione', key: 'abbreviation', width: 160 },
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
