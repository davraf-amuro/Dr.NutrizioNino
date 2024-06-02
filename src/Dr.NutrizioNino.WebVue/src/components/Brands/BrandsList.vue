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

      <tr v-for="(brand, index) in brands" :key="index">
        <td>{{ brand }}</td>
        <td>{{ brand }}</td>
        <td><button @click="Update(brand)">Modifica</button></td>
        <td><button @click="Delete(brand)">Elimina</button></td>
      </tr>
    </table>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'

const brands = ref([])
axios.get('https://localhost:44360/brands').then(function (response) {
  brands.value = response.data
})

function Update(brand: any) {
  console.log(brand)
}

function Delete(brand: any) {
  console.log(brand)
  //brands.value = brands.value.filter((x) => x.id != brand.id)
}
</script>
