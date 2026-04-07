import { ref, computed } from 'vue'
import { authApi } from '@/modules/auth/api/auth.api'
import type { MeResponse } from '@/Interfaces/auth/AuthDto'
import { saveToken, getToken, removeToken } from '@/core/http/tokenStorage'
import { useTheme } from '@/modules/auth/composables/useTheme'

function isTokenValid(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]))
    return typeof payload.exp === 'number' && payload.exp * 1000 > Date.now()
  } catch {
    return false
  }
}

// stato condiviso a livello modulo — un'unica istanza per tutta l'app
const user = ref<MeResponse | null>(null)
const checked = ref(false)

export function useAuth() {
  const isAuthenticated = computed(() => user.value !== null)
  const isAdmin = computed(() => user.value?.role === 'Admin')

  async function checkAuth(): Promise<void> {
    if (checked.value) return
    const token = getToken()
    if (!token || !isTokenValid(token)) {
      user.value = null
      checked.value = true
      return
    }
    try {
      const me = await authApi.me()
      user.value = me
      useTheme().initFromUser(me.themePreference)
    } catch {
      removeToken()
      user.value = null
    } finally {
      checked.value = true
    }
  }

  async function login(userName: string, password: string): Promise<void> {
    const response = await authApi.login({ userName, password })
    saveToken(response.token)
    user.value = {
      id: '',
      userName: response.userName,
      email: '',
      dateOfBirth: '',
      role: response.role,
      themePreference: 'light'
    }
    checked.value = true
  }

  async function logout(): Promise<void> {
    await authApi.logout()
    removeToken()
    user.value = null
    checked.value = false
  }

  function resetAuth(): void {
    removeToken()
    user.value = null
    checked.value = false
  }

  return { user, isAuthenticated, isAdmin, checked, checkAuth, login, logout, resetAuth }
}
