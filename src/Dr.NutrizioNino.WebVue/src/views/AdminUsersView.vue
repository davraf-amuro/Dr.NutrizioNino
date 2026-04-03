<script setup lang="ts">
import { onMounted } from 'vue'
import { NAlert, NButton, NCard, NSpace, useDialog, useMessage } from 'naive-ui'
import UsersList from '@/components/Admin/UsersList.vue'
import UserDetail from '@/components/Admin/UserDetail.vue'
import { useAdminUsers } from '@/modules/admin/composables/useAdminUsers'
import type { AdminUser } from '@/modules/admin/api/admin.api'

const {
  users,
  selectedUser,
  isEditing,
  formMode,
  isLoading,
  errorMessage,
  loadUsers,
  startCreateUser,
  startEditUser,
  cancelEdit,
  submitCreate,
  submitUpdate,
  removeUser
} = useAdminUsers()

const message = useMessage()
const dialog = useDialog()

const handleDeleteUser = async (user: AdminUser) => {
  dialog.warning({
    title: 'Conferma eliminazione',
    content: `Eliminare l'utente "${user.userName}"?`,
    positiveText: 'Elimina',
    negativeText: 'Annulla',
    onPositiveClick: () => removeUser(user)
  })
}

onMounted(async () => {
  await loadUsers()
})
</script>

<template>
  <n-space vertical size="large" class="admin-users-page">
    <n-card title="Gestione Utenti" size="large">
      <template #header-extra>
        <n-button
          v-if="!isEditing"
          type="primary"
          @click="startCreateUser"
          :loading="isLoading"
        >
          Nuovo utente
        </n-button>
      </template>

      <n-space vertical size="medium">
        <n-alert v-if="errorMessage" type="error" :show-icon="true" :bordered="false">
          {{ errorMessage }}
        </n-alert>

        <UsersList
          v-if="!isEditing"
          :users="users"
          @edit="startEditUser"
          @delete="handleDeleteUser"
        />

        <UserDetail
          v-else
          :mode="formMode"
          :user="selectedUser"
          :is-submitting="isLoading"
          @create="submitCreate"
          @update="submitUpdate"
          @cancel="cancelEdit"
        />
      </n-space>
    </n-card>
  </n-space>
</template>

<style scoped>
.admin-users-page {
  width: 100%;
}
</style>
