<script setup lang="ts">
import { RouterView } from 'vue-router'
import Toast from 'primevue/toast'
import AppSidebar from '@/components/AppSidebar.vue'
import { demoMode } from '@/services/tecnologia.api'
</script>
<template><div class="app-shell"><AppSidebar/><main class="app-body"><header class="app-toolbar"><span>Tecnología / Centro de trabajo</span><span class="mode">{{ demoMode ? 'Demostración · datos en este navegador' : 'Desarrollo · API local' }}</span></header><RouterView/></main><Toast/></div></template>
