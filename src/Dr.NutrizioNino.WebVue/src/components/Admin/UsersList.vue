<template>
  <n-space vertical>
    <n-input
      v-model:value="searchQuery"
      placeholder="Cerca per username..."
      clearable
      aria-label="Cerca per username"
    />
    <n-data-table
      :columns="columns"
      :data="filteredData"
      :row-key="(row: AdminUser) => row.id"
      :single-line="false"
      :bordered="true"
      :pagination="false"
      aria-label="Lista utenti"
    />
  </n-space>
</template>

<script setup lang="ts">
import { h } from 'vue'
import type { AdminUser } from '@/modules/admin/api/admin.api'
import { NButton, NDataTable, NInput, NSpace, NTag, type DataTableColumns } from 'naive-ui'
import { useTableSearch } from '@/core/composables/useTableSearch'

const props = defineProps<{
  users: AdminUser[]
}>()

const emit = defineEmits<{
  edit: [user: AdminUser]
  delete: [user: AdminUser]
}>()

const { searchQuery, filteredData } = useTableSearch(() => props.users, 'userName')

const roleType = (role: string) => role === 'Admin' ? 'error' : 'info'

const columns: DataTableColumns<AdminUser> = [
  { title: 'Username', key: 'userName', sorter: 'default' },
  { title: 'Email', key: 'email', sorter: 'default' },
  { title: 'Data di nascita', key: 'dateOfBirth', width: 140 },
  {
    title: 'Ruolo',
    key: 'role',
    width: 100,
    render: (row) => h(NTag, { size: 'small', type: roleType(row.role) }, { default: () => row.role })
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
