<script setup lang="ts">
import list from '../components/Foods/FoodsList.vue'
import detail from '../components/Foods/FoodDetail.vue'
import { ref } from 'vue'

var isNew = ref(false)
</script>

<template>
  <div class="foods">
    <h1>Alimenti</h1>
    <br />
    <button @click="isNew = !isNew">Nuovo</button>

    <br />
    <list v-if="!isNew"></list>
    <br />
    <detail v-if="isNew"></detail>
  </div>
</template>
