import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api, demoMode } from '@/services/tecnologia.api'
import type { Notificacion } from '@/types/tecnologia'

export const useNotificacionesStore = defineStore('notificaciones', () => {
 const notificaciones = ref<Notificacion[]>([])
 const loading = ref(false)
 const noLeidas = computed(() => notificaciones.value.filter(n => !n.leida).length)

 async function cargar(destinatarioId: string) {
  if (demoMode) return
  loading.value = true
  try {
   const result = await api.notificaciones(destinatarioId)
   notificaciones.value = result.notificaciones
  } finally {
   loading.value = false
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
