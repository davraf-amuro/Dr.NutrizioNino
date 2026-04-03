<script setup lang="ts">
import { onMounted } from 'vue'
import { NAlert, NButton, NCard, NSpace, useDialog, useMessage } from 'naive-ui'
import CategoriesList from '@/components/Categories/CategoriesList.vue'
import CategoryDetail from '@/components/Categories/CategoryDetail.vue'
import { useCategories } from '@/modules/categories/composables/useCategories'
import { isCategoryInUse } from '@/modules/categories/api/categories.api'
import type { Category } from '@/Interfaces/Category'

const {
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
} = useCategories()

const message = useMessage()
const dialog = useDialog()

const handleDeleteCategory = async (category: Category) => {
  const inUse = await isCategoryInUse(category.id)
  if (inUse) {
    message.warning(`La categoria "${category.name}" è collegata a uno o più alimenti e non può essere eliminata.`)
    return
  }

  dialog.warning({
    title: 'Conferma eliminazione',
    content: `Eliminare la categoria "${category.name}"?`,
    positiveText: 'Elimina',
    negativeText: 'Annulla',
    onPositiveClick: () => removeCategory(category)
  })
}

onMounted(async () => {
  await loadCategories()
})
</script>

<template>
  <n-space vertical size="large" class="categories-page">
    <n-card title="Categorie" size="large">
      <template #header-extra>
        <n-button
          v-if="!isCreating"
          type="primary"
          @click="startCreateCategory"
          :loading="isLoading"
        >
          Nuova categoria
        </n-button>
      </template>

      <n-space vertical size="medium">
        <n-alert v-if="errorMessage" type="error" :show-icon="true" :bordered="false">
          {{ errorMessage }}
        </n-alert>

        <CategoriesList
          v-if="!isCreating"
          :categories="categories"
          @edit="startEditCategory"
          @delete="handleDeleteCategory"
        />

        <CategoryDetail
          v-else
          :mode="formMode"
          :category="selectedCategory"
          :is-submitting="isLoading"
          @save="submitCategory"
          @cancel="cancelCreateCategory"
        />
      </n-space>
    </n-card>
  </n-space>
</template>

<style scoped>
.categories-page {
  width: 100%;
}
</style>
