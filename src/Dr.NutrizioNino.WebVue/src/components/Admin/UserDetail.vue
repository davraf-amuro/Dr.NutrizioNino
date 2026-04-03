<template>
  <n-space vertical size="medium">
    <n-h3 style="margin: 0">{{ isEditMode ? 'Modifica utente' : 'Nuovo utente' }}</n-h3>

    <n-form ref="formRef" :model="formModel" :rules="rules" label-placement="left" label-width="140" label-align="right">
      <n-form-item label="Username" path="userName">
        <n-input v-model:value="formModel.userName" :disabled="isSubmitting" clearable />
      </n-form-item>

      <n-form-item label="Email" path="email">
        <n-input v-model:value="formModel.email" :disabled="isSubmitting" clearable />
      </n-form-item>

      <n-form-item v-if="!isEditMode" label="Password" path="password">
        <n-input
          v-model:value="formModel.password"
          type="password"
          show-password-on="click"
          :disabled="isSubmitting"
          clearable
        />
      </n-form-item>

      <n-form-item label="Data di nascita" path="dateOfBirth">
        <n-input v-model:value="formModel.dateOfBirth" placeholder="YYYY-MM-DD" :disabled="isSubmitting" />
      </n-form-item>

      <n-form-item label="Ruolo" path="role">
        <n-select
          v-model:value="formModel.role"
          :options="roleOptions"
          :disabled="isSubmitting"
        />
      </n-form-item>
    </n-form>

    <n-space justify="end" size="small">
      <n-button @click="cancel" :disabled="isSubmitting">Annulla</n-button>
      <n-button type="primary" @click="save" :loading="isSubmitting">
        {{ isEditMode ? 'Aggiorna' : 'Salva' }}
      </n-button>
    </n-space>
  </n-space>
</template>

<script setup lang="ts">
import { computed, reactive, ref, watch } from 'vue'
import { NButton, NForm, NFormItem, NH3, NInput, NSelect, NSpace, type FormInst, type FormRules } from 'naive-ui'
import type { AdminUser, CreateAdminUserRequest, UpdateAdminUserRequest } from '@/modules/admin/api/admin.api'

const props = defineProps<{
  mode?: 'create' | 'edit'
  user?: AdminUser | null
  isSubmitting?: boolean
}>()

const emit = defineEmits<{
  create: [payload: CreateAdminUserRequest]
  update: [id: string, payload: UpdateAdminUserRequest]
  cancel: []
}>()

const formRef = ref<FormInst | null>(null)
const isEditMode = computed(() => props.mode === 'edit')

const roleOptions = [
  { label: 'User', value: 'User' },
  { label: 'Admin', value: 'Admin' }
]

const formModel = reactive({
  userName: '',
  email: '',
  password: '',
  dateOfBirth: '',
  role: 'User'
})

const rules = computed<FormRules>(() => ({
  userName: [{ required: true, message: 'Username obbligatorio', trigger: ['blur', 'input'] }],
  email: [{ required: true, type: 'email', message: 'Email valida obbligatoria', trigger: ['blur', 'input'] }],
  password: isEditMode.value ? [] : [{ required: true, min: 8, message: 'Password min 8 caratteri', trigger: ['blur', 'input'] }],
  dateOfBirth: [{ required: true, message: 'Data di nascita obbligatoria', trigger: ['blur', 'input'] }],
  role: [{ required: true, message: 'Ruolo obbligatorio', trigger: 'change' }]
}))

watch(
  () => props.user,
  (user) => {
    formModel.userName = user?.userName ?? ''
    formModel.email = user?.email ?? ''
    formModel.dateOfBirth = user?.dateOfBirth ?? ''
    formModel.role = user?.role ?? 'User'
    formModel.password = ''
  },
  { immediate: true }
)

function save() {
  formRef.value?.validate((errors) => {
    if (errors) return

    if (isEditMode.value && props.user) {
      emit('update', props.user.id, {
        userName: formModel.userName.trim(),
        email: formModel.email.trim(),
        dateOfBirth: formModel.dateOfBirth.trim()
      })
    } else {
      emit('create', {
        userName: formModel.userName.trim(),
        email: formModel.email.trim(),
        password: formModel.password,
        dateOfBirth: formModel.dateOfBirth.trim(),
        role: formModel.role
      })
    }
  })
}

function cancel() {
  emit('cancel')
}
</script>
