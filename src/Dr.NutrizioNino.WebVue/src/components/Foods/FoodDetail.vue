<template>
  <n-space vertical size="medium">
    <n-h3 style="margin: 0">{{ isEditMode ? 'Modifica alimento' : 'Nuovo alimento' }}</n-h3>

    <n-spin :show="isSubmitting">
      <n-form ref="formRef" :model="localFood" :rules="rules" label-placement="left" label-width="140" label-align="right">
        <n-form-item label="Nome" path="name">
          <n-space vertical size="small" style="width: 100%">
            <n-input v-model:value="localFood.name" :maxlength="50" :disabled="isSubmitting" placeholder="Inserisci il nome dell'alimento" />

            <n-list v-if="nameSuggestions.length > 0" bordered size="small" style="max-width: 400px">
              <n-list-item v-for="s in nameSuggestions" :key="s.id" style="padding: 4px 8px">
                <n-space justify="space-between" align="center" style="width: 100%">
                  <n-text style="font-size: 12px">
                    {{ s.name }}<span v-if="s.brandDescription" style="color:#888"> — {{ s.brandDescription }}</span>
                  </n-text>
                  <n-button size="tiny" secondary @click="useExistingFood(s.id)">Usa questo esistente</n-button>
                </n-space>
              </n-list-item>
            </n-list>
          </n-space>
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
          📋 Incolla
        </n-button>

        <n-button size="small" :loading="isExtracting" @click="triggerFileUpload">
          📁 Carica file
        </n-button>
        <input
          ref="fileInputRef"
          type="file"
          accept="image/*"
          style="display: none"
          @change="onFileSelected"
        />

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

      <!-- Anteprima immagine + JSON raw Ollama (affiancati) -->
      <n-space v-if="pastedImageUrl" align="start" size="medium" style="margin-bottom: 12px">
        <div style="max-width: 200px">
          <img :src="pastedImageUrl" alt="Etichetta" style="max-width: 100%; border-radius: 6px; border: 1px solid #ddd" />
        </div>

        <n-collapse v-if="lastRawJson" style="max-width: 360px; font-size: 12px">
          <n-collapse-item title="Dati raw Ollama" name="raw-json">
            <pre style="white-space: pre-wrap; word-break: break-word; margin: 0; font-size: 11px">{{ lastRawJson }}</pre>
          </n-collapse-item>
        </n-collapse>
      </n-space>

      <!-- Conversioni effettuate dalla AI -->
      <n-alert v-if="extractionConversions.length > 0" type="info" :show-icon="false" style="margin-bottom: 8px; font-size: 12px">
        Conversioni: {{ extractionConversions.join(' | ') }}
      </n-alert>

      <!-- Riconciliazione: Caso A (IncompleteMatch) e Caso B (Unrecognized) -->
      <ExtractionReconciliation
        v-if="lastExtractionResults.length > 0"
        :extracted-nutrients="lastExtractionResults"
        :available-nutrients="availableNutrients"
        :units-of-measures="unitsOfMeasures"
        @alias-confirmed="onAliasConfirmed"
        @nutrient-created="onNutrientCreated"
        @unit-created="onReconUnitCreated"
      />

      <n-space vertical size="small">
        <FoodNutrientInput
          v-for="fnu in sortedNutrients"
          :key="fnu.nutrientId"
          :food-nutrient-dto="fnu"
          :units-of-measures="unitsOfMeasures"
          :status="matchStatusByNutrientId[fnu.nutrientId]"
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
  NCollapse,
  NCollapseItem,
  NDivider,
  NForm,
  NFormItem,
  NGi,
  NGrid,
  NH3,
  NInput,
  NInputGroup,
  NInputNumber,
  NList,
  NListItem,
  NSelect,
  NSpace,
  NSpin,
  NText,
  useMessage,
  type FormInst,
  type FormRules,
  type SelectOption
} from 'naive-ui'
import type { FoodDto } from '@/Interfaces/foods/FoodDto'
import type { ExtractedNutrientDto, ExtractionStatus } from '@/Interfaces/foods/ExtractedNutrientDto'
import type { Nutrient } from '@/Interfaces/Nutrients/Nutrient'
import { extractNutrientsFromImage, getSimilarFoodNames } from '@/modules/foods/api/foods.api'
import type { FoodSuggestionDto } from '@/Interfaces/foods/FoodSuggestionDto'
import { ApiError } from '@/core/http/ApiError'
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
  availableNutrients?: Nutrient[]
  isSubmitting?: boolean
}>()

const emit = defineEmits<{
  cancel: []
  complete: [food: FoodDto]
  'brand-created': [brand: Brand]
  'unit-created': [unit: UnitOfMeasureDto]
  'supermarket-created': [supermarket: Supermarket]
  'category-created': [category: Category]
  'nutrient-created': [nutrient: Nutrient]
  'use-existing': [foodId: string]
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

// Suggerimenti alimenti con nome simile durante la digitazione (solo in creazione, mai non bloccante)
const nameSuggestions = ref<FoodSuggestionDto[]>([])
let nameSuggestionsDebounce: ReturnType<typeof setTimeout> | null = null
let nameSuggestionsAbort: AbortController | null = null

watch(() => localFood.value.name, (name) => {
  if (isEditMode.value) return

  if (nameSuggestionsDebounce) clearTimeout(nameSuggestionsDebounce)
  nameSuggestionsAbort?.abort()
  nameSuggestions.value = []

  nameSuggestionsDebounce = setTimeout(async () => {
    nameSuggestionsAbort = new AbortController()
    try {
      nameSuggestions.value = await getSimilarFoodNames(name, nameSuggestionsAbort.signal)
    } catch {
      // richiesta abortita o errore di rete: nessun suggerimento, non bloccante
    }
  }, 300)
})

const useExistingFood = (foodId: string) => {
  nameSuggestions.value = []
  emit('use-existing', foodId)
}

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
const lastRawJson = ref<string>('')
let abortController: AbortController | null = null

// Mappa esito estrazione per nutriente, propagata come badge alle righe FoodNutrientInput
const matchStatusByNutrientId = computed<Record<string, ExtractionStatus>>(() => {
  const map: Record<string, ExtractionStatus> = {}
  for (const result of lastExtractionResults.value) {
    if (result.matchedNutrientId) map[result.matchedNutrientId] = result.status
  }
  return map
})

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
  lastRawJson.value = ''
  abortController = new AbortController()
  timer.start()

  try {
    const result = await extractNutrientsFromImage(base64, selectedProviderKey.value, mediaType, abortController.signal)
    lastExtractionResults.value = result.nutrients
    lastRawJson.value = result.rawJson

    for (const extracted of result.nutrients) {
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
    // 422 = immagine non riconosciuta come etichetta: avviso non bloccante con il detail dal backend
    if (err instanceof ApiError && err.status === 422) {
      message.warning(err.message)
      return
    }
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
  lastExtractionResults.value = lastExtractionResults.value.filter((n) => n.name !== aiName)
}

const onNutrientCreated = (nutrient: Nutrient, unitOfMeasureId: string) => {
  emit('nutrient-created', nutrient)
  localFood.value.nutrients.push({
    nutrientId: nutrient.id,
    name: nutrient.name,
    positionOrder: nutrient.positionOrder,
    unitOfMeasureId: unitOfMeasureId || nutrient.defaultUnitOfMeasureId,
    quantity: 0
  })
}

// UoM creata al volo dalla riconciliazione: assegnala alla riga e segna il match come completo
const onReconUnitCreated = (unit: UnitOfMeasureDto, nutrientId: string) => {
  emit('unit-created', unit)
  const target = localFood.value.nutrients.find((n) => n.nutrientId === nutrientId)
  if (target) target.unitOfMeasureId = unit.id
  lastExtractionResults.value = lastExtractionResults.value.map((result) =>
    result.matchedNutrientId === nutrientId
      ? { ...result, status: 'Matched', canonicalUnit: unit.abbreviation }
      : result
  )
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

const fileInputRef = ref<HTMLInputElement | null>(null)

const triggerFileUpload = () => {
  fileInputRef.value?.click()
}

const onFileSelected = (e: Event) => {
  const file = (e.target as HTMLInputElement).files?.[0]
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

  // Reset per permettere di ricaricare lo stesso file consecutivamente
  ;(e.target as HTMLInputElement).value = ''
}
</script>
