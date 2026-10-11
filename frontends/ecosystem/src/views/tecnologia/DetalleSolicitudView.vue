<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import Select from 'primevue/select'
import Textarea from 'primevue/textarea'
import InputText from 'primevue/inputtext'
import Tag from 'primevue/tag'
import Message from 'primevue/message'
import { useToast } from 'primevue/usetoast'
import { useTecnologiaStore } from '@/stores/tecnologia.store'
import { statuses, type Activity, type Status, type Adjunto } from '@/types/tecnologia'

// Demo: en producción vendrá del perfil autenticado (REQ-005)
const ACTOR_ID = 'tecnologia-demo'

const route  = useRoute()
const router = useRouter()
const store  = useTecnologiaStore()
const toast  = useToast()
const busy   = ref(false)

const ticket = computed(() => store.data.tickets.find(t => t.id === route.params.id))
const notFound = computed(() => store.ready && !ticket.value)

// Actividades de este ticket en orden cronológico
const actividades = computed(() =>
  store.data.activities
    .filter(a => a.ticketId === ticket.value?.id)
    .slice()
    .sort((a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime())
)

// Conversación = solo "Respuesta al solicitante"
const conversacion = computed(() => actividades.value.filter(a => a.visibility === 'Respuesta al solicitante'))

// Línea de tiempo = todo, en orden cronológico
const lineaDeTiempo = computed(() => actividades.value)

// ── Adjuntos (REQ-010) ─────────────────────────────────────────────────
const adjuntosTicket = computed<Adjunto[]>(() =>
  store.data.adjuntos.filter(a => a.ticketId === ticket.value?.id && a.activityId === null)
)
const adjuntosDeActividad = (actId: string) =>
  store.data.adjuntos.filter(a => a.activityId === actId)

const fileInput  = ref<HTMLInputElement | null>(null)
const archivos   = ref<File[]>([])
function onFileChange(e: Event) {
  const el = e.target as HTMLInputElement
  if (!el.files) return
  const nuevos = Array.from(el.files).filter(f => !archivos.value.some(a => a.name === f.name && a.size === f.size))
  archivos.value = [...archivos.value, ...nuevos]
  el.value = ''
}
function quitarArchivo(i: number) { archivos.value = archivos.value.filter((_, j) => j !== i) }
function formatSize(b: number) { return b < 1024 ? b + ' B' : b < 1048576 ? (b/1024).toFixed(1) + ' KB' : (b/1048576).toFixed(1) + ' MB' }

// REQ-012: interceptar Resuelta / Rechazada para exigir mensaje
const pendingEstado = ref<'Resuelta' | 'Rechazada' | null>(null)
const pendingMensaje = ref('')

function onEstadoChange(value: Status) {
  if (value === 'Resuelta' || value === 'Rechazada') {
    pendingEstado.value = value
    pendingMensaje.value = ''
  } else {
    changeStatus(value)
  }
}

function cancelarCierre() {
  pendingEstado.value = null
  pendingMensaje.value = ''
}

async function confirmarCierre() {
  if (!pendingMensaje.value.trim() || !ticket.value) return
  await run(async () => {
    if (pendingEstado.value === 'Resuelta') {
      await store.resolver(ticket.value!.id, { mensaje: pendingMensaje.value.trim() })
    } else {
      await store.rechazar(ticket.value!.id, { motivo: pendingMensaje.value.trim() })
    }
    pendingEstado.value = null
    pendingMensaje.value = ''
  })
}

// ── Formularios ────────────────────────────────────────────────────────
const note       = ref('')
const visibility = ref<Activity['visibility']>('Nota interna')
const reasignarId       = ref('')
const nuevoColaborador  = ref('')

const severity = (s: string) =>
  s === 'Resuelta' ? 'success' : s === 'Rechazada' ? 'danger'
    : s === 'Necesitamos información' ? 'warn' : s === 'En proceso' ? 'info' : 'secondary'

const date = (s: string) =>
  new Date(s).toLocaleString('es-MX', { dateStyle: 'medium', timeStyle: 'short' })

const tipoEvento = (a: Activity) => {
  if (a.text.includes('tomó la solicitud'))  return { icon: 'pi-user-plus', label: 'Asignación' }
  if (a.text.includes('Reasignada'))         return { icon: 'pi-arrow-right-arrow-left', label: 'Reasignación' }
  if (a.text.includes('Estado actualizado')) return { icon: 'pi-refresh', label: 'Cambio de estado' }
  if (a.text.includes('Colaborador'))        return { icon: 'pi-users', label: 'Colaborador' }
  if (a.visibility === 'Respuesta al solicitante') return { icon: 'pi-comment', label: 'Respuesta al solicitante' }
  return { icon: 'pi-info-circle', label: 'Nota interna' }
}

async function run(action: () => Promise<void>) {
  busy.value = true
  try { await action(); toast.add({ severity: 'success', summary: 'Guardado', life: 2000 }) }
  catch (e) { toast.add({ severity: 'error', summary: 'Error', detail: e instanceof Error ? e.message : 'Intenta de nuevo', life: 6000 }) }
  finally { busy.value = false }
}

function changeStatus(value: Status) {
  if (ticket.value && value !== ticket.value.estado)
    void run(() => store.changeStatus(ticket.value!.id, value))
}

async function guardarActividad() {
  if (!note.value.trim()) return
  await run(async () => {
    await store.addActivity({
      ticketId: ticket.value!.id, text: note.value.trim(), visibility: visibility.value,
      adjuntos: archivos.value.map(f => ({ nombre: f.name, tipo: f.type || 'application/octet-stream', tamaño: f.size })),
    })
    note.value = ''
    archivos.value = []
  })
}

async function tomarSolicitud() {
  await run(() => store.tomarSolicitud(ticket.value!.id, { actorId: ACTOR_ID }))
}

async function reasignar() {
  if (!reasignarId.value.trim()) return
  await run(async () => {
    await store.reasignar(ticket.value!.id, { actorId: ACTOR_ID, nuevoResponsableId: reasignarId.value.trim() })
    reasignarId.value = ''
  })
}

async function agregarColaborador() {
  if (!nuevoColaborador.value.trim()) return
  await run(async () => {
    await store.agregarColaborador(ticket.value!.id, { colaboradorId: nuevoColaborador.value.trim() })
    nuevoColaborador.value = ''
  })
}

onMounted(() => store.load())
</script>

<template>
  <div class="page">
    <!-- Navegación -->
    <button class="volver" @click="router.back()">
      <i class="pi pi-arrow-left"/> Volver al listado
    </button>

    <Message v-if="store.error" severity="error" :closable="false">
      {{ store.error }} <Button label="Reintentar" text @click="store.load()"/>
    </Message>
    <p v-if="store.loading" role="status">Cargando…</p>
    <Message v-else-if="notFound" severity="warn" :closable="false">
      La solicitud no existe o no está disponible.
    </Message>

    <template v-else-if="ticket">
      <!-- Cabecera -->
      <div class="detalle-header">
        <div class="detalle-header-main">
          <div class="detalle-id-row">
            <code class="ticket-id">{{ ticket.id.slice(0, 14) }}</code>
            <Tag :value="ticket.estado" :severity="severity(ticket.estado)"/>
            <span class="detalle-meta-text">
              {{ ticket.categoria }} · Creada {{ date(ticket.createdAt) }}
            </span>
          </div>
          <h1 class="detalle-titulo">{{ ticket.title }}</h1>
        </div>
      </div>

      <!-- Layout dos columnas -->
      <div class="detalle-layout">

        <!-- ─── Zona izquierda ─────────────────────────────────── -->
        <div class="detalle-izq">

          <!-- Solicitud original + descripción -->
          <section class="panel detalle-card">
            <div class="card-header">
              <i class="pi pi-file-edit"/> <h2>Solicitud original</h2>
            </div>
            <div class="card-body">
              <p class="descripcion">{{ ticket.description }}</p>
            </div>
          </section>

          <!-- REQ-010: Adjuntos de la solicitud original -->
          <section class="panel detalle-card">
            <div class="card-header">
              <i class="pi pi-paperclip"/> <h2>Adjuntos de la solicitud</h2>
              <small class="badge-publica">Visibles para el solicitante</small>
            </div>
            <div class="card-body">
              <div v-if="adjuntosTicket.length" class="adj-lista">
                <div v-for="adj in adjuntosTicket" :key="adj.id" class="adj-chip">
                  <i class="pi pi-file adj-icon"/>
                  <span class="adj-nombre">{{ adj.nombre }}</span>
                  <small class="adj-size">{{ formatSize(adj.tamaño) }}</small>
                  <span class="adj-storage-badge" title="URL pendiente de storage">
                    <i class="pi pi-clock"/> Sin storage
                  </span>
                </div>
              </div>
              <p v-else class="empty">No se adjuntaron archivos a esta solicitud.</p>
            </div>
          </section>

          <!-- REQ-009: Agregar comentario / nota interna -->
          <section class="panel detalle-card">
            <div class="card-header">
              <i class="pi pi-pencil"/> <h2>Registrar actividad</h2>
            </div>
            <div class="card-body">
              <!-- Selector de tipo con diferenciación visual clara (REQ-009) -->
              <div class="tipo-tabs">
                <button type="button"
                  :class="['tipo-tab', 'tipo-tab-interno', { active: visibility === 'Nota interna' }]"
                  @click="visibility = 'Nota interna'">
                  <i class="pi pi-lock"/> Nota interna
                </button>
                <button type="button"
                  :class="['tipo-tab', 'tipo-tab-respuesta', { active: visibility === 'Respuesta al solicitante' }]"
                  @click="visibility = 'Respuesta al solicitante'">
                  <i class="pi pi-send"/> Respuesta al solicitante
                </button>
              </div>

              <!-- Banner de advertencia / confirmación (REQ-009) -->
              <div :class="['vis-banner', visibility === 'Nota interna' ? 'banner-interno' : 'banner-respuesta']">
                <i :class="['pi', visibility === 'Nota interna' ? 'pi-lock' : 'pi-eye']"/>
                <span v-if="visibility === 'Nota interna'">
                  <strong>Solo visible para Tecnología.</strong>
                  El solicitante <em>no</em> verá este mensaje.
                </span>
                <span v-else>
                  <strong>Visible para el solicitante</strong> en su portal de solicitudes.
                </span>
              </div>

              <form :class="['conv-form', visibility === 'Nota interna' ? 'form-interno' : 'form-respuesta']"
                @submit.prevent="guardarActividad">
                <Textarea v-model="note" rows="3" maxlength="10000"
                  :placeholder="visibility === 'Nota interna'
                    ? 'Avance interno, diagnóstico, observación del equipo…'
                    : 'Respuesta o información que el solicitante necesita saber…'"/>

                <!-- REQ-010: adjuntos al comentario/nota -->
                <div class="adj-form-row">
                  <button type="button" class="adj-pick-btn" @click="fileInput?.click()">
                    <i class="pi pi-paperclip"/> Adjuntar archivos
                  </button>
                  <input ref="fileInput" type="file" multiple class="file-hidden" @change="onFileChange"/>
                  <div v-if="archivos.length" class="adj-chips-form">
                    <span v-for="(f, i) in archivos" :key="i" class="adj-chip-form">
                      <i class="pi pi-file"/>{{ f.name }}
                      <small>{{ formatSize(f.size) }}</small>
                      <button type="button" class="quitar-btn" @click="quitarArchivo(i)"><i class="pi pi-times"/></button>
                    </span>
                  </div>
                </div>

                <div class="conv-form-footer">
                  <Button type="submit"
                    :label="visibility === 'Nota interna' ? 'Guardar nota' : 'Enviar respuesta'"
                    :icon="visibility === 'Nota interna' ? 'pi pi-save' : 'pi pi-send'"
                    :severity="visibility === 'Nota interna' ? 'secondary' : 'primary'"
                    size="small" :loading="busy" :disabled="!note.trim()"/>
                </div>
              </form>
            </div>
          </section>

          <!-- Conversación (solo respuestas visibles al solicitante) -->
          <section class="panel detalle-card">
            <div class="card-header">
              <i class="pi pi-comments"/> <h2>Conversación con el solicitante</h2>
              <small class="badge-respuesta">Visible para el solicitante</small>
            </div>
            <div class="card-body">
              <div v-if="conversacion.length" class="conv-list">
                <article v-for="msg in conversacion" :key="msg.id" class="conv-msg conv-msg-respuesta">
                  <div class="conv-msg-header">
                    <i class="pi pi-send conv-avatar-respuesta"/>
                    <span class="conv-autor">Respuesta al solicitante</span>
                    <small>{{ date(msg.createdAt) }}</small>
                  </div>
                  <p class="conv-texto">{{ msg.text }}</p>
                </article>
              </div>
              <p v-else class="empty">Aún no hay mensajes enviados al solicitante.</p>
            </div>
          </section>

          <!-- Línea de tiempo / Trazabilidad -->
          <section class="panel detalle-card">
            <div class="card-header">
              <i class="pi pi-history"/> <h2>Línea de tiempo</h2>
              <small>Trazabilidad completa · solo visible para Tecnología</small>
            </div>
            <div class="card-body">
              <div v-if="lineaDeTiempo.length" class="timeline">
                <!-- REQ-009: diferenciación visual por tipo en la trazabilidad -->
                <article v-for="ev in lineaDeTiempo" :key="ev.id"
                  :class="['tl-entry', ev.visibility === 'Nota interna' ? 'tl-interno' : 'tl-respuesta']">
                  <div class="tl-top">
                    <i :class="['pi', tipoEvento(ev).icon, 'tl-icon']"/>
                    <span class="tl-tipo">{{ tipoEvento(ev).label }}</span>
                    <span v-if="ev.visibility === 'Nota interna'" class="tl-badge tl-badge-interno">
                      <i class="pi pi-lock"/> Interno
                    </span>
                    <span v-else-if="ev.visibility === 'Respuesta al solicitante'" class="tl-badge tl-badge-respuesta">
                      <i class="pi pi-eye"/> Al solicitante
                    </span>
                    <small>{{ date(ev.createdAt) }}</small>
                  </div>
                  <p>{{ ev.text }}</p>
                  <!-- REQ-010: adjuntos de esta actividad -->
                  <div v-if="adjuntosDeActividad(ev.id).length" class="tl-adjuntos">
                    <span v-for="adj in adjuntosDeActividad(ev.id)" :key="adj.id" class="tl-adj-chip">
                      <i class="pi pi-paperclip"/> {{ adj.nombre }}
                      <small>{{ formatSize(adj.tamaño) }}</small>
                    </span>
                  </div>
                </article>
              </div>
              <p v-else class="empty">No hay eventos registrados aún.</p>
            </div>
          </section>
        </div>

        <!-- ─── Zona derecha ──────────────────────────────────── -->
        <aside class="detalle-der">

          <!-- Estado (REQ-012: Resuelta/Rechazada requieren mensaje) -->
          <div class="panel sidebar-card">
            <div class="sidebar-card-label"><i class="pi pi-circle"/> Estado</div>
            <Select :modelValue="ticket.estado" :options="[...statuses]"
              :disabled="busy || !!pendingEstado" @update:modelValue="onEstadoChange" class="w-full"/>

            <!-- Panel de confirmación de cierre (Resuelta / Rechazada) -->
            <div v-if="pendingEstado" :class="['cierre-panel', pendingEstado==='Resuelta' ? 'cierre-resolucion' : 'cierre-rechazo']">
              <div class="cierre-header">
                <i :class="['pi', pendingEstado==='Resuelta' ? 'pi-check-circle' : 'pi-times-circle']"/>
                <strong>{{ pendingEstado === 'Resuelta' ? 'Mensaje de resolución' : 'Motivo del rechazo' }}</strong>
              </div>
              <p class="cierre-hint">
                {{ pendingEstado === 'Resuelta'
                  ? 'Este mensaje será visible para el solicitante. Es obligatorio.'
                  : 'Este motivo será visible para el solicitante. Es obligatorio.' }}
              </p>
              <Textarea v-model="pendingMensaje" rows="4" maxlength="10000"
                :placeholder="pendingEstado === 'Resuelta'
                  ? 'Describe cómo se resolvió la solicitud…'
                  : 'Explica por qué se rechaza la solicitud…'"
                class="cierre-textarea" autofocus/>
              <div class="cierre-actions">
                <Button label="Cancelar" size="small" severity="secondary" text
                  :disabled="busy" @click="cancelarCierre"/>
                <Button
                  :label="pendingEstado === 'Resuelta' ? 'Marcar como Resuelta' : 'Rechazar solicitud'"
                  :icon="pendingEstado === 'Resuelta' ? 'pi pi-check' : 'pi pi-times'"
                  :severity="pendingEstado === 'Resuelta' ? 'success' : 'danger'"
                  size="small" :loading="busy"
                  :disabled="!pendingMensaje.trim()"
                  @click="confirmarCierre"/>
              </div>
            </div>
          </div>

          <!-- Categoría -->
          <div class="panel sidebar-card">
            <div class="sidebar-card-label"><i class="pi pi-tag"/> Categoría</div>
            <Tag :value="ticket.categoria" severity="secondary"/>
          </div>

          <!-- Responsable + Tomar / Reasignar (REQ-005) -->
          <div class="panel sidebar-card">
            <div class="sidebar-card-label"><i class="pi pi-user"/> Responsable</div>
            <div v-if="ticket.responsableId" class="resp-actual">
              <span class="resp-id">{{ ticket.responsableId }}</span>
              <div class="reasig-form">
                <InputText v-model="reasignarId" placeholder="Nuevo responsable"
                  maxlength="160" size="small"/>
                <Button label="Reasignar" size="small" text :loading="busy"
                  :disabled="!reasignarId.trim()" @click="reasignar"/>
              </div>
            </div>
            <div v-else class="sin-responsable">
              <span class="muted">Sin responsable</span>
              <Button label="Tomar solicitud" icon="pi pi-user-plus"
                size="small" :loading="busy" @click="tomarSolicitud"/>
            </div>
          </div>

          <!-- Colaboradores (REQ-005) -->
          <div class="panel sidebar-card">
            <div class="sidebar-card-label"><i class="pi pi-users"/> Colaboradores</div>
            <div v-if="ticket.colaboradores.length" class="colab-list">
              <span v-for="c in ticket.colaboradores" :key="c.colaboradorId" class="colab-chip">
                {{ c.colaboradorId }}
              </span>
            </div>
            <p v-else class="muted" style="font-size:12px;margin:0">Ninguno</p>
            <div class="colab-form">
              <InputText v-model="nuevoColaborador" placeholder="ID colaborador"
                maxlength="160" size="small"/>
              <Button label="Agregar" size="small" text :loading="busy"
                :disabled="!nuevoColaborador.trim()" @click="agregarColaborador"/>
            </div>
          </div>

          <!-- Metadatos -->
          <div class="panel sidebar-card">
            <div class="sidebar-card-label"><i class="pi pi-info-circle"/> Metadatos</div>
            <dl class="meta-dl">
              <dt>Folio</dt>       <dd><code>{{ ticket.id.slice(0,14) }}</code></dd>
              <dt>Solicitante</dt> <dd>{{ ticket.solicitanteId }}</dd>
              <dt>Rol</dt>         <dd class="muted">— (pendiente)</dd>
              <dt>Creada</dt>      <dd>{{ date(ticket.createdAt) }}</dd>
            </dl>
          </div>

          <!-- Trabajo técnico relacionado (REQ-014 pendiente) -->
          <div class="panel sidebar-card placeholder-card">
            <div class="sidebar-card-label"><i class="pi pi-wrench"/> Trabajo técnico</div>
            <p class="muted" style="font-size:12px;margin:0">
              Sin trabajo técnico vinculado.<br>
              <small>Próximamente · <abbr title="REQ-014 — Relación Solicitud ↔ Trabajo técnico">REQ-014</abbr></small>
            </p>
          </div>

        </aside>
      </div>
    </template>
  </div>
</template>

<style scoped>
.volver {
  display: inline-flex; align-items: center; gap: 8px;
  border: 0; background: none; color: #2563eb; font-size: 13px;
  cursor: pointer; padding: 0; margin-bottom: 20px;
}
.volver:hover { text-decoration: underline; }

/* Cabecera */
.detalle-header      { margin-bottom: 24px; }
.detalle-id-row      { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; margin-bottom: 8px; }
.ticket-id           { font-size: 12px; color: #64748b; background: #f1f5f9; padding: 2px 8px; border-radius: 4px; }
.detalle-meta-text   { font-size: 12px; color: #64748b; }
.detalle-titulo      { font-size: 24px; font-weight: 700; letter-spacing: -.5px; margin: 0; line-height: 1.3; }

/* Layout dos columnas */
.detalle-layout {
  display: grid;
  grid-template-columns: minmax(0,1fr) 280px;
  gap: 22px;
  align-items: start;
}

/* Zona izquierda */
.detalle-izq { display: flex; flex-direction: column; gap: 18px; }

.detalle-card { overflow: hidden; }
.card-header  {
  display: flex; align-items: center; gap: 10px;
  padding: 16px 20px; border-bottom: 1px solid #edf1f5;
}
.card-header i { color: #3b82f6; font-size: 15px; }
.card-header h2 { font-size: 14px; font-weight: 700; margin: 0; flex: 1; }
.card-header small { color: #94a3b8; }
.card-body { padding: 20px; }

.descripcion { white-space: pre-wrap; line-height: 1.75; margin: 0; font-size: 14px; }

/* Adjuntos placeholder */
.adjuntos-placeholder { text-align: center; padding: 32px 20px; color: #94a3b8; }
.adjuntos-icon { font-size: 32px; margin-bottom: 12px; display: block; }
.adjuntos-placeholder p { margin: 0 0 6px; font-size: 13px; }
.adjuntos-placeholder abbr { text-decoration: none; font-weight: 600; }

/* REQ-009: Selector de tipo con diferenciación visual */
.tipo-tabs {
  display: flex; gap: 0; border: 1px solid #e2e8f0; border-radius: 8px;
  overflow: hidden; margin-bottom: 12px;
}
.tipo-tab {
  flex: 1; display: flex; align-items: center; justify-content: center;
  gap: 7px; padding: 9px 14px; border: 0; background: #f8fafc;
  font-size: 13px; font-weight: 500; cursor: pointer; color: #64748b;
  transition: background .15s, color .15s;
}
.tipo-tab:first-child { border-right: 1px solid #e2e8f0; }
.tipo-tab-interno.active  { background: #fffbeb; color: #92400e; font-weight: 700; }
.tipo-tab-respuesta.active { background: #eff6ff; color: #1d4ed8; font-weight: 700; }
.tipo-tab:not(.active):hover { background: #f1f5f9; color: #334155; }

/* REQ-009: Banner de advertencia / confirmación */
.vis-banner {
  display: flex; align-items: flex-start; gap: 10px;
  padding: 10px 14px; border-radius: 7px; font-size: 12px;
  margin-bottom: 12px; line-height: 1.5;
}
.banner-interno  { background: #fffbeb; color: #92400e; border: 1px solid #fde68a; }
.banner-interno i { color: #d97706; margin-top: 1px; }
.banner-respuesta { background: #eff6ff; color: #1e40af; border: 1px solid #bfdbfe; }
.banner-respuesta i { color: #3b82f6; margin-top: 1px; }

/* REQ-009: Formulario con borde indicativo del tipo */
.conv-form { display: flex; flex-direction: column; gap: 8px; }
.conv-form.form-interno .p-textarea  { border-color: #fbbf24 !important; }
.conv-form.form-respuesta .p-textarea { border-color: #93c5fd !important; }
.conv-form .p-textarea { width: 100%; }
.conv-form-footer { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; }

/* Conversación con solicitante */
.conv-list { display: flex; flex-direction: column; gap: 14px; }
.conv-msg { border-radius: 8px; padding: 14px 16px; }
.conv-msg-respuesta { background: #eff6ff; border: 1px solid #bfdbfe; }
.conv-msg-header { display: flex; align-items: center; gap: 8px; margin-bottom: 8px; }
.conv-avatar-respuesta { font-size: 16px; color: #3b82f6; }
.conv-autor  { font-size: 12px; font-weight: 600; color: #1d4ed8; }
.conv-msg-header small { margin-left: auto; color: #64748b; }
.conv-texto  { margin: 0; font-size: 13px; line-height: 1.65; white-space: pre-wrap; }

/* Badge encabezado conversación */
.badge-respuesta {
  font-size: 11px; padding: 2px 8px; border-radius: 10px;
  background: #dbeafe; color: #1d4ed8; font-weight: 600;
}

/* REQ-009: Línea de tiempo con diferenciación visual por tipo */
.tl-entry { padding: 10px 12px; border-radius: 7px; border-left: 3px solid transparent; margin-bottom: 2px; }
.tl-interno  { background: #fffbeb; border-left-color: #fbbf24; }
.tl-respuesta { background: #eff6ff; border-left-color: #93c5fd; }
/* Entradas de sistema (sin visibility explícita) se muestran en gris claro */
.tl-entry:not(.tl-interno):not(.tl-respuesta) { background: #f8fafc; border-left-color: #e2e8f0; }

.tl-top  { display: flex; align-items: center; gap: 8px; margin-bottom: 4px; flex-wrap: wrap; }
.tl-icon { font-size: 13px; color: #3b82f6; }
.tl-tipo { font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: .5px; color: #64748b; flex: 1; }
.tl-top small { color: #94a3b8; }

/* REQ-010: Adjuntos */
.badge-publica {
  font-size: 11px; padding: 2px 8px; border-radius: 10px;
  background: #dcfce7; color: #166534; font-weight: 600;
}
.adj-lista { display: flex; flex-direction: column; gap: 6px; }
.adj-chip {
  display: flex; align-items: center; gap: 8px;
  padding: 8px 10px; background: #f8fafc; border: 1px solid #e2e8f0;
  border-radius: 6px; font-size: 12px;
}
.adj-icon { color: #3b82f6; font-size: 14px; }
.adj-nombre { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.adj-size { color: #94a3b8; white-space: nowrap; }
.adj-storage-badge {
  display: inline-flex; align-items: center; gap: 4px;
  font-size: 10px; color: #f59e0b; background: #fffbeb;
  border: 1px solid #fde68a; border-radius: 8px; padding: 1px 6px;
  white-space: nowrap;
}

.adj-form-row { display: flex; flex-direction: column; gap: 6px; margin: 4px 0; }
.adj-pick-btn {
  display: inline-flex; align-items: center; gap: 6px;
  border: 1px dashed #93c5fd; background: #f0f9ff; color: #3b82f6;
  font-size: 12px; padding: 6px 12px; border-radius: 6px; cursor: pointer;
  align-self: flex-start;
}
.adj-pick-btn:hover { background: #dbeafe; }
.file-hidden { display: none; }
.adj-chips-form { display: flex; flex-wrap: wrap; gap: 5px; }
.adj-chip-form {
  display: inline-flex; align-items: center; gap: 5px;
  font-size: 11px; padding: 4px 8px; background: #f1f5f9;
  border: 1px solid #e2e8f0; border-radius: 10px; color: #334155;
}
.adj-chip-form i { color: #3b82f6; }
.adj-chip-form small { color: #94a3b8; }
.quitar-btn { border: 0; background: none; color: #94a3b8; cursor: pointer; padding: 1px; line-height: 1; }
.quitar-btn:hover { color: #ef4444; }

.tl-adjuntos { display: flex; flex-wrap: wrap; gap: 5px; margin-top: 6px; }
.tl-adj-chip {
  display: inline-flex; align-items: center; gap: 5px;
  font-size: 11px; padding: 3px 8px; background: #f8fafc;
  border: 1px solid #e2e8f0; border-radius: 8px; color: #334155;
}
.tl-adj-chip i { color: #64748b; font-size: 10px; }
.tl-adj-chip small { color: #94a3b8; }

/* REQ-009: Badges de visibilidad en la timeline */
.tl-badge {
  display: inline-flex; align-items: center; gap: 4px;
  font-size: 10px; font-weight: 700; padding: 1px 7px; border-radius: 10px;
  text-transform: uppercase; letter-spacing: .4px;
}
.tl-badge-interno  { background: #fef3c7; color: #92400e; }
.tl-badge-respuesta { background: #dbeafe; color: #1d4ed8; }
.tl-badge i { font-size: 9px; }

/* Zona derecha */
.detalle-der { display: flex; flex-direction: column; gap: 14px; }
.sidebar-card { padding: 16px 18px; display: flex; flex-direction: column; gap: 10px; }
.sidebar-card-label {
  font-size: 11px; font-weight: 700; text-transform: uppercase; letter-spacing: .6px;
  color: #64748b; display: flex; align-items: center; gap: 6px;
}
.sidebar-card-label i { color: #3b82f6; }
.w-full { width: 100%; }

.resp-actual   { display: flex; flex-direction: column; gap: 8px; }
.resp-id       { font-size: 13px; font-weight: 600; color: #1e293b; }
.reasig-form   { display: flex; gap: 6px; flex-wrap: wrap; }
.sin-responsable { display: flex; flex-direction: column; gap: 8px; }

.colab-list { display: flex; flex-wrap: wrap; gap: 6px; }
.colab-chip { background: #dbeafe; color: #1d4ed8; font-size: 11px; padding: 3px 9px; border-radius: 12px; font-weight: 500; }
.colab-form { display: flex; gap: 6px; flex-wrap: wrap; }

.meta-dl { margin: 0; display: grid; grid-template-columns: auto 1fr; gap: 4px 12px; font-size: 12px; }
.meta-dl dt { color: #64748b; font-weight: 600; }
.meta-dl dd { margin: 0; color: #1e293b; word-break: break-all; }
.meta-dl code { font-size: 11px; background: #f1f5f9; padding: 1px 5px; border-radius: 3px; }

/* REQ-012: Panel inline de resolución / rechazo */
.cierre-panel {
  border-radius: 8px; padding: 14px; margin-top: 4px;
  display: flex; flex-direction: column; gap: 10px;
  border: 1px solid transparent;
}
.cierre-resolucion { background: #f0fdf4; border-color: #86efac; }
.cierre-rechazo    { background: #fff1f2; border-color: #fca5a5; }
.cierre-header {
  display: flex; align-items: center; gap: 8px; font-size: 13px;
}
.cierre-resolucion .cierre-header i { color: #16a34a; }
.cierre-rechazo    .cierre-header i { color: #dc2626; }
.cierre-hint { margin: 0; font-size: 12px; color: #64748b; line-height: 1.5; }
.cierre-textarea { width: 100%; font-size: 13px; }
.cierre-actions { display: flex; justify-content: flex-end; gap: 8px; flex-wrap: wrap; }

.placeholder-card { opacity: .75; }
.muted { color: #94a3b8; }

@media (max-width: 860px) {
  .detalle-layout { grid-template-columns: 1fr; }
  .detalle-der { order: -1; display: grid; grid-template-columns: repeat(2, 1fr); gap: 12px; }
}
@media (max-width: 540px) {
  .detalle-der { grid-template-columns: 1fr; }
  .detalle-titulo { font-size: 20px; }
}
</style>
