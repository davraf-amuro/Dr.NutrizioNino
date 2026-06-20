<template>
  <n-space vertical size="medium">
    <n-h3 style="margin: 0">{{ isEditMode ? 'Modifica alimento' : 'Nuovo alimento' }}</n-h3>

    <n-spin :show="isSubmitting">
      <n-form ref="formRef" :model="localFood" :rules="rules" label-placement="left" label-width="140" label-align="right">
        <n-form-item label="Nome" path="name">
          <n-input v-model:value="localFood.name" :maxlength="50" :disabled="isSubmitting" placeholder="Inserisci il nome dell'alimento" />
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
              filterable
              :disabled="isSubmitting"
              style="flex: 1; min-width: 0"
            />
            <n-button :disabled="isSubmitting" aria-label="Aggiungi nuova marca" @click="showBrandModal = true">+</n-button>
          </n-input-group>
        </n-form-item>

        <n-form-item label="Supermercati" path="supermarketIds">
          <n-input-group>
            <n-select
              v-model:value="localFood.supermarketIds"
              :options="supermarketOptions"
              placeholder="-seleziona-"
              multiple
              clearable
              filterable
              :disabled="isSubmitting"
              style="flex: 1; min-width: 0"
            />
            <n-button :disabled="isSubmitting" aria-label="Aggiungi nuovo supermercato" @click="showSupermarketModal = true">+</n-button>
          </n-input-group>
        </n-form-item>

        <n-form-item label="Categorie" path="categoryIds">
          <n-input-group>
            <n-select
              v-model:value="localFood.categoryIds"
              :options="categoryOptions"
              placeholder="-seleziona-"
              multiple
              clearable
              filterable
              :disabled="isSubmitting"
              style="flex: 1; min-width: 0"
            />
            <n-button :disabled="isSubmitting" aria-label="Aggiungi nuova categoria" @click="showCategoryModal = true">+</n-button>
          </n-input-group>
        </n-form-item>

        <n-grid :cols="2" :x-gap="12">
          <n-gi>
            <n-form-item label="Unità di misura" path="unitOfMeasureId">
              <n-input-group>
                <n-select
                  v-model:value="localFood.unitOfMeasureId"
                  :options="unitOptions"
                  filterable
                  :disabled="isSubmitting"
                  style="flex: 1; min-width: 0"
                />
                <n-button :disabled="isSubmitting" aria-label="Aggiungi nuova unità di misura" @click="showUnitModal = true">+</n-button>
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
      </n-form>

      <n-divider title-placement="left">Nutrienti</n-divider>

      <!-- Controlli AI vision -->
      <n-space size="small" align="center" style="margin-bottom: 8px; flex-wrap: wrap">
        <LlmProviderSelect v-model="selectedProviderKey" :disabled="isExtracting" />

        <n-button size="small" :loading="isExtracting" @click="triggerImagePaste">
          📷 Incolla immagine etichetta
        </n-button>

        <n-button
          v-if="pastedImageBase64 && !isExtracting"
          size="small"
          secondary
          @click="reanalyze"
        >
          🔄 Rivaluta
        </n-button>

        <n-button
          v-if="pastedImageUrl"
          size="small"
          quaternary
          @click="openImageInNewWindow"
          aria-label="Apri immagine etichetta in nuova finestra"
        >
          🔍 Apri immagine
        </n-button>

        <!-- Cronometro semaforo -->
        <n-space v-if="isExtracting" align="center" size="small">
          <span
            :style="{ display: 'inline-block', width: '10px', height: '10px', borderRadius: '50%', background: timerColor }"
            :aria-label="`Tempo trascorso: ${timer.formattedElapsed.value}`"
          />
          <span style="font-size: 12px; color: #888">{{ timer.formattedElapsed.value }}</span>
          <n-button size="tiny" type="error" @click="cancelExtraction">Annulla</n-button>
        </n-space>

        <span v-if="pastedImageUrl && !isExtracting" style="font-size: 12px; color: #888">Immagine incollata</span>
      </n-space>

      <!-- Anteprima immagine -->
      <div v-if="pastedImageUrl" style="margin-bottom: 12px; max-width: 200px">
        <img :src="pastedImageUrl" alt="Etichetta" style="max-width: 100%; border-radius: 6px; border: 1px solid #ddd" />
      </div>

      <!-- Conversioni effettuate dalla AI -->
      <n-alert v-if="extractionConversions.length > 0" type="info" :show-icon="false" style="margin-bottom: 8px; font-size: 12px">
        Conversioni: {{ extractionConversions.join(' | ') }}
      </n-alert>

      <!-- Riconciliazione: Caso A (IncompleteMatch) e Caso B (Unrecognized) -->
      <ExtractionReconciliation
        v-if="lastExtractionResults.length > 0"
        :extracted-nutrients="lastExtractionResults"
        :available-nutrients="availableNutrients"
        @alias-confirmed="onAliasConfirmed"
      />

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
    <SupermarketQuickAddModal v-model:show="showSupermarketModal" @created="onSupermarketCreated" />
    <CategoryQuickAddModal v-model:show="showCategoryModal" @created="onCategoryCreated" />

    <n-space justify="space-between">
      <n-button type="error" :disabled="isSubmitting" @click="cancelHandler">Annulla</n-button>
      <n-button type="primary" :loading="isSubmitting" :disabled="!localFood.name.trim()" @click="completeHandler">
        {{ isEditMode ? 'Aggiorna' : 'Salva' }}
      </n-button>
    </n-space>
  </n-space>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { sortNutrients } from '@/core/utils/sortNutrients'
import {
  NAlert,
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
  useMessage,
  type FormInst,
  type FormRules,
  type SelectOption
} from 'naive-ui'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'
import type { ExtractedNutrientDto } from '@/Interfaces/foods/ExtractedNutrientDto'
import type { NutrientDto } from '@/Interfaces/Nutrients/NutrientDto'
import { extractNutrientsFromImage } from '@/modules/foods/api/foods.api'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'
import type { Brand } from '@/Interfaces/Brand'
import type { Supermarket } from '@/Interfaces/Supermarket'
import type { Category } from '@/Interfaces/Category'
import FoodNutrientInput from './FoodNutrientInput.vue'
import BrandQuickAddModal from './BrandQuickAddModal.vue'
import UnitQuickAddModal from './UnitQuickAddModal.vue'
import SupermarketQuickAddModal from './SupermarketQuickAddModal.vue'
import CategoryQuickAddModal from './CategoryQuickAddModal.vue'
import LlmProviderSelect from './LlmProviderSelect.vue'
import ExtractionReconciliation from './ExtractionReconciliation.vue'
import { useExtractionTimer } from '@/composables/useExtractionTimer'

const props = defineProps<{
  food: FoodDto
  mode?: 'create' | 'edit'
  brands: Brand[]
  unitsOfMeasures: UnitOfMeasureDto[]
  supermarkets: Supermarket[]
  categories: Category[]
  availableNutrients?: NutrientDto[]
  isSubmitting?: boolean
}>()

const emit = defineEmits<{
  cancel: []
  complete: [food: FoodDto]
  'brand-created': [brand: Brand]
  'unit-created': [unit: UnitOfMeasureDto]
  'supermarket-created': [supermarket: Supermarket]
  'category-created': [category: Category]
}>()

const formRef = ref<FormInst | null>(null)
const emptyGuid = '00000000-0000-0000-0000-000000000000'

const isSubmitting = computed(() => props.isSubmitting ?? false)
const isEditMode = computed(() => props.mode === 'edit')
const availableNutrients = computed(() => props.availableNutrients ?? [])

const rules: FormRules = {
  name: [{ required: true, message: 'Il nome è obbligatorio', min: 3, trigger: 'blur' }],
  unitOfMeasureId: [{ required: true, message: "L'unità di misura è obbligatoria", trigger: 'change' }]
}

const brandOptions = computed<SelectOption[]>(() =>
  [...props.brands]
    .sort((a, b) => a.name.localeCompare(b.name, 'it'))
    .map((brand) => ({ label: brand.name, value: brand.id }))
)

const unitOptions = computed<SelectOption[]>(() =>
  [...props.unitsOfMeasures]
    .sort((a, b) => a.name.localeCompare(b.name, 'it'))
    .map((unit) => ({ label: unit.name, value: unit.id }))
)

const supermarketOptions = computed<SelectOption[]>(() =>
  [...props.supermarkets]
    .sort((a, b) => a.name.localeCompare(b.name, 'it'))
    .map((s) => ({ label: s.name, value: s.id }))
)

const categoryOptions = computed<SelectOption[]>(() =>
  [...props.categories]
    .sort((a, b) => a.name.localeCompare(b.name, 'it'))
    .map((c) => ({ label: c.name, value: c.id }))
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
const showSupermarketModal = ref(false)
const showCategoryModal = ref(false)

const onBrandCreated = (brand: Brand) => {
  localFood.value.brandId = brand.id
  emit('brand-created', brand)
}
const onUnitCreated = (unit: UnitOfMeasureDto) => {
  localFood.value.unitOfMeasureId = unit.id
  emit('unit-created', unit)
}
const onSupermarketCreated = (supermarket: Supermarket) => {
  localFood.value.supermarketIds = [...(localFood.value.supermarketIds ?? []), supermarket.id]
  emit('supermarket-created', supermarket)
}
const onCategoryCreated = (category: Category) => {
  localFood.value.categoryIds = [...(localFood.value.categoryIds ?? []), category.id]
  emit('category-created', category)
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

// ── AI vision ─────────────────────────────────────────────────
const message = useMessage()
const timer = useExtractionTimer()
const isExtracting = ref(false)
const pastedImageUrl = ref<string | null>(null)
const pastedImageBase64 = ref<string | null>(null)
const pastedMediaType = ref('image/jpeg')
const selectedProviderKey = ref('ollama')
const extractionConversions = ref<string[]>([])
const lastExtractionResults = ref<ExtractedNutrientDto[]>([])
let abortController: AbortController | null = null

const timerColor = computed(() => {
  const map = { idle: '#aaa', green: '#18a058', orange: '#f0a020', red: '#d03050' }
  return map[timer.status.value]
})

const cancelExtraction = () => {
  abortController?.abort()
  timer.stop()
  isExtracting.value = false
}

const openImageInNewWindow = () => {
  if (pastedImageUrl.value) window.open(pastedImageUrl.value, '_blank')
}

const handleImageExtraction = async (base64: string, mediaType: string) => {
  isExtracting.value = true
  extractionConversions.value = []
  lastExtractionResults.value = []
  abortController = new AbortController()
  timer.start()

  try {
    const results = await extractNutrientsFromImage(base64, selectedProviderKey.value, mediaType, abortController.signal)
    lastExtractionResults.value = results

    for (const extracted of results) {
      if (extracted.status !== 'Matched' || !extracted.matchedNutrientId) continue

      const target = localFood.value.nutrients.find((n) => n.nutrientId === extracted.matchedNutrientId)
      if (!target) continue

      const finalValue = extracted.convertedValue ?? extracted.value
      target.quantity = finalValue

      // Aggiorna anche l'unità di misura se l'AI ha fornito la canonical unit
      if (extracted.canonicalUnit) {
        const matchedUom = props.unitsOfMeasures.find(
          (u) => u.abbreviation?.toLowerCase() === extracted.canonicalUnit!.toLowerCase()
        )
        if (matchedUom) target.unitOfMeasureId = matchedUom.id
      }

      if (extracted.convertedValue !== null && extracted.canonicalUnit) {
        extractionConversions.value.push(
          `${extracted.value} ${extracted.unit} → ${extracted.convertedValue} ${extracted.canonicalUnit} (${extracted.name})`
        )
      }
    }
  } catch (err: unknown) {
    if (err instanceof Error && (err.name === 'CanceledError' || err.name === 'AbortError')) return
    const msg = err instanceof Error ? err.message : 'Errore sconosciuto'
    message.error(`Estrazione fallita: ${msg}`)
  } finally {
    timer.stop()
    isExtracting.value = false
  }
}

const reanalyze = async () => {
  if (!pastedImageBase64.value) return
  await handleImageExtraction(pastedImageBase64.value, pastedMediaType.value)
}

const onAliasConfirmed = (aiName: string, nutrientId: string) => {
  // Rimuovi dalla lista non riconosciuti dopo conferma alias
  lastExtractionResults.value = lastExtractionResults.value.filter((n) => n.name !== aiName)
}

const triggerImagePaste = () => {
  navigator.clipboard.read().then(async (items) => {
    for (const item of items) {
      const imageType = item.types.find((t) => t.startsWith('image/'))
      if (!imageType) continue
      const blob = await item.getType(imageType)
      const reader = new FileReader()
      reader.onload = async () => {
        const dataUrl = reader.result as string
        pastedImageUrl.value = dataUrl
        pastedImageBase64.value = dataUrl.split(',')[1]
        pastedMediaType.value = imageType
        await handleImageExtraction(pastedImageBase64.value!, imageType)
      }
      reader.readAsDataURL(blob)
      break
    }
  }).catch(() => {
    const handler = async (e: ClipboardEvent) => {
      document.removeEventListener('paste', handler)
      const file = Array.from(e.clipboardData?.files ?? []).find((f) => f.type.startsWith('image/'))
      if (!file) return
      const reader = new FileReader()
      reader.onload = async () => {
        const dataUrl = reader.result as string
        pastedImageUrl.value = dataUrl
        pastedImageBase64.value = dataUrl.split(',')[1]
        pastedMediaType.value = file.type
        await handleImageExtraction(pastedImageBase64.value!, file.type)
      }
      reader.readAsDataURL(file)
    }
    document.addEventListener('paste', handler, { once: true })
  })
}
</script>
