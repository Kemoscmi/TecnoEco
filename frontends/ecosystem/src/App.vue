<script setup lang="ts">
import { RouterView } from 'vue-router'
import Toast from 'primevue/toast'
import AppSidebar from '@/components/AppSidebar.vue'
import NotificacionesBell from '@/components/NotificacionesBell.vue'
import { demoMode } from '@/services/tecnologia.api'
// ID del usuario actual: en desarrollo se usa un valor fijo hasta integrar autenticación.
const currentUserId = demoMode ? 'demo-cliente-01' : (import.meta.env.VITE_CURRENT_USER_ID ?? 'dev-user-01')
</script>
<template><div class="app-shell"><AppSidebar/><main class="app-body"><header class="app-toolbar"><span>Tecnología / Centro de trabajo</span><div style="display:flex;align-items:center;gap:12px"><NotificacionesBell :destinatario-id="currentUserId"/><span class="mode">{{ demoMode ? 'Demostración · datos en este navegador' : 'Desarrollo · API local' }}</span></div></header><RouterView/></main><Toast/></div></template>
