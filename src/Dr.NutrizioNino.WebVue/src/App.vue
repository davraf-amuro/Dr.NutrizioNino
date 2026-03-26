<script setup lang="ts">
import { computed } from 'vue'
import { RouterView, useRoute, useRouter } from 'vue-router'
import {
  NDialogProvider,
  NLayout,
  NLayoutContent,
  NLayoutHeader,
  NMenu,
  NMessageProvider,
  NText,
  type MenuOption
} from 'naive-ui'

const route = useRoute()
const router = useRouter()

const menuOptions: MenuOption[] = [
  { label: 'Alimenti', key: '/foods' },
  { label: 'Piatti', key: '/dishes' },
  { label: 'Nutrienti', key: '/nutrients' },
  { label: 'Marche', key: '/brands' },
  { label: 'Unità di misura', key: '/units' },
  { label: 'Supermercati', key: '/supermarkets' }
]

const activeKey = computed(() => {
  if (route.path.startsWith('/dishes')) return '/dishes'
  if (route.path.startsWith('/brands')) return '/brands'
  if (route.path.startsWith('/nutrients')) return '/nutrients'
  if (route.path.startsWith('/units')) return '/units'
  if (route.path.startsWith('/supermarkets')) return '/supermarkets'
  return '/foods'
})
</script>

<template>
  <n-message-provider>
    <n-dialog-provider>
      <n-layout class="app-shell">
        <n-layout-header bordered class="app-header">
          <n-text class="app-title">Dr. NutrizioNino</n-text>
          <n-menu
            mode="horizontal"
            :options="menuOptions"
            :value="activeKey"
            @update:value="(key) => router.push(key)"
            class="app-nav"
          />
        </n-layout-header>

        <n-layout-content class="app-content">
          <RouterView />
        </n-layout-content>
      </n-layout>
    </n-dialog-provider>
  </n-message-provider>
</template>

<style scoped>
.app-shell {
  min-height: 100vh;
}

.app-header {
  position: sticky;
  top: 0;
  z-index: 100;
  display: flex;
  align-items: center;
  padding: 0 32px;
  height: 60px;
  gap: 32px;
}

.app-title {
  font-size: 18px;
  font-weight: 700;
  white-space: nowrap;
  letter-spacing: -0.3px;
}

.app-nav {
  flex: 1;
}

.app-content {
  padding: 28px 32px;
}
</style>
