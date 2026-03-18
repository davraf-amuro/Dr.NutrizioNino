import { ref, type Ref } from 'vue'
import { useAsyncState } from './useAsyncState'

export type ListDetailMode = 'create' | 'edit'

export interface ListDetailApi<T, TCreate> {
  getAll: () => Promise<T[]>
  getById: (id: string) => Promise<T>
  create: (payload: TCreate) => Promise<T>
  update: (payload: T) => Promise<void>
  remove: (id: string) => Promise<void>
}

/**
 * Factory that returns a composable function with shared module-level cache.
 * The cache is shared across all component instances; reactive state (refs) is per-instance.
 */
export const createListDetail = <T extends { id: string }, TCreate>(
  api: ListDetailApi<T, TCreate>,
  cacheTtlMs = 60_000
) => {
  let cache: T[] | null = null
  let cacheAt = 0

  return () => {
    const { isLoading, errorMessage, run } = useAsyncState()
    const items = ref<T[]>([]) as unknown as Ref<T[]>
    const selectedItem = ref<T | null>(null) as unknown as Ref<T | null>
    const isEditing = ref(false)
    const formMode = ref<ListDetailMode>('create')

    const updateCache = (list: T[]) => {
      cache = [...list]
      cacheAt = Date.now()
    }

    const load = async (force = false) => {
      if (!force && cache && Date.now() - cacheAt < cacheTtlMs) {
        items.value = [...cache]
        return
      }
      const result = await run(() => api.getAll())
      if (result) {
        items.value = result
        updateCache(result)
      }
    }

    const startCreate = () => {
      formMode.value = 'create'
      selectedItem.value = null
      isEditing.value = true
    }

    const startEdit = async (item: T) => {
      formMode.value = 'edit'
      const result = await run(() => api.getById(item.id))
      if (result) {
        selectedItem.value = result
        isEditing.value = true
      }
    }

    const cancelEdit = () => {
      selectedItem.value = null
      isEditing.value = false
    }

    const submit = async (payload: TCreate | T) => {
      if (formMode.value === 'edit' && selectedItem.value) {
        const result = await run(async () => {
          await api.update(payload as T)
          return payload as T
        })
        if (result) {
          const index = items.value.findIndex((i) => i.id === result.id)
          if (index >= 0) items.value[index] = result
          updateCache(items.value)
          selectedItem.value = null
          isEditing.value = false
        }
        return
      }

      const created = await run(() => api.create(payload as TCreate))
      if (created) {
        items.value.push(created)
        updateCache(items.value)
        isEditing.value = false
      }
    }

    const remove = async (item: T) => {
      const removedId = await run(async () => {
        await api.remove(item.id)
        return item.id
      })
      if (removedId) {
        items.value = items.value.filter((i) => i.id !== removedId)
        updateCache(items.value)
      }
    }

    return {
      items,
      isEditing,
      formMode,
      selectedItem,
      isLoading,
      errorMessage,
      load,
      startCreate,
      startEdit,
      cancelEdit,
      submit,
      remove
    }
  }
}
