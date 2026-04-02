<script setup lang="ts">
import { ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuth } from '@/modules/auth/composables/useAuth'
import { ApiError } from '@/core/http/ApiError'
import { NCard, NForm, NFormItem, NInput, NButton, NAlert } from 'naive-ui'

const router = useRouter()
const route = useRoute()
const { login } = useAuth()

const userName = ref('')
const password = ref('')
const errorMessage = ref<string | null>(null)
const isLoading = ref(false)

const sessionExpired = route.query.reason === 'session_expired'

async function handleSubmit() {
  errorMessage.value = null
  isLoading.value = true
  try {
    await login(userName.value, password.value)
    const redirect = (route.query.redirect as string | undefined) ?? '/'
    await router.push(redirect)
  } catch (e) {
    errorMessage.value = e instanceof ApiError ? e.message : 'Credenziali non valide.'
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <NCard title="Dr. NutrizioNino — Accesso" style="max-width: 400px; width: 100%">
      <NAlert v-if="sessionExpired" type="warning" style="margin-bottom: 16px">
        Sessione scaduta. Effettua nuovamente il login.
      </NAlert>
      <NForm @submit.prevent="handleSubmit" autocomplete="on">
        <NFormItem label="Nome utente" :label-props="{ for: 'username' }">
          <NInput
            id="username"
            v-model:value="userName"
            placeholder="Inserisci il nome utente"
            autocomplete="username"
            :disabled="isLoading"
          />
        </NFormItem>
        <NFormItem label="Password" :label-props="{ for: 'password' }">
          <NInput
            id="password"
            v-model:value="password"
            type="password"
            show-password-on="click"
            placeholder="Inserisci la password"
            autocomplete="current-password"
            :disabled="isLoading"
          />
        </NFormItem>
        <NAlert v-if="errorMessage" type="error" style="margin-bottom: 16px" role="alert">
          {{ errorMessage }}
        </NAlert>
        <NButton
          type="primary"
          attr-type="submit"
          block
          :loading="isLoading"
        >
          Accedi
        </NButton>
      </NForm>
    </NCard>
  </div>
</template>

<style scoped>
.login-page {
  display: flex;
  justify-content: center;
  align-items: center;
  min-height: 100vh;
  padding: 24px;
}
</style>
