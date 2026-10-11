<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useNotificacionesStore } from '@/stores/notificaciones.store'

const props = defineProps<{ destinatarioId: string }>()
const store = useNotificacionesStore()
const open = ref(false)

onMounted(() => store.cargar(props.destinatarioId))

function toggle() { open.value = !open.value }

function etiqueta(tipo: string): string {
  const map: Record<string, string> = {
    TecnologiaRespondio: 'Tecnología respondió',
    NecesitamosInformacion: 'Necesitamos información',
    SolicitudResuelta: 'Solicitud resuelta',
    SolicitudRechazada: 'Solicitud rechazada',
    UsuarioRespondioAResponsable: 'Usuario respondió',
  }
  return map[tipo] ?? tipo
}

async function leer(id: string) { await store.marcarLeida(id) }
async function leerTodas() { await store.marcarTodasLeidas(props.destinatarioId) }
</script>

<template>
  <div class="notif-bell">
    <button class="notif-trigger" @click="toggle" :aria-label="'Notificaciones (' + store.noLeidas + ' sin leer)'">
      <span class="notif-icon">🔔</span>
      <span v-if="store.noLeidas > 0" class="notif-badge">{{ store.noLeidas }}</span>
    </button>
    <div v-if="open" class="notif-panel">
      <div class="notif-header">
        <span>Notificaciones</span>
        <button v-if="store.noLeidas > 0" class="notif-mark-all" @click="leerTodas">Marcar todas como leídas</button>
      </div>
      <div v-if="store.notificaciones.length === 0" class="notif-empty">Sin notificaciones.</div>
      <ul v-else class="notif-list">
        <li
          v-for="n in store.notificaciones"
          :key="n.id"
          class="notif-item"
          :class="{ 'notif-unread': !n.leida }"
        >
          <div class="notif-tipo">{{ etiqueta(n.tipoNotificacion) }}</div>
          <div class="notif-msg">{{ n.mensaje }}</div>
          <div class="notif-footer">
            <span class="notif-date">{{ new Date(n.fechaUtc).toLocaleString() }}</span>
            <button v-if="!n.leida" class="notif-leer" @click="leer(n.id)">Marcar leída</button>
          </div>
        </li>
      </ul>
    </div>
  </div>
</template>

<style scoped>
.notif-bell { position: relative; display: inline-block; }
.notif-trigger { background: none; border: none; cursor: pointer; padding: 4px 8px; font-size: 1.2rem; position: relative; }
.notif-badge { position: absolute; top: 0; right: 0; background: #e53e3e; color: #fff; border-radius: 9999px; font-size: .65rem; padding: 1px 5px; font-weight: 700; line-height: 1.4; }
.notif-panel { position: absolute; right: 0; top: calc(100% + 4px); width: 340px; background: var(--p-surface-0, #fff); border: 1px solid var(--p-surface-200, #e2e8f0); border-radius: 8px; box-shadow: 0 4px 16px rgba(0,0,0,.12); z-index: 200; }
.notif-header { display: flex; justify-content: space-between; align-items: center; padding: 10px 14px; border-bottom: 1px solid var(--p-surface-200, #e2e8f0); font-weight: 600; font-size: .9rem; }
.notif-mark-all { background: none; border: none; cursor: pointer; font-size: .8rem; color: var(--p-primary-500, #3b82f6); }
.notif-empty { padding: 20px 14px; color: var(--p-surface-500, #6b7280); font-size: .875rem; text-align: center; }
.notif-list { list-style: none; margin: 0; padding: 0; max-height: 360px; overflow-y: auto; }
.notif-item { padding: 10px 14px; border-bottom: 1px solid var(--p-surface-100, #f1f5f9); }
.notif-item:last-child { border-bottom: none; }
.notif-unread { background: var(--p-primary-50, #eff6ff); }
.notif-tipo { font-size: .75rem; font-weight: 600; color: var(--p-primary-600, #2563eb); margin-bottom: 2px; }
.notif-msg { font-size: .875rem; color: var(--p-surface-700, #374151); }
.notif-footer { display: flex; justify-content: space-between; align-items: center; margin-top: 4px; }
.notif-date { font-size: .7rem; color: var(--p-surface-400, #9ca3af); }
.notif-leer { background: none; border: none; cursor: pointer; font-size: .7rem; color: var(--p-primary-500, #3b82f6); }
</style>
