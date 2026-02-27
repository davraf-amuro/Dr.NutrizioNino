<template>
  <div>
    <h3>Aggiungi nuovo marchio</h3>
    Nome:
    <input type="text" v-model="name" />

    <br />
    <button @click="save" :disabled="!name.trim() || isSubmitting">Salva</button>
    <button @click="cancel" :disabled="isSubmitting">Annulla</button>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'

defineProps<{
  isSubmitting?: boolean
}>()

const emit = defineEmits<{
  save: [name: string]
  cancel: []
}>()

const name = ref('')

function save() {
  emit('save', name.value.trim())
  name.value = ''
}

function cancel() {
  emit('cancel')
  name.value = ''
}
</script>

<style scoped></style>
