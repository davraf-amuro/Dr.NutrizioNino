<template>
  <n-space vertical size="small" class="brands-table">
    <n-data-table
      :columns="columns"
      :data="brands"
      :row-key="(row: Brand) => row.id"
      :single-line="false"
      :bordered="true"
      :pagination="false"
      :scroll-x="980"
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
  edit: [brand: Brand]
  delete: [brand: Brand]
}>()

function updateBrand(brand: Brand) {
  emit('edit', brand)
}

function deleteBrand(brand: Brand) {
  emit('delete', brand)
}

const columns: DataTableColumns<Brand> = [
  {
    title: 'Guid',
    key: 'id',
    width: 420
  },
  {
    title: 'Nome',
    key: 'name',
    width: 300
  },
  {
    title: 'Azioni',
    key: 'actions',
    width: 220,
    render: (row) =>
      h(NSpace, { size: 'small' }, () => [
        h(
          NButton,
          {
            size: 'small',
            type: 'primary',
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

<style scoped>
.brands-table {
  width: 100%;
}
</style>
