import { ref } from 'vue'
import { useAsyncState } from '@/core/composables/useAsyncState'
import {
  type AdminUser,
  type CreateAdminUserRequest,
  type UpdateAdminUserRequest,
  getAdminUsers,
  createAdminUser,
  updateAdminUser,
  deleteAdminUser,
  changeAdminUserRole
} from '@/modules/admin/api/admin.api'

type UserFormMode = 'create' | 'edit'

export const useAdminUsers = () => {
  const { isLoading, errorMessage, run } = useAsyncState()
  const users = ref<AdminUser[]>([])
  const selectedUser = ref<AdminUser | null>(null)
  const isEditing = ref(false)
  const formMode = ref<UserFormMode>('create')

  const loadUsers = async () => {
    const result = await run(() => getAdminUsers())
    if (result) users.value = result
  }

  const startCreateUser = () => {
    formMode.value = 'create'
    selectedUser.value = null
    isEditing.value = true
  }

  const startEditUser = (user: AdminUser) => {
    formMode.value = 'edit'
    selectedUser.value = { ...user }
    isEditing.value = true
  }

  const cancelEdit = () => {
    selectedUser.value = null
    isEditing.value = false
  }

  const submitCreate = async (payload: CreateAdminUserRequest) => {
    const result = await run(async () => {
      await createAdminUser(payload)
      return getAdminUsers()
    })
    if (result) {
      users.value = result
      isEditing.value = false
    }
  }

  const submitUpdate = async (id: string, payload: UpdateAdminUserRequest) => {
    const result = await run(async () => {
      await updateAdminUser(id, payload)
      return getAdminUsers()
    })
    if (result) {
      users.value = result
      isEditing.value = false
      selectedUser.value = null
    }
  }

  const removeUser = async (user: AdminUser) => {
    const result = await run(async () => {
      await deleteAdminUser(user.id)
      return user.id
    })
    if (result) {
      users.value = users.value.filter((u) => u.id !== result)
    }
  }

  const changeRole = async (id: string, role: string) => {
    await run(() => changeAdminUserRole(id, role))
    const user = users.value.find((u) => u.id === id)
    if (user) user.role = role
  }

  return {
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
    removeUser,
    changeRole
  }
}
