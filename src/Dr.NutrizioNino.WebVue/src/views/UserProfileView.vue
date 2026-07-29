<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { NButton, NCard, NFormItem, NGi, NGrid, NInputNumber, NRadioGroup, NRadioButton, NSpace, NText, NAlert, useMessage } from 'naive-ui'
import { useAuth } from '@/modules/auth/composables/useAuth'
import { useTheme } from '@/modules/auth/composables/useTheme'
import { getMyNutritionalTarget, setMyNutritionalTarget } from '@/modules/nutritionalTarget/api/nutritionalTarget.api'

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

// ── Fabbisogno personale ──────────────────────────────────────
const target = reactive<{ kcalTarget: number | null; carbsTarget: number | null; proteinTarget: number | null; fatTarget: number | null }>({
  kcalTarget: null,
  carbsTarget: null,
  proteinTarget: null,
  fatTarget: null
})
const savingTarget = ref(false)

onMounted(async () => {
  const current = await getMyNutritionalTarget()
  if (current) {
    target.kcalTarget = current.kcalTarget
    target.carbsTarget = current.carbsTarget
    target.proteinTarget = current.proteinTarget
    target.fatTarget = current.fatTarget
  }
})

async function handleSaveTarget(): Promise<void> {
  savingTarget.value = true
  try {
    await setMyNutritionalTarget({ ...target })
    message.success('Fabbisogno aggiornato')
  } catch {
    message.error('Errore nel salvataggio del fabbisogno')
  } finally {
    savingTarget.value = false
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

    <n-card title="Fabbisogno giornaliero" size="large">
      <n-grid :cols="2" :x-gap="12" :y-gap="8">
        <n-gi>
          <n-form-item label="Kcal">
            <n-input-number v-model:value="target.kcalTarget" :min="0.1" :precision="1" clearable style="width: 100%" />
          </n-form-item>
        </n-gi>
        <n-gi>
          <n-form-item label="Carboidrati (g)">
            <n-input-number v-model:value="target.carbsTarget" :min="0.1" :precision="1" clearable style="width: 100%" />
          </n-form-item>
        </n-gi>
        <n-gi>
          <n-form-item label="Proteine (g)">
            <n-input-number v-model:value="target.proteinTarget" :min="0.1" :precision="1" clearable style="width: 100%" />
          </n-form-item>
        </n-gi>
        <n-gi>
          <n-form-item label="Grassi (g)">
            <n-input-number v-model:value="target.fatTarget" :min="0.1" :precision="1" clearable style="width: 100%" />
          </n-form-item>
        </n-gi>
        <n-gi :span="2">
          <n-button type="primary" :loading="savingTarget" @click="handleSaveTarget">Salva fabbisogno</n-button>
        </n-gi>
      </n-grid>
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
