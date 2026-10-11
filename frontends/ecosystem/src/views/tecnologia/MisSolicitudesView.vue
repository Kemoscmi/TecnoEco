<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import Button from 'primevue/button'
import Textarea from 'primevue/textarea'
import Tag from 'primevue/tag'
import Message from 'primevue/message'
import { useToast } from 'primevue/usetoast'
import { useTecnologiaStore } from '@/stores/tecnologia.store'
import type { Ticket } from '@/types/tecnologia'

function formatSize(b: number) { return b < 1024 ? b + ' B' : b < 1048576 ? (b/1024).toFixed(1) + ' KB' : (b/1048576).toFixed(1) + ' MB' }

// REQ-003: el usuario autenticado determina qué solicitudes se muestran.
// En demo se usa el mismo ID simulado que CrearSolicitudView.
// En producción este valor vendrá del perfil en sesión.
const MI_SOLICITANTE_ID = 'usuario-demo'

const store = useTecnologiaStore()
const toast = useToast()

const selectedId = ref<string | null>(null)
const nuevoTexto = ref('')
const busy = ref(false)

// Solo las solicitudes del usuario en sesión
const misSolicitudes = computed(() =>
  store.data.tickets.filter(t => t.solicitanteId === MI_SOLICITANTE_ID)
)

const selected = computed<Ticket | undefined>(() =>
  store.data.tickets.find(t => t.id === selectedId.value)
)

// REQ-003: el usuario general NO ve notas internas ni detalles técnicos.
// Solo se entregan actividades marcadas como "Respuesta al solicitante".
const historialVisible = computed(() =>
  store.data.activities.filter(
    a => a.ticketId === selectedId.value && a.visibility === 'Respuesta al solicitante'
  )
)

// REQ-010: el solicitante solo ve adjuntos públicos (visibility='publica')
// Incluye adjuntos de la solicitud original y de respuestas al solicitante.
const adjuntosVisibles = computed(() =>
  store.data.adjuntos.filter(
    a => a.ticketId === selectedId.value && a.visibility === 'publica'
  )
)

const severity = (s: string) =>
  s === 'Resuelta' ? 'success'
    : s === 'Rechazada' ? 'danger'
    : s === 'Necesitamos información' ? 'warn'
    : s === 'En proceso' ? 'info'
    : 'secondary'

const date = (s: string) =>
  new Date(s).toLocaleString('es-MX', { dateStyle: 'medium', timeStyle: 'short' })

function abrir(id: string) {
  selectedId.value = id
  nuevoTexto.value = ''
}

function cerrar() {
  selectedId.value = null
  nuevoTexto.value = ''
}

// REQ-003: el usuario puede agregar información posterior a su solicitud.
// Se registra siempre como "Respuesta al solicitante" para que Tecnología la vea.
async function enviarInfo() {
  if (!nuevoTexto.value.trim() || !selected.value) return
  busy.value = true
  try {
    await store.addActivity({
      ticketId: selectedId.value,
      text: nuevoTexto.value.trim(),
      visibility: 'Respuesta al solicitante',
    })
    nuevoTexto.value = ''
    toast.add({ severity: 'success', summary: 'Información enviada', detail: 'El equipo de Tecnología podrá verla.', life: 3000 })
  } catch (e) {
    toast.add({ severity: 'error', summary: 'No se pudo enviar', detail: e instanceof Error ? e.message : 'Intenta de nuevo.', life: 6000 })
  } finally {
    busy.value = false
  }
}

onMounted(() => store.load())
</script>

<template>
  <div class="page">
    <div class="page-heading">
      <div>
        <h1>Mis solicitudes</h1>
        <p>Seguimiento de las solicitudes que has enviado al equipo de Tecnología.</p>
      </div>
    </div>

    <Message v-if="store.error" severity="error" :closable="false">
      {{ store.error }} <Button label="Reintentar" text @click="store.load()" />
    </Message>

    <p v-if="store.loading" role="status">Cargando…</p>

    <div v-else-if="!selectedId" class="ms-layout">
      <!-- Listado de solicitudes propias -->
      <div v-if="misSolicitudes.length === 0" class="empty-state panel">
        <i class="pi pi-inbox" />
        <p>Todavía no has enviado ninguna solicitud.</p>
        <Button label="Crear solicitud" icon="pi pi-plus" @click="$router.push('/tecnologia/solicitudes/nueva')" />
      </div>

      <div v-else class="solicitudes-lista">
        <button
          v-for="ticket in misSolicitudes"
          :key="ticket.id"
          class="solicitud-card panel"
          @click="abrir(ticket.id)"
        >
          <div class="solicitud-top">
            <Tag :value="ticket.estado" :severity="severity(ticket.estado)" />
            <small>{{ ticket.categoria }} · {{ ticket.id.slice(0, 14) }}</small>
          </div>
          <strong class="solicitud-titulo">{{ ticket.title }}</strong>
          <small class="solicitud-fecha">Creada el {{ date(ticket.createdAt) }}</small>
        </button>
      </div>
    </div>

    <!-- Detalle de solicitud -->
    <div v-else-if="selected" class="ms-detalle">
      <button class="volver" @click="cerrar">
        <i class="pi pi-arrow-left" /> Volver a mis solicitudes
      </button>

      <div class="detalle-header panel">
        <div class="detalle-meta">
          <Tag :value="selected.estado" :severity="severity(selected.estado)" />
          <small>{{ selected.categoria }} · {{ selected.id.slice(0, 14) }}</small>
        </div>
        <h2 class="detalle-titulo">{{ selected.title }}</h2>
        <p class="detalle-desc">{{ selected.description }}</p>
        <small>Enviada el {{ date(selected.createdAt) }}</small>
      </div>

      <!-- REQ-010: adjuntos públicos del ticket -->
      <div v-if="adjuntosVisibles.length" class="historial panel">
        <div class="panel-heading">
          <h2>Archivos adjuntos</h2>
        </div>
        <div class="adj-lista">
          <div v-for="adj in adjuntosVisibles" :key="adj.id" class="adj-chip">
            <i class="pi pi-file adj-icon"/>
            <span class="adj-nombre">{{ adj.nombre }}</span>
            <small class="adj-size">{{ formatSize(adj.tamaño) }}</small>
            <span class="adj-pending" title="Descarga pendiente de configuración de storage">
              <i class="pi pi-clock"/> Pendiente
            </span>
          </div>
        </div>
      </div>

      <!-- Historial simplificado: solo respuestas visibles al solicitante -->
      <div class="historial panel">
        <div class="panel-heading">
          <h2>Historial de la solicitud</h2>
        </div>
        <div class="timeline" v-if="historialVisible.length">
          <article v-for="evento in historialVisible" :key="evento.id">
            <p>{{ evento.text }}</p>
            <small>{{ date(evento.createdAt) }}</small>
          </article>
        </div>
        <p v-else class="empty">
          El equipo de Tecnología aún no ha registrado novedades visibles para esta solicitud.
        </p>
      </div>

      <!-- Agregar información posterior -->
      <div class="agregar-info panel">
        <div class="panel-heading">
          <h2>Agregar información</h2>
        </div>
        <form class="agregar-form" @submit.prevent="enviarInfo">
          <label for="nueva-info">
            ¿Tienes más contexto o datos que puedan ayudar a resolver tu solicitud?
          </label>
          <Textarea
            id="nueva-info"
            v-model="nuevoTexto"
            rows="4"
            maxlength="10000"
            placeholder="Escribe aquí cualquier información adicional…"
          />
          <div class="actions">
            <Button
              type="submit"
              label="Enviar información"
              icon="pi pi-send"
              :loading="busy"
              :disabled="!nuevoTexto.trim()"
            />
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<style scoped>
.ms-layout { max-width: 760px; }

.solicitudes-lista {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.solicitud-card {
  display: block;
  width: 100%;
  text-align: left;
  border: 1px solid #e2e8f0;
  background: white;
  border-radius: 10px;
  padding: 20px 22px;
  cursor: pointer;
  transition: box-shadow .15s, border-color .15s;
}
.solicitud-card:hover {
  border-color: #93c5fd;
  box-shadow: 0 2px 8px rgba(59,130,246,.12);
}

.solicitud-top {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 10px;
}

.solicitud-titulo {
  display: block;
  font-size: 15px;
  margin-bottom: 8px;
  line-height: 1.4;
}

.solicitud-fecha { display: block; }

.empty-state {
  padding: 48px 32px;
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
}
.empty-state i { font-size: 36px; color: #94a3b8; }
.empty-state p { margin: 0; color: #64748b; }

/* Detalle */
.ms-detalle { display: flex; flex-direction: column; gap: 18px; max-width: 760px; }

.volver {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  border: 0;
  background: none;
  color: #2563eb;
  font-size: 13px;
  cursor: pointer;
  padding: 0;
  margin-bottom: 4px;
}
.volver:hover { text-decoration: underline; }

.detalle-header { padding: 26px 28px; }
.detalle-meta { display: flex; align-items: center; gap: 10px; margin-bottom: 14px; }
.detalle-titulo { font-size: 20px; margin: 0 0 14px; }
.detalle-desc { white-space: pre-wrap; margin: 0 0 16px; line-height: 1.7; }

.historial .empty { padding: 0 22px 22px; }

.adj-lista { padding: 14px 22px 18px; display: flex; flex-direction: column; gap: 6px; }
.adj-chip { display: flex; align-items: center; gap: 8px; padding: 8px 10px; background: #f8fafc; border: 1px solid #e2e8f0; border-radius: 6px; font-size: 12px; }
.adj-icon { color: #3b82f6; font-size: 14px; }
.adj-nombre { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.adj-size { color: #94a3b8; white-space: nowrap; }
.adj-pending { display: inline-flex; align-items: center; gap: 4px; font-size: 10px; color: #f59e0b; background: #fffbeb; border: 1px solid #fde68a; border-radius: 8px; padding: 1px 6px; white-space: nowrap; }

.agregar-form { padding: 18px 22px 22px; }
.agregar-form label { font-size: 13px; font-weight: 600; margin-bottom: 10px; display: block; line-height: 1.6; }
.agregar-form .p-textarea { width: 100%; }
</style>
