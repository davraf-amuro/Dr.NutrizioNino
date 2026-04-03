import { ref } from 'vue'
import type { Category } from '@/Interfaces/Category'
import { useAsyncState } from '@/core/composables/useAsyncState'
import {
  createCategory,
  deleteCategory,
  getCategories,
  updateCategory
} from '@/modules/categories/api/categories.api'

type CategoryFormMode = 'create' | 'edit'

const cacheTtlMs = 60_000
let categoriesCache: Category[] | null = null
let categoriesCacheAt = 0

export const useCategories = () => {
  const { isLoading, errorMessage, run } = useAsyncState()
  const categories = ref<Category[]>([])
  const isCreating = ref(false)
  const formMode = ref<CategoryFormMode>('create')
  const selectedCategory = ref<Category | null>(null)

  const updateCache = (items: Category[]) => {
    categoriesCache = [...items]
    categoriesCacheAt = Date.now()
  }

  const loadCategories = async (force = false) => {
    const hasValidCache = !force && categoriesCache && Date.now() - categoriesCacheAt < cacheTtlMs
    if (hasValidCache && categoriesCache) {
      categories.value = [...categoriesCache]
      return
    }

    const result = await run(() => getCategories())
    if (result) {
      categories.value = result
      updateCache(result)
    }
  }

  const startCreateCategory = () => {
    formMode.value = 'create'
    selectedCategory.value = null
    isCreating.value = true
  }

  const startEditCategory = (category: Category) => {
    formMode.value = 'edit'
    selectedCategory.value = { ...category }
    isCreating.value = true
  }

  const cancelCreateCategory = () => {
    selectedCategory.value = null
    isCreating.value = false
  }

  const submitCategory = async (name: string) => {
    if (formMode.value === 'edit' && selectedCategory.value) {
      const updated: Category = { ...selectedCategory.value, name }

      const result = await run(async () => {
        await updateCategory(updated)
        return updated
      })

      if (result) {
        const index = categories.value.findIndex((c) => c.id === result.id)
        if (index >= 0) categories.value[index] = result
        updateCache(categories.value)
        selectedCategory.value = null
        isCreating.value = false
      }
      return
    }

    const created = await run(() => createCategory({ name }))
    if (created) {
      categories.value.push(created)
      updateCache(categories.value)
      isCreating.value = false
    }
  }

  const removeCategory = async (category: Category) => {
    const removed = await run(async () => {
      await deleteCategory(category.id)
      return category.id
    })

    if (removed) {
      categories.value = categories.value.filter((c) => c.id !== removed)
      updateCache(categories.value)
    }
  }

  return {
    categories,
    isCreating,
    formMode,
    selectedCategory,
    isLoading,
    errorMessage,
    loadCategories,
    startCreateCategory,
    startEditCategory,
    cancelCreateCategory,
    submitCategory,
    removeCategory
  }
}
