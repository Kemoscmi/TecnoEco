import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { HubConnection } from '@microsoft/signalr'
import { crearConexionNotificaciones } from '@/services/signalr'
import { api, demoMode } from '@/services/tecnologia.api'
import type { Notificacion } from '@/types/tecnologia'

export const useNotificacionesStore = defineStore('notificaciones', () => {
 const notificaciones = ref<Notificacion[]>([])
 const loading = ref(false)
 const noLeidas = computed(() => notificaciones.value.filter(n => !n.leida).length)
 let hub: HubConnection | null = null

 async function cargar(destinatarioId: string) {
  if (demoMode) return
  loading.value = true
  try {
   const result = await api.notificaciones(destinatarioId)
   notificaciones.value = result.notificaciones
  } finally {
   loading.value = false
  }
  await conectar(destinatarioId)
 }

 async function conectar(destinatarioId: string) {
  if (demoMode || hub) return
  hub = crearConexionNotificaciones(destinatarioId)
  hub.on('notificacion', (n: Notificacion) => {
   notificaciones.value.unshift(n)
  })
  try {
   await hub.start()
  } catch {
   // conexión fallida: la reconexión automática lo reintentará
  }
 }

 async function marcarLeida(id: string) {
  if (demoMode) {
   const n = notificaciones.value.find(x => x.id === id)
   if (n) n.leida = true
   return
  }
  await api.marcarLeida(id)
  const n = notificaciones.value.find(x => x.id === id)
  if (n) n.leida = true
 }

 async function marcarTodasLeidas(destinatarioId: string) {
  const pendientes = notificaciones.value.filter(n => !n.leida)
  await Promise.all(pendientes.map(n => marcarLeida(n.id)))
  void destinatarioId
 }

 return { notificaciones, loading, noLeidas, cargar, marcarLeida, marcarTodasLeidas }
})
