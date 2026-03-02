<script setup lang="ts">
import { RouterLink, RouterView } from 'vue-router'
</script>

<template>
  <header class="app-header">
    <nav>
      <RouterLink to="/foods">Alimenti</RouterLink>
      <RouterLink to="/brands">Marche</RouterLink>
    </nav>
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

nav {
  display: flex;
  gap: 1rem;
  justify-content: center;
  font-size: 0.95rem;
}

nav a.router-link-exact-active {
  color: var(--color-text);
}

nav a.router-link-exact-active:hover {
  background-color: transparent;
}

nav a {
  display: inline-block;
  padding: 0.25rem 0.75rem;
}

.app-content {
  width: 100%;
}
</style>
