import { ref } from 'vue'
import type { Brand } from '@/Interfaces/Brand'
import { createBrand, deleteBrand, getBrands, updateBrand } from '@/modules/brands/api/brands.api'

type BrandFormMode = 'create' | 'edit'

export const useBrands = () => {
  const brands = ref<Brand[]>([])
  const isCreating = ref(false)
  const formMode = ref<BrandFormMode>('create')
  const selectedBrand = ref<Brand | null>(null)
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
    formMode.value = 'create'
    selectedBrand.value = null
    isCreating.value = true
  }

  const startEditBrand = (brand: Brand) => {
    formMode.value = 'edit'
    selectedBrand.value = { ...brand }
    isCreating.value = true
  }

  const cancelCreateBrand = () => {
    selectedBrand.value = null
    isCreating.value = false
  }

  const submitCreateBrand = async (name: string) => {
    if (formMode.value === 'edit' && selectedBrand.value) {
      const updatedBrand: Brand = {
        ...selectedBrand.value,
        name
      }

      const updated = await withErrorHandling(async () => {
        await updateBrand(updatedBrand)
        return updatedBrand
      })

      if (updated) {
        const index = brands.value.findIndex((brand) => brand.id === updated.id)
        if (index >= 0) {
          brands.value[index] = updated
        }

        selectedBrand.value = null
        isCreating.value = false
      }

      return
    }

    const created = await withErrorHandling(() => createBrand({ name }))
    if (created) {
      brands.value.push(created)
      isCreating.value = false
    }
  }

  const removeBrand = async (brand: Brand) => {
    const removed = await withErrorHandling(async () => {
      await deleteBrand(brand.id)
      return brand.id
    })

    if (removed) {
      brands.value = brands.value.filter((item) => item.id !== removed)
    }
  }

  return {
    brands,
    isCreating,
    formMode,
    selectedBrand,
    isLoading,
    errorMessage,
    loadBrands,
    startCreateBrand,
    startEditBrand,
    cancelCreateBrand,
    submitCreateBrand,
    removeBrand
  }
}
