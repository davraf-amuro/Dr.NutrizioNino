<script setup lang="ts">
import list from '../components/Brands/BrandsList.vue'
import detail from '../components/Brands/BrandDetail.vue'
import { ref } from 'vue'

var isNew = ref(false)
</script>

<template>
  <div class="brands">
    <h1>Marche</h1>
    <br />
    <button @click="isNew = !isNew">Nuovo</button>

    <br />
    <list v-if="!isNew"></list>
    <br />
    <detail v-if="isNew"></detail>
  </div>
</template>
