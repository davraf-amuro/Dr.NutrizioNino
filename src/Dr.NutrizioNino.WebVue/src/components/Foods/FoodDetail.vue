<template>
  <div>
    <h3>Alimento</h3>
    <span v-if="waiting">caricamento...</span>
    <table>
      <tr>
        <td>
          <b>Marca:</b>
        </td>
        <td style="width: 80%">
          <n-select :options="brands" value-field="id" label-field="name"></n-select>
        </td>
      </tr>
      <tr>
        <td>
          <b>Unità di misura</b>
        </td>
        <td>
          <n-select :options="uom" value-field="id" label-field="name"></n-select>
        </td>
      </tr>
      <tr>
        <td></td>
        <td></td>
      </tr>
      <tr>
        <td></td>
        <td></td>
      </tr>
    </table>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'
import { NSelect } from 'naive-ui'
import type { BrandsApiResponse } from '@/Interfaces/BrandsApiResponse'
import type { ApiResponseDto } from '@/Interfaces/ApiResponseDto'
import type { UnitOfMeasureDto } from '@/Interfaces/UnitOfMeasureDto'

const waiting = ref(true)
const brands = ref()
const uom = ref()

axios.get('https://localhost:44360/brands').then(function (response) {
  const casted: BrandsApiResponse = response.data
  brands.value = casted.data
})

axios.get('https://localhost:44360/unitsOfMeasures').then(function (response) {
  const casted: ApiResponseDto<UnitOfMeasureDto> = response.data
  uom.value = casted.data
})

axios.get('https://localhost:44360/nutrients').then(function (response) {
  const casted: ApiResponseDto<UnitOfMeasureDto> = response.data
  uom.value = casted.data
})

waiting.value = false
</script>

<style scoped></style>
