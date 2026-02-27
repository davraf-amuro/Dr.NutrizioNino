<template>
  <div>
    <h3>Elenco dei marchi</h3>
    <table>
      <tr>
        <td>Guid</td>
        <td>Nome</td>
        <td>&nbsp;</td>
        <td>&nbsp;</td>
      </tr>

      <tr v-for="brand in brands" :key="brand.id">
        <td>{{ brand.id }}</td>
        <td>{{ brand.name }}</td>
        <td><button @click="updateBrand(brand)">Modifica</button></td>
        <td><button @click="deleteBrand(brand)">Elimina</button></td>
      </tr>
    </table>
  </div>
</template>

<script setup lang="ts">
import type { Brand } from '@/Interfaces/Brand'

defineProps<{
  brands: Brand[]
}>()

const emit = defineEmits<{
  update: [brand: Brand]
  delete: [brand: Brand]
}>()

function updateBrand(brand: Brand) {
  emit('update', brand)
}

function deleteBrand(brand: Brand) {
  emit('delete', brand)
}
</script>
