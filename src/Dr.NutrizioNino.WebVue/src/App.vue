<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
import { NButton, NSpace } from 'naive-ui'
</script>

<template>
  <header class="app-header">
    <n-space justify="center" size="small">
      <n-button tag="a">
        <RouterLink to="/foods">Alimenti</RouterLink>
      </n-button>
      <n-button tag="a">
        <RouterLink to="/brands">Marche</RouterLink>
      </n-button>
    </n-space>
  </header>

  <main class="app-content">
    <RouterView />
  </main>
</template>

<style scoped>
.app-header {
  line-height: 1.5;
  margin-bottom: 1rem;
}

a {
  color: inherit;
  text-decoration: none;
}

.app-content {
  width: 100%;
}
</style>
