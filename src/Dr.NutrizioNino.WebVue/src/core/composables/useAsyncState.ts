import { computed, ref } from 'vue'
import { ApiError } from '@/core/http/ApiError'

const errorMessages: Record<number, string> = {
  400: 'Richiesta non valida.',
  401: 'Non autorizzato. Effettua il login.',
  403: 'Accesso negato.',
  404: 'Risorsa non trovata.',
  422: 'Dati non validi.',
  500: 'Errore interno del server.'
}

const fallbackErrorMessage = 'Errore imprevisto.'

export const useAsyncState = () => {
  const pendingCount = ref(0)
  const errorMessage = ref<string | null>(null)
  const isLoading = computed(() => pendingCount.value > 0)

  const run = async <T>(operation: () => Promise<T>): Promise<T | null> => {
    pendingCount.value += 1
    errorMessage.value = null

    try {
      return await operation()
    } catch (error) {
      if (error instanceof ApiError && error.status) {
        errorMessage.value = errorMessages[error.status] ?? error.message ?? fallbackErrorMessage
      } else {
        errorMessage.value = error instanceof Error ? error.message : fallbackErrorMessage
      }
      return null
    } finally {
      pendingCount.value -= 1
    }
  }

  return {
    isLoading,
    errorMessage,
    run
  }
}
