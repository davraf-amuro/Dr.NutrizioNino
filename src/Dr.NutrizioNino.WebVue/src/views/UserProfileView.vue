<script setup lang="ts">
import { ref } from 'vue'
import { NCard, NRadioGroup, NRadioButton, NSpace, NText, NAlert, useMessage } from 'naive-ui'
import { useAuth } from '@/modules/auth/composables/useAuth'
import { useTheme } from '@/modules/auth/composables/useTheme'

const { user } = useAuth()
const { preference, setTheme } = useTheme()
const message = useMessage()

const saving = ref(false)
const errorMessage = ref<string | null>(null)

const themeOptions = [
  { label: 'Chiaro', value: 'light' },
  { label: 'Scuro', value: 'dark' },
  { label: 'Sistema', value: 'system' }
]

async function handleThemeChange(value: string): Promise<void> {
  saving.value = true
  errorMessage.value = null
  try {
    await setTheme(value)
    message.success('Tema aggiornato')
  } catch {
    errorMessage.value = 'Errore nel salvataggio del tema'
  } finally {
    saving.value = false
  }
}
</script>

<template>
  <n-space vertical size="large" class="profile-page">
    <n-card title="Profilo" size="large">
      <n-space vertical size="medium">
        <n-alert v-if="errorMessage" type="error" :show-icon="true" :bordered="false">
          {{ errorMessage }}
        </n-alert>

        <div v-if="user" class="profile-info">
          <n-text depth="3" class="profile-label">Utente</n-text>
          <n-text class="profile-value">{{ user.userName }}</n-text>
        </div>

        <div class="profile-info">
          <n-text depth="3" class="profile-label">Tema interfaccia</n-text>
          <n-radio-group
            :value="preference"
            :disabled="saving"
            @update:value="handleThemeChange"
          >
            <n-radio-button
              v-for="opt in themeOptions"
              :key="opt.value"
              :value="opt.value"
              :label="opt.label"
            />
          </n-radio-group>
        </div>
      </n-space>
    </n-card>
  </n-space>
</template>

<style scoped>
.profile-page {
  width: 100%;
  max-width: 480px;
}

.profile-info {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.profile-label {
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.profile-value {
  font-size: 15px;
}
</style>
