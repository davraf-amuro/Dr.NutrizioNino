<script setup lang="ts">
import list from '../components/Foods/FoodsList.vue'
import detail from '../components/Foods/FoodDetail.vue'
import { ref } from 'vue'
import type { CreatingFoodDto } from '@/Interfaces/CreatingFoodDto'
import type { FoodNutrient } from '@/Interfaces/FoodNutrient'

var isNew = ref(false)
let food: CreatingFoodDto = {
  id: '',
  name: 'alimento di prova',
  quantity: 100,
  unitOfMeasureId: 'DCA98E63-9327-4AD7-8E01-04DDB5DDCF0E'.toLowerCase(),
  barcode: '',
  brandId: '38153233-449b-4dcd-8188-69a47deba0fd'.toLocaleLowerCase(),
  calorie: 0,
  nutrients: []
}

const completeHandler = (creatingFood: CreatingFoodDto) => {
  food = creatingFood
  isNew.value = false
}
</script>

<template>
  <div class="foods">
    <h1>Alimenti</h1>
    <br />
    <button @click="isNew = !isNew" v-if="!isNew">Nuovo</button>
    <br v-if="!isNew" />
    <list v-if="!isNew"></list>
    <detail v-if="isNew" @cancel="isNew = false" @complete="completeHandler" :food="food"></detail>
    {{ food }}
  </div>
</template>
