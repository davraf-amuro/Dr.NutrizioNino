<template>
  <div>
    <h3>Aggiungi nuovo marchio</h3>
    Guid: {{ myBrand.id }}
    <br />
    Nome:
    <input type="text" :value="myBrand.name" />

    <br />
    <button @click="onUpdate">aggiorna</button>
  </div>
</template>

<script setup lang="ts">
import { reactive } from 'vue'

const myBrand = reactive({
  id: '000-000-000',
  name: 'Nestlé'
})

function onUpdate() {
  myBrand.name = 'ancheno'
}
</script>

<style scoped></style>
