<template>
  <n-space vertical size="medium">
    <n-h3 style="margin: 0">{{ isEditMode ? 'Modifica alimento' : 'Nuovo alimento' }}</n-h3>

    <n-spin :show="isSubmitting">
      <n-form ref="formRef" :model="localFood" :rules="rules" label-placement="left" label-width="140" label-align="right">
        <n-form-item label="Nome" path="name">
          <n-input v-model:value="localFood.name" :maxlength="50" :disabled="isSubmitting" />
        </n-form-item>

        <n-form-item label="Barcode" path="barcode">
          <n-input v-model:value="localFood.barcode" :maxlength="50" :disabled="isSubmitting" clearable />
        </n-form-item>

        <n-form-item label="Marca" path="brandId">
          <n-input-group>
            <n-select
              v-model:value="localFood.brandId"
              :options="brandOptions"
              placeholder="-seleziona-"
              clearable
              :disabled="isSubmitting"
              style="flex: 1; min-width: 0"
            />
            <n-button
              :disabled="isSubmitting"
              aria-label="Aggiungi nuova marca"
              @click="showBrandModal = true"
            >+</n-button>
          </n-input-group>
        </n-form-item>

        <n-grid :cols="2" :x-gap="12">
          <n-gi>
            <n-form-item label="Unità di misura" path="unitOfMeasureId">
              <n-input-group>
                <n-select
                  v-model:value="localFood.unitOfMeasureId"
                  :options="unitOptions"
                  :disabled="isSubmitting"
                  style="flex: 1; min-width: 0"
                />
                <n-button
                  :disabled="isSubmitting"
                  aria-label="Aggiungi nuova unità di misura"
                  @click="showUnitModal = true"
                >+</n-button>
              </n-input-group>
            </n-form-item>
          </n-gi>
          <n-gi>
            <n-form-item label="Quantità" path="quantity">
              <n-input-number
                v-model:value="localFood.quantity"
                :min="0"
                :max="9999"
                :precision="1"
                :show-button="false"
                :disabled="isSubmitting"
                style="width: 100%"
              />
            </n-form-item>
          </n-gi>
        </n-grid>

        <n-form-item label="Calorie (kcal)" path="calorie">
          <n-input-number
            v-model:value="localFood.calorie"
            :min="0"
            :max="99999"
            :precision="1"
            :show-button="false"
            :disabled="isSubmitting"
            style="width: 100%"
          />
        </n-form-item>
      </n-form>

      <n-divider title-placement="left">Nutrienti</n-divider>

      <n-space vertical size="small">
        <FoodNutrientInput
          v-for="fnu in sortedNutrients"
          :key="fnu.nutrientId"
          :food-nutrient-dto="fnu"
          :units-of-measures="unitsOfMeasures"
          @update="updateNutrient"
        />
      </n-space>
    </n-spin>

    <BrandQuickAddModal v-model:show="showBrandModal" @created="onBrandCreated" />
    <UnitQuickAddModal v-model:show="showUnitModal" @created="onUnitCreated" />

    <n-space justify="space-between">
      <n-button type="error" @click="cancelHandler" :disabled="isSubmitting">Annulla</n-button>
      <n-button type="primary" @click="completeHandler" :loading="isSubmitting" :disabled="!localFood.name.trim()">
        {{ isEditMode ? 'Aggiorna' : 'Salva' }}
      </n-button>
    </n-space>
  </n-space>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { sortNutrients } from '@/core/utils/sortNutrients'
import {
  NButton,
  NDivider,
  NForm,
  NFormItem,
  NGi,
  NGrid,
  NH3,
  NInput,
  NInputGroup,
  NInputNumber,
  NSelect,
  NSpace,
  NSpin,
  type FormInst,
  type FormRules,
  type SelectOption
} from 'naive-ui'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import type { Brand } from '@/Interfaces/Brand'
import FoodNutrientInput from './FoodNutrientInput.vue'
import BrandQuickAddModal from './BrandQuickAddModal.vue'
import UnitQuickAddModal from './UnitQuickAddModal.vue'

const props = defineProps<{
  food: FoodDto
  mode?: 'create' | 'edit'
  brands: Brand[]
  unitsOfMeasures: UnitOfMeasureDto[]
  isSubmitting?: boolean
}>()

const emit = defineEmits<{
  cancel: []
  complete: [food: FoodDto]
  'brand-created': [brand: Brand]
  'unit-created': [unit: UnitOfMeasureDto]
}>()

const formRef = ref<FormInst | null>(null)
const emptyGuid = '00000000-0000-0000-0000-000000000000'

const isSubmitting = computed(() => props.isSubmitting ?? false)
const isEditMode = computed(() => props.mode === 'edit')

const rules: FormRules = {
  name: [{ required: true, message: 'Il nome è obbligatorio', min: 3, trigger: 'blur' }],
  unitOfMeasureId: [{ required: true, message: "L'unità di misura è obbligatoria", trigger: 'change' }]
}

const brandOptions = computed<SelectOption[]>(() =>
  props.brands.map((brand) => ({ label: brand.name, value: brand.id }))
)

const unitOptions = computed<SelectOption[]>(() =>
  props.unitsOfMeasures.map((unit) => ({ label: unit.name, value: unit.id }))
)

const cloneFood = (food: FoodDto): FoodDto => ({
  ...food,
  nutrients: food.nutrients.map((nutrient) => ({ ...nutrient }))
})

const ensureBrandSelection = () => {
  if (!props.brands.length) return
  const brandId = localFood.value.brandId
  const hasValidBrand = !!brandId && brandId !== emptyGuid && props.brands.some((b) => b.id === brandId)
  if (!hasValidBrand) localFood.value.brandId = null
}

const localFood = ref(cloneFood(props.food))
const sortedNutrients = computed(() => sortNutrients(localFood.value.nutrients))

watch(() => props.food, (newFood) => {
  localFood.value = cloneFood(newFood)
  ensureBrandSelection()
}, { immediate: true })

watch(() => props.brands, () => ensureBrandSelection(), { immediate: true })

const showBrandModal = ref(false)
const showUnitModal = ref(false)

const onBrandCreated = (brand: Brand) => {
  localFood.value.brandId = brand.id
  emit('brand-created', brand)
}

const onUnitCreated = (unit: UnitOfMeasureDto) => {
  localFood.value.unitOfMeasureId = unit.id
  emit('unit-created', unit)
}

const cancelHandler = () => emit('cancel')

const completeHandler = () => {
  formRef.value?.validate((errors) => {
    if (!errors) emit('complete', cloneFood(localFood.value))
  })
}

const updateNutrient = (updatedNutrient: FoodDto['nutrients'][number]) => {
  const idx = localFood.value.nutrients.findIndex((n) => n.nutrientId === updatedNutrient.nutrientId)
  if (idx >= 0) localFood.value.nutrients[idx] = { ...updatedNutrient }
}
</script>
