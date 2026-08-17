<template>
  <n-space vertical size="large" class="recipe-comparison-page">
    <n-card title="Confronto ricette" size="large">
      <template #header-extra>
        <n-button quaternary :disabled="isLoading" @click="reset">Azzera</n-button>
      </template>

      <n-space vertical size="medium">
        <n-alert v-if="errorMessage" type="error" :show-icon="true" :bordered="false">
          {{ errorMessage }}
        </n-alert>

        <n-text depth="3">
          Scegli da {{ minSlots }} a {{ maxSlots }} ricette con la quantità da confrontare.
        </n-text>

        <n-grid :cols="gridCols" responsive="screen" :x-gap="12" :y-gap="12">
          <n-gi v-for="(slot, index) in slots" :key="index">
            <n-card size="small" :title="`Ricetta ${index + 1}`">
              <template #header-extra>
                <n-button
                  text
                  size="tiny"
                  :disabled="slot.recipeId === null"
                  @click="clearSlot(index)"
                >
                  Rimuovi
                </n-button>
              </template>

              <n-space vertical size="small">
                <n-select
                  v-model:value="slot.recipeId"
                  :options="recipeOptions"
                  :loading="isLoading"
                  placeholder="Seleziona ricetta"
                  filterable
                  clearable
                  :aria-label="`Ricetta ${index + 1}`"
                />
                <n-input-number
                  v-model:value="slot.quantityGrams"
                  :min="1"
                  :max="maxQuantityGrams"
                  :step="10"
                  :aria-label="`Quantità in grammi della ricetta ${index + 1}`"
                >
                  <template #suffix>g</template>
                </n-input-number>
              </n-space>
            </n-card>
          </n-gi>
        </n-grid>

        <n-alert v-if="hasDuplicates" type="warning" :show-icon="true" :bordered="false">
          Le ricette da confrontare devono essere diverse tra loro.
        </n-alert>

        <n-space>
          <n-button type="primary" :disabled="!canCompare" :loading="isLoading" @click="compare">
            Confronta
          </n-button>
        </n-space>

        <n-alert
          v-if="staleRecipeNames.length > 0"
          type="warning"
          :show-icon="true"
          :bordered="false"
        >
          Valori nutrizionali da ricalcolare per: {{ staleRecipeNames.join(', ') }}.
        </n-alert>

        <RecipeComparisonTable :columns="columns" :rows="mergedRows" />
      </n-space>
    </n-card>
  </n-space>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import {
  NAlert,
  NButton,
  NCard,
  NGi,
  NGrid,
  NInputNumber,
  NSelect,
  NSpace,
  NText
} from 'naive-ui'
import RecipeComparisonTable from '@/components/Recipes/RecipeComparisonTable.vue'
import {
  maxQuantityGrams,
  maxSlots,
  minSlots,
  useRecipeComparison
} from '@/modules/recipes/composables/useRecipeComparison'

const {
  availableRecipes,
  slots,
  columns,
  mergedRows,
  staleRecipeNames,
  hasDuplicates,
  canCompare,
  isLoading,
  errorMessage,
  loadRecipes,
  compare,
  clearSlot,
  reset
} = useRecipeComparison()

const gridCols = '1 s:2 m:3'

const recipeOptions = computed(() =>
  availableRecipes.value.map((recipe) => ({ label: recipe.name, value: recipe.id }))
)

onMounted(async () => {
  await loadRecipes()
})
</script>

<style scoped>
.recipe-comparison-page {
  width: 100%;
}
</style>
