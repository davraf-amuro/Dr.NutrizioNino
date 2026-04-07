<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, RouterView, useRoute, useRouter } from 'vue-router'
import {
  NConfigProvider,
  NDialogProvider,
  NLayout,
  NLayoutContent,
  NLayoutHeader,
  NMenu,
  NMessageProvider,
  NText,
  type MenuOption
} from 'naive-ui'
import { useAuth } from '@/modules/auth/composables/useAuth'
import { useTheme } from '@/modules/auth/composables/useTheme'

const route = useRoute()
const router = useRouter()
const { isAdmin, user } = useAuth()
const { resolvedTheme } = useTheme()

const menuOptions = computed<MenuOption[]>(() => {
  const items: MenuOption[] = [
    { label: 'Alimenti', key: '/foods' },
    { label: 'Piatti', key: '/dishes' },
    { label: 'Simulazioni', key: '/daily-simulations' },
    {
      label: 'Configurazione',
      key: 'configurazione',
      children: [
        { label: 'Nutrienti', key: '/nutrients' },
        { label: 'Sezioni simulazione', key: '/section-configs' },
        { label: 'Marche', key: '/brands' },
        { label: 'Unità di misura', key: '/units' },
        { label: 'Supermercati', key: '/supermarkets' },
        { label: 'Categorie', key: '/categories' }
      ]
    }
  ]
  if (isAdmin.value) {
    items.push({ label: 'Utenti', key: '/admin/users' })
  }
  return items
})

const activeKey = computed(() => {
  if (route.path.startsWith('/dishes')) return '/dishes'
  if (route.path.startsWith('/brands')) return '/brands'
  if (route.path.startsWith('/nutrients')) return '/nutrients'
  if (route.path.startsWith('/units')) return '/units'
  if (route.path.startsWith('/supermarkets')) return '/supermarkets'
  if (route.path.startsWith('/categories')) return '/categories'
  if (route.path.startsWith('/daily-simulations')) return '/daily-simulations'
  if (route.path.startsWith('/admin')) return '/admin/users'
  return '/foods'
})
</script>

<template>
  <n-config-provider :theme="resolvedTheme">
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
            <RouterLink v-if="user" to="/profile" class="app-username-link">
              <n-text depth="3" class="app-username">{{ user.userName }}</n-text>
            </RouterLink>
          </n-layout-header>

          <n-layout-content class="app-content">
            <RouterView />
          </n-layout-content>
        </n-layout>
      </n-dialog-provider>
    </n-message-provider>
  </n-config-provider>
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

.app-username-link {
  text-decoration: none;
  white-space: nowrap;
}

.app-username {
  font-size: 13px;
  cursor: pointer;
}

.app-username:hover {
  opacity: 0.75;
}
</style>
