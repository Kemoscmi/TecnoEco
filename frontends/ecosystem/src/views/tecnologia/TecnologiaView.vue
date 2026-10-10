<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Select from 'primevue/select'
import Dialog from 'primevue/dialog'
import Tag from 'primevue/tag'
import Message from 'primevue/message'
import { useToast } from 'primevue/usetoast'
import { useTecnologiaStore } from '@/stores/tecnologia.store'
import { statuses, categorias } from '@/types/tecnologia'

const store  = useTecnologiaStore()
const route  = useRoute()
const router = useRouter()
const toast  = useToast()

const section = computed(() => route.params.section)

// ── Listado (REQ-007) ──────────────────────────────────────────────────
const query    = ref('')
const categoria = ref('Todos')
const status   = ref(typeof route.query.estado === 'string' ? route.query.estado : 'Todos')
const sinAsignarFiltro = ref(route.query.sinAsignar === '1')
const vista    = ref<'todas' | 'asignadas'>('todas')
const orden    = ref<'Más recientes' | 'Más antiguas'>('Más recientes')

// Demo: actor para tab "Asignadas a mí"
const ACTOR_ID = 'tecnologia-demo'

const dateShort = (s: string) =>
  new Date(s).toLocaleDateString('es-MX', { day: '2-digit', month: 'short', year: 'numeric' })

const filtered = computed(() => {
  let list = store.data.tickets.filter(t => {
    const texto = (t.title + ' ' + t.id + ' ' + t.solicitanteId).toLocaleLowerCase()
    if (!texto.includes(query.value.toLocaleLowerCase())) return false
    if (categoria.value !== 'Todos' && t.categoria !== categoria.value) return false
    if (sinAsignarFiltro.value && t.responsableId) return false
    if (!sinAsignarFiltro.value && status.value !== 'Todos' && t.estado !== status.value) return false
    if (vista.value === 'asignadas' && t.responsableId !== ACTOR_ID) return false
    return true
  })
  list = [...list].sort((a, b) => {
    const diff = new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    return orden.value === 'Más recientes' ? diff : -diff
  })
  return list
})

function limpiarFiltroContexto() {
  sinAsignarFiltro.value = false
  status.value = 'Todos'
  router.replace({ path: '/tecnologia/tickets' })
}

watch(() => route.query, q => {
  status.value = typeof q.estado === 'string' ? q.estado : 'Todos'
  sinAsignarFiltro.value = q.sinAsignar === '1'
})

// REQ-008: abrir la página de detalle en vez de un Dialog
function abrirDetalle(id: string) {
  router.push('/tecnologia/solicitudes/' + id)
}

// ── Bitácora general ───────────────────────────────────────────────────
const busy        = ref(false)
const general     = ref('')
const generalOpen = ref(false)
const date = (s: string) => new Date(s).toLocaleString('es-MX', { dateStyle: 'medium', timeStyle: 'short' })

async function run(action: () => Promise<void>) {
  busy.value = true
  try { await action(); toast.add({ severity: 'success', summary: 'Guardado', life: 2200 }) }
  catch (e) { toast.add({ severity: 'error', summary: 'Error', detail: e instanceof Error ? e.message : 'Intenta de nuevo', life: 6000 }) }
  finally { busy.value = false }
}
async function saveGeneral() {
  if (!general.value.trim()) return
  await run(async () => {
    await store.addActivity({ ticketId: null, text: general.value.trim(), visibility: 'Nota interna' })
    general.value = ''
    generalOpen.value = false
  })
}

const severity = (s: string) =>
  s === 'Resuelta' ? 'success' : s === 'Necesitamos información' ? 'secondary' : s === 'En proceso' ? 'warn' : 'info'

onMounted(() => store.load())
</script>

<template>
<div class="page">
  <div class="page-heading">
    <div><h1>Tecnología</h1><p>Un espacio para reportar, resolver y dejar constancia.</p></div>
    <Button label="Nueva solicitud" icon="pi pi-plus" :disabled="!store.ready||busy"
      @click="router.push('/tecnologia/solicitudes/nueva')"/>
  </div>

  <Message v-if="store.error" severity="error" :closable="false">
    {{ store.error }} <Button label="Reintentar" text @click="store.load()"/>
  </Message>

  <div class="tabs">
    <RouterLink to="/tecnologia/tickets">Solicitudes</RouterLink>
    <RouterLink to="/tecnologia/tablero">Tablero</RouterLink>
    <RouterLink to="/tecnologia/bitacora">Bitácora general</RouterLink>
  </div>

  <p v-if="store.loading" role="status">Cargando…</p>

  <!-- ── REQ-007: Listado ──────────────────────────────────────────── -->
  <div v-else-if="section==='tickets'" class="listado-layout">
    <section class="panel listado-panel">

      <div class="panel-heading">
        <div>
          <h2>Listado de solicitudes</h2>
          <small v-if="sinAsignarFiltro" class="filtro-activo">
            <i class="pi pi-filter-fill"/> Contexto: Sin asignar ·
            <button class="text-link" @click="limpiarFiltroContexto">Limpiar</button>
          </small>
          <small v-else-if="status!=='Todos'" class="filtro-activo">
            <i class="pi pi-filter-fill"/> Contexto: {{ status }} ·
            <button class="text-link" @click="limpiarFiltroContexto">Limpiar</button>
          </small>
        </div>
        <Button label="Nueva solicitud" icon="pi pi-plus" size="small"
          :disabled="!store.ready||busy" @click="router.push('/tecnologia/solicitudes/nueva')"/>
      </div>

      <div class="list-tabs">
        <button :class="['list-tab', { active: vista==='todas' }]" @click="vista='todas'">
          Todas <span class="tab-count">{{ store.data.tickets.length }}</span>
        </button>
        <button :class="['list-tab', { active: vista==='asignadas' }]" @click="vista='asignadas'">
          Asignadas a mí
          <span class="tab-count">{{ store.data.tickets.filter(t=>t.responsableId===ACTOR_ID).length }}</span>
        </button>
      </div>

      <div class="filters">
        <InputText v-model="query" aria-label="Buscar solicitudes"
          placeholder="Buscar por #, título o solicitante…" class="search-input"/>
        <Select v-model="categoria" :options="['Todos',...categorias]"
          aria-label="Categoría" :disabled="sinAsignarFiltro"/>
        <Select v-model="status" :options="['Todos',...statuses]"
          aria-label="Estado" :disabled="sinAsignarFiltro"/>
        <Select v-model="orden" :options="['Más recientes','Más antiguas']"
          aria-label="Ordenar"/>
      </div>

      <div class="table-scroll">
        <table>
          <thead>
            <tr>
              <th class="col-num">#</th>
              <th>Título</th>
              <th>Solicitante / Rol</th>
              <th>Categoría</th>
              <th>Responsable</th>
              <th>Estado</th>
              <th>Creada</th>
            </tr>
          </thead>
          <tbody>
            <!-- REQ-008: clic navega a la página de detalle -->
            <tr v-for="(ticket, idx) in filtered" :key="ticket.id"
              class="fila-ticket" @click="abrirDetalle(ticket.id)">
              <td class="col-num">{{ idx + 1 }}</td>
              <td>
                <span class="tit-link">{{ ticket.title }}</span>
                <small class="tit-id">{{ ticket.id.slice(0,14) }}</small>
              </td>
              <td>
                <span>{{ ticket.solicitanteId }}</span>
                <small class="rol-label">—</small>
              </td>
              <td><Tag :value="ticket.categoria" severity="secondary"/></td>
              <td>{{ ticket.responsableId ?? '—' }}</td>
              <td><Tag :value="ticket.estado" :severity="severity(ticket.estado)"/></td>
              <td class="col-fecha">{{ dateShort(ticket.createdAt) }}</td>
            </tr>
            <tr v-if="!filtered.length">
              <td colspan="7" class="empty">No hay solicitudes que coincidan con los filtros.</td>
            </tr>
          </tbody>
        </table>
      </div>
      <div class="tabla-footer">
        <small>{{ filtered.length }} solicitud{{ filtered.length !== 1 ? 'es' : '' }}</small>
      </div>
    </section>
  </div>

  <!-- Tablero Kanban -->
  <div v-else-if="section==='tablero'" class="board">
    <section v-for="column in statuses" :key="column" class="board-column">
      <h2>{{ column }} <small>{{ store.data.tickets.filter(t=>t.estado===column).length }}</small></h2>
      <!-- REQ-008: clic navega a la página de detalle -->
      <button v-for="ticket in store.data.tickets.filter(t=>t.estado===column)" :key="ticket.id"
        class="board-card" @click="abrirDetalle(ticket.id)">
        <small>{{ ticket.categoria }} · {{ ticket.id.slice(0,14) }}</small>
        <strong>{{ ticket.title }}</strong>
        <p>{{ ticket.responsableId ?? 'Sin responsable' }}</p>
      </button>
      <p v-if="!store.data.tickets.some(t=>t.estado===column)" class="empty">Sin tickets</p>
    </section>
  </div>

  <!-- Bitácora general -->
  <section v-else class="panel">
    <div class="panel-heading">
      <div><h2>Bitácora general</h2><small>Avances, cambios de estado y actividades del equipo.</small></div>
      <Button label="Registrar actividad" icon="pi pi-plus"
        :disabled="!store.ready||busy" @click="generalOpen=true"/>
    </div>
    <div class="timeline">
      <article v-for="event in store.data.activities" :key="event.id">
        <!-- REQ-008: enlace al detalle desde la bitácora -->
        <button v-if="event.ticketId" class="text-link" @click="abrirDetalle(event.ticketId)">
          {{ event.ticketId.slice(0,14) }}
        </button>
        <b v-else>Actividad general</b>
        <p>{{ event.text }}</p>
        <small>{{ date(event.createdAt) }} · {{ event.visibility }}</small>
      </article>
      <p v-if="!store.data.activities.length" class="empty">Registra la primera actividad del equipo.</p>
    </div>
  </section>

  <!-- Dialog: actividad general (solo para bitácora sin ticket) -->
  <Dialog v-model:visible="generalOpen" modal header="Registrar actividad general"
    :style="{width:'560px'}" :breakpoints="{'600px':'95vw'}">
    <form @submit.prevent="saveGeneral">
      <label for="general">Actividad realizada</label>
      <Textarea id="general" v-model="general" required rows="5" maxlength="10000"
        placeholder="Mantenimiento, revisión de equipos, actualización…"/>
      <p>Se guardará como nota interna sin vincularla a un ticket.</p>
      <div class="actions"><Button type="submit" label="Guardar actividad" :loading="busy"/></div>
    </form>
  </Dialog>
</div>
</template>

<style scoped>
.listado-layout { display: flex; flex-direction: column; gap: 0; }
.listado-panel  { overflow: hidden; }

.list-tabs { display: flex; gap: 0; border-bottom: 1px solid #e2e8f0; padding: 0 20px; }
.list-tab  {
  padding: 12px 18px; border: 0; background: none; color: #64748b; font-size: 13px;
  cursor: pointer; border-bottom: 3px solid transparent; margin-bottom: -1px;
  display: flex; align-items: center; gap: 7px;
}
.list-tab.active { color: #2563eb; border-bottom-color: #3b82f6; font-weight: 600; }
.list-tab:hover:not(.active) { color: #334155; }
.tab-count {
  background: #f1f5f9; color: #64748b; font-size: 11px; font-weight: 600;
  padding: 1px 7px; border-radius: 10px;
}
.list-tab.active .tab-count { background: #dbeafe; color: #2563eb; }

.filters { padding: 14px 20px; display: flex; gap: 10px; flex-wrap: wrap; }
.search-input { flex: 1; min-width: 180px; }

.col-num   { width: 42px; text-align: center; color: #94a3b8; font-size: 12px; }
.col-fecha { white-space: nowrap; font-size: 12px; color: #64748b; }
.fila-ticket { cursor: pointer; }
.fila-ticket:hover td { background: #f8fafc; }
.tit-link  { display: block; font-size: 13px; font-weight: 600; color: #1e293b; line-height: 1.4; }
.fila-ticket:hover .tit-link { color: #2563eb; }
.tit-id    { display: block; font-size: 11px; color: #94a3b8; margin-top: 3px; }
.rol-label { display: block; font-size: 11px; color: #94a3b8; }
.tabla-footer { padding: 12px 20px; border-top: 1px solid #eef2f6; }

.filtro-activo { display: flex; align-items: center; gap: 6px; margin-top: 4px; }
.filtro-activo i { font-size: 10px; color: #3b82f6; }

@media (max-width: 900px) {
  .filters { flex-direction: column; }
  .search-input { min-width: 0; }
  th.col-fecha, td.col-fecha { display: none; }
}
@media (max-width: 640px) {
  th:nth-child(3), td:nth-child(3),
  th:nth-child(4), td:nth-child(4) { display: none; }
}
</style>
