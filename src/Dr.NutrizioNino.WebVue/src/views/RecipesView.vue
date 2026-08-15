<template>
  <n-space vertical size="large" class="recipes-page">
    <n-card title="Ricette" size="large">
      <template #header-extra>
        <n-button v-if="!isCreating && !recipeDetail" type="primary" :loading="isLoading" @click="startCreate">
          Nuova ricetta
        </n-button>
      </template>

      <n-space vertical size="medium">
        <n-alert v-if="errorMessage" type="error" :show-icon="true" :bordered="false">
          {{ errorMessage }}
        </n-alert>

        <RecipesList
          v-if="!isCreating && !recipeDetail"
          :recipes="recipes"
          @detail="handleViewRecipe"
          @delete="handleDeleteRecipe"
        />

        <RecipeBuilder
          v-if="isCreating"
          :available-foods="availableFoods"
          :is-loading="isLoading"
          @cancel="cancelCreate"
          @save="completeRecipe"
        />

        <n-spin v-if="recipeDetail === null && isLoading" :show="true" />

        <RecipeDetail
          v-if="recipeDetail"
          :recipe="recipeDetail"
          @close="closeDetail"
        />
      </n-space>
    </n-card>
  </n-space>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { NAlert, NButton, NCard, NSpace, NSpin, useDialog } from 'naive-ui'
import RecipesList from '@/components/Recipes/RecipesList.vue'
import RecipeBuilder from '@/components/Recipes/RecipeBuilder.vue'
import RecipeDetail from '@/components/Recipes/RecipeDetail.vue'
import { useRecipes } from '@/modules/recipes/composables/useRecipes'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'

const {
  recipes,
  availableFoods,
  isCreating,
  recipeDetail,
  isLoading,
  errorMessage,
  loadRecipes,
  loadAvailableFoods,
  startCreate,
  cancelCreate,
  completeRecipe,
  removeRecipe,
  viewRecipe,
  closeDetail
} = useRecipes()

const dialog = useDialog()

const handleViewRecipe = async (recipe: FoodDashboardDto) => {
  await viewRecipe(recipe)
}

const handleDeleteRecipe = (recipe: FoodDashboardDto) => {
  dialog.warning({
    title: 'Conferma eliminazione',
    content: `Eliminare la ricetta "${recipe.name}"?`,
    positiveText: 'Elimina',
    negativeText: 'Annulla',
    onPositiveClick: () => removeRecipe(recipe)
  })
}

onMounted(async () => {
  await Promise.all([loadRecipes(), loadAvailableFoods()])
})
</script>

<style scoped>
.recipes-page {
  width: 100%;
}
</style>
