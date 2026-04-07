import { computed, ref } from 'vue'
import { darkTheme, lightTheme, useOsTheme } from 'naive-ui'
import { authApi } from '@/modules/auth/api/auth.api'

const STORAGE_KEY = 'theme'
const osTheme = useOsTheme()

function readStorage(): string {
  return localStorage.getItem(STORAGE_KEY) ?? 'system'
}

const preference = ref<string>(readStorage())

export function useTheme() {
  const resolvedTheme = computed(() => {
    const p = preference.value
    if (p === 'dark') return darkTheme
    if (p === 'light') return lightTheme
    // system
    return osTheme.value === 'dark' ? darkTheme : lightTheme
  })

  function applyPreference(value: string): void {
    preference.value = value
    localStorage.setItem(STORAGE_KEY, value)
  }

  async function setTheme(value: string): Promise<void> {
    applyPreference(value)
    await authApi.updateTheme(value)
  }

  function initFromUser(themePreference: string): void {
    applyPreference(themePreference)
  }

  return { preference, resolvedTheme, setTheme, initFromUser }
}
