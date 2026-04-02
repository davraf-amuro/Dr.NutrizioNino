import { ref, computed } from 'vue'
import { authApi } from '@/modules/auth/api/auth.api'
import type { MeResponse } from '@/Interfaces/auth/AuthDto'

// stato condiviso a livello modulo — un'unica istanza per tutta l'app
const user = ref<MeResponse | null>(null)
const checked = ref(false)

export function useAuth() {
  const isAuthenticated = computed(() => user.value !== null)
  const isAdmin = computed(() => user.value?.role === 'Admin')

  async function checkAuth(): Promise<void> {
    if (checked.value) return
    try {
      user.value = await authApi.me()
    } catch {
      user.value = null
    } finally {
      checked.value = true
    }
  }

  async function login(userName: string, password: string): Promise<void> {
    const response = await authApi.login({ userName, password })
    // il cookie è settato nella risposta — non chiamiamo /me subito perché
    // il browser potrebbe non averlo ancora disponibile nella richiesta sincrona successiva.
    // Usiamo i dati già presenti nella LoginResponse per popolare lo stato minimo.
    user.value = {
      id: '',
      userName: response.userName,
      email: '',
      dateOfBirth: '',
      role: response.role
    }
    checked.value = true
  }

  async function logout(): Promise<void> {
    await authApi.logout()
    user.value = null
    checked.value = false
  }

  function resetAuth(): void {
    user.value = null
    checked.value = false
  }

  return { user, isAuthenticated, isAdmin, checked, checkAuth, login, logout, resetAuth }
}
