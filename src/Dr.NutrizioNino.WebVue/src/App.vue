<script setup lang="ts">
import { computed } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import { NButton, NDialogProvider, NMessageProvider, NSpace } from 'naive-ui'
import { useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()

const isActive = (path: string) => computed(() => route.path.startsWith(path))
</script>

<template>
  <n-message-provider>
    <n-dialog-provider>
      <header class="app-header">
        <n-space justify="center" size="small">
          <n-button
            :type="isActive('/foods').value ? 'primary' : 'default'"
            @click="router.push('/foods')"
          >
            Alimenti
          </n-button>
          <n-button
            :type="isActive('/brands').value ? 'primary' : 'default'"
            @click="router.push('/brands')"
          >
            Marche
          </n-button>
        </n-space>
      </header>

      <main class="app-content">
        <RouterView />
      </main>
    </n-dialog-provider>
  </n-message-provider>
</template>

<style scoped>
.app-header {
  line-height: 1.5;
  margin-bottom: 1rem;
}

.app-content {
  width: 100%;
}
</style>
