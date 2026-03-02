import { ref } from 'vue'
import type { Brand } from '@/Interfaces/Brand'
import { useAsyncState } from '@/core/composables/useAsyncState'
import { createBrand, deleteBrand, getBrands, updateBrand } from '@/modules/brands/api/brands.api'

type BrandFormMode = 'create' | 'edit'

const cacheTtlMs = 60_000
let brandsCache: Brand[] | null = null
let brandsCacheAt = 0

export const useBrands = () => {
  const { isLoading, errorMessage, run } = useAsyncState()
  const brands = ref<Brand[]>([])
  const isCreating = ref(false)
  const formMode = ref<BrandFormMode>('create')
  const selectedBrand = ref<Brand | null>(null)

  const updateCache = (items: Brand[]) => {
    brandsCache = [...items]
    brandsCacheAt = Date.now()
  }

  const loadBrands = async (force = false) => {
    const hasValidCache = !force && brandsCache && Date.now() - brandsCacheAt < cacheTtlMs
    if (hasValidCache && brandsCache) {
      brands.value = [...brandsCache]
      return
    }

    const result = await run(() => getBrands())
    if (result) {
      brands.value = result
      updateCache(result)
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

      const updated = await run(async () => {
        await updateBrand(updatedBrand)
        return updatedBrand
      })

      if (updated) {
        const index = brands.value.findIndex((brand) => brand.id === updated.id)
        if (index >= 0) {
          brands.value[index] = updated
        }

        updateCache(brands.value)

        selectedBrand.value = null
        isCreating.value = false
      }

      return
    }

    const created = await run(() => createBrand({ name }))
    if (created) {
      brands.value.push(created)
      updateCache(brands.value)
      isCreating.value = false
    }
  }

  const removeBrand = async (brand: Brand) => {
    const removed = await run(async () => {
      await deleteBrand(brand.id)
      return brand.id
    })

    if (removed) {
      brands.value = brands.value.filter((item) => item.id !== removed)
      updateCache(brands.value)
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
