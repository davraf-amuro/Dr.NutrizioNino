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
      :row-key="(row: DailySimulationListItemDto) => row.id"
      :single-line="false"
      :bordered="true"
      :pagination="false"
      aria-label="Lista simulazioni piano giornaliero"
    />
  </n-space>
</template>

<script setup lang="ts">
import { h } from 'vue'
import { NButton, NDataTable, NInput, NSpace, type DataTableColumns } from 'naive-ui'
import type { DailySimulationListItemDto } from '@/Interfaces/dailySimulations/DailySimulationDto'
import { useTableSearch } from '@/core/composables/useTableSearch'

const props = defineProps<{ simulations: DailySimulationListItemDto[] }>()
const emit = defineEmits<{
  detail: [sim: DailySimulationListItemDto]
  clone: [sim: DailySimulationListItemDto]
  delete: [sim: DailySimulationListItemDto]
}>()

const { searchQuery, filteredData } = useTableSearch(() => props.simulations, 'name')

const columns: DataTableColumns<DailySimulationListItemDto> = [
  { title: 'Nome', key: 'name', sorter: 'default' },
  {
    title: 'Voci',
    key: 'entryCount',
    width: 80,
    render: (row) => h('span', row.entryCount)
  },
  {
    title: 'Azioni',
    key: 'actions',
    width: 240,
    render: (row) =>
      h(NSpace, { size: 'small' }, {
        default: () => [
          h(NButton, { size: 'small', secondary: true, onClick: () => emit('detail', row) }, { default: () => 'Apri' }),
          h(NButton, { size: 'small', secondary: true, onClick: () => emit('clone', row) }, { default: () => 'Clona' }),
          h(NButton, { size: 'small', type: 'error', tertiary: true, onClick: () => emit('delete', row) }, { default: () => 'Elimina' })
        ]
      })
  }
]
</script>
