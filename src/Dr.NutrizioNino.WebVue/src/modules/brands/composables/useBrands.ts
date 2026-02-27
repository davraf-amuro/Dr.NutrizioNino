import { ref } from 'vue'
import type { Brand } from '@/Interfaces/Brand'
import { createBrand, getBrands } from '@/modules/brands/api/brands.api'

export const useBrands = () => {
  const brands = ref<Brand[]>([])
  const isCreating = ref(false)
  const isLoading = ref(false)
  const errorMessage = ref<string | null>(null)

  const withErrorHandling = async <T>(operation: () => Promise<T>): Promise<T | null> => {
    isLoading.value = true
    errorMessage.value = null

    try {
      return await operation()
    } catch (error) {
      errorMessage.value = error instanceof Error ? error.message : 'Errore imprevisto.'
      return null
    } finally {
      isLoading.value = false
    }
  }

  const loadBrands = async () => {
    const result = await withErrorHandling(() => getBrands())
    if (result) {
      brands.value = result
    }
  }

  const startCreateBrand = () => {
    isCreating.value = true
  }

  const cancelCreateBrand = () => {
    isCreating.value = false
  }

  const submitCreateBrand = async (name: string) => {
    const created = await withErrorHandling(() => createBrand({ name }))
    if (created) {
      brands.value.push(created)
      isCreating.value = false
    }
  }

  return {
    brands,
    isCreating,
    isLoading,
    errorMessage,
    loadBrands,
    startCreateBrand,
    cancelCreateBrand,
    submitCreateBrand
  }
}
