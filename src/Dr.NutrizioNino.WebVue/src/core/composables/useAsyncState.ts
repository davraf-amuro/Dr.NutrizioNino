import { computed, ref } from 'vue'

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
      errorMessage.value = error instanceof Error ? error.message : fallbackErrorMessage
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
