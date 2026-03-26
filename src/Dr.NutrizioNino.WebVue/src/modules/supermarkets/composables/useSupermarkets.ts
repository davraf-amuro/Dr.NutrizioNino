import { ref } from 'vue'
import type { Supermarket } from '@/Interfaces/Supermarket'
import { useAsyncState } from '@/core/composables/useAsyncState'
import {
  createSupermarket,
  deleteSupermarket,
  getSupermarkets,
  updateSupermarket
} from '@/modules/supermarkets/api/supermarkets.api'

type SupermarketFormMode = 'create' | 'edit'

const cacheTtlMs = 60_000
let supermarketsCache: Supermarket[] | null = null
let supermarketsCacheAt = 0

export const useSupermarkets = () => {
  const { isLoading, errorMessage, run } = useAsyncState()
  const supermarkets = ref<Supermarket[]>([])
  const isCreating = ref(false)
  const formMode = ref<SupermarketFormMode>('create')
  const selectedSupermarket = ref<Supermarket | null>(null)

  const updateCache = (items: Supermarket[]) => {
    supermarketsCache = [...items]
    supermarketsCacheAt = Date.now()
  }

  const loadSupermarkets = async (force = false) => {
    const hasValidCache = !force && supermarketsCache && Date.now() - supermarketsCacheAt < cacheTtlMs
    if (hasValidCache && supermarketsCache) {
      supermarkets.value = [...supermarketsCache]
      return
    }

    const result = await run(() => getSupermarkets())
    if (result) {
      supermarkets.value = result
      updateCache(result)
    }
  }

  const startCreateSupermarket = () => {
    formMode.value = 'create'
    selectedSupermarket.value = null
    isCreating.value = true
  }

  const startEditSupermarket = (supermarket: Supermarket) => {
    formMode.value = 'edit'
    selectedSupermarket.value = { ...supermarket }
    isCreating.value = true
  }

  const cancelCreateSupermarket = () => {
    selectedSupermarket.value = null
    isCreating.value = false
  }

  const submitSupermarket = async (name: string) => {
    if (formMode.value === 'edit' && selectedSupermarket.value) {
      const updated: Supermarket = { ...selectedSupermarket.value, name }

      const result = await run(async () => {
        await updateSupermarket(updated)
        return updated
      })

      if (result) {
        const index = supermarkets.value.findIndex((s) => s.id === result.id)
        if (index >= 0) supermarkets.value[index] = result
        updateCache(supermarkets.value)
        selectedSupermarket.value = null
        isCreating.value = false
      }
      return
    }

    const created = await run(() => createSupermarket({ name }))
    if (created) {
      supermarkets.value.push(created)
      updateCache(supermarkets.value)
      isCreating.value = false
    }
  }

  const removeSupermarket = async (supermarket: Supermarket) => {
    const removed = await run(async () => {
      await deleteSupermarket(supermarket.id)
      return supermarket.id
    })

    if (removed) {
      supermarkets.value = supermarkets.value.filter((s) => s.id !== removed)
      updateCache(supermarkets.value)
    }
  }

  return {
    supermarkets,
    isCreating,
    formMode,
    selectedSupermarket,
    isLoading,
    errorMessage,
    loadSupermarkets,
    startCreateSupermarket,
    startEditSupermarket,
    cancelCreateSupermarket,
    submitSupermarket,
    removeSupermarket
  }
}
