<template>
  <n-space vertical size="small">
    <n-data-table
      :columns="columns"
      :data="brands"
      :single-line="false"
      :bordered="true"
      :pagination="false"
    />
  </n-space>
</template>

<script setup lang="ts">
import { h } from 'vue'
import type { Brand } from '@/Interfaces/Brand'
import { NButton, NDataTable, NSpace, type DataTableColumns } from 'naive-ui'

const props = defineProps<{
  brands: Brand[]
}>()

const emit = defineEmits<{
  update: [brand: Brand]
  delete: [brand: Brand]
}>()

function updateBrand(brand: Brand) {
  emit('update', brand)
}

function deleteBrand(brand: Brand) {
  emit('delete', brand)
}

const columns: DataTableColumns<Brand> = [
  {
    title: 'Guid',
    key: 'id'
  },
  {
    title: 'Nome',
    key: 'name'
  },
  {
    title: 'Azioni',
    key: 'actions',
    render: (row) =>
      h(NSpace, { size: 'small' }, () => [
        h(
          NButton,
          {
            size: 'small',
            tertiary: true,
            onClick: () => updateBrand(row)
          },
          { default: () => 'Modifica' }
        ),
        h(
          NButton,
          {
            size: 'small',
            type: 'error',
            tertiary: true,
            onClick: () => deleteBrand(row)
          },
          { default: () => 'Elimina' }
        )
      ])
  }
]
</script>
