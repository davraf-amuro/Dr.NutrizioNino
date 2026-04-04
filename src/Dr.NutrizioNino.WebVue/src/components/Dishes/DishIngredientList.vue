<template>
  <n-space vertical size="small">
    <n-auto-complete
      v-model:value="searchInput"
      :options="autocompleteOptions"
      placeholder="Cerca alimento da aggiungere..."
      clearable
      :get-show="() => true"
      :render-label="renderFoodLabel"
      @select="onFoodSelect"
      aria-label="Cerca alimento"
    />

    <n-data-table
      v-if="ingredients.length > 0"
      :columns="columns"
      :data="ingredients"
      :row-key="(row) => row.food.id"
      :single-line="false"
      :bordered="true"
      :pagination="false"
      :summary="() => ({ food: { value: 'Totale', colSpan: 1 }, quantityGrams: { value: totalWeight + ' g' }, remove: { value: '' } })"
      aria-label="Lista ingredienti"
    />
  </n-space>
</template>

<script setup lang="ts">
import { computed, h, nextTick, ref } from 'vue'
import {
  NAutoComplete,
  NButton,
  NDataTable,
  NInputNumber,
  NSpace,
  type DataTableColumns
} from 'naive-ui'
import type { FoodDashboardDto } from '@/Interfaces/foods/FoodDashboardDto'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'
import type { DishIngredient } from '@/core/composables/useDishCalculator'

const props = defineProps<{
  ingredients: DishIngredient[]
  availableFoods: FoodDashboardDto[]
  isLoading?: boolean
}>()

const emit = defineEmits<{
  'add-food': [foodId: string]
  'update-quantity': [foodId: string, quantity: number]
  remove: [foodId: string]
}>()

const searchInput = ref('')

const autocompleteOptions = computed(() =>
  props.availableFoods
    .filter(
      (f) =>
        f.name?.toLowerCase().includes(searchInput.value.toLowerCase()) &&
        !props.ingredients.some((i) => i.food.id === f.id)
    )
    .map((f) => ({ label: f.name ?? '', value: f.id }))
)

const renderFoodLabel = (option: { label: string; value: string | number }) => {
  const food = props.availableFoods.find((f) => f.id === option.value)
  return h('div', { style: 'line-height: 1.4; padding: 2px 0' }, [
    h('div', option.label),
    food?.brandDescription
      ? h('div', { style: 'font-size: 12px; color: #aaa' }, food.brandDescription)
      : null
  ])
}

const totalWeight = computed(() => props.ingredients.reduce((s, i) => s + i.quantityGrams, 0))

const onFoodSelect = (foodId: string) => {
  emit('add-food', foodId)
  nextTick(() => { searchInput.value = '' })
}

const columns: DataTableColumns<DishIngredient> = [
  {
    title: 'Alimento',
    key: 'food',
    render: (row) => {
      const brand = props.availableFoods.find((f) => f.id === row.food.id)?.brandDescription
      return brand ? `${row.food.name} (${brand})` : row.food.name
    }
  },
  {
    title: 'Quantità (g)',
    key: 'quantityGrams',
    width: 140,
    render: (row) =>
      h(NInputNumber, {
        value: row.quantityGrams,
        min: 1,
        max: 9999,
        precision: 1,
        showButton: false,
        style: 'width: 100%',
        'onUpdate:value': (val: number | null) => {
          if (val !== null) emit('update-quantity', row.food.id, val)
        }
      })
  },
  {
    title: '',
    key: 'remove',
    width: 60,
    render: (row) =>
      h(NButton, { size: 'small', quaternary: true, type: 'error', onClick: () => emit('remove', row.food.id) }, { default: () => '✕' })
  }
]
</script>
