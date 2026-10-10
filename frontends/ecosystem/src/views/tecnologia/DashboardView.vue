<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Message from 'primevue/message'
import Button from 'primevue/button'
import { useTecnologiaStore } from '@/stores/tecnologia.store'
import { statuses } from '@/types/tecnologia'

const store = useTecnologiaStore()
const router = useRouter()

// REQ-006: cards numéricas por estado + Sin asignar.
// "Asignadas a mí" explícitamente fuera de alcance.
const cards = computed(() => [
  ...statuses.map(estado => ({
    label: estado,
    count: store.data.tickets.filter(t => t.estado === estado).length,
    icon: iconoPor(estado),
    acento: acentoPor(estado),
    query: { estado },
  })),
  {
    label: 'Sin asignar',
    count: store.data.tickets.filter(t => !t.responsableId).length,
    icon: 'pi-user-minus',
    acento: 'gris',
    query: { sinAsignar: '1' },
  },
])

function iconoPor(estado: string) {
  if (estado === 'Recibida') return 'pi-inbox'
  if (estado === 'En proceso') return 'pi-spin pi-spinner'
  if (estado === 'Necesitamos información') return 'pi-question-circle'
  if (estado === 'Resuelta') return 'pi-check-circle'
  if (estado === 'Rechazada') return 'pi-times-circle'
  return 'pi-circle'
}

function acentoPor(estado: string) {
  if (estado === 'Recibida') return 'azul'
  if (estado === 'En proceso') return 'naranja'
  if (estado === 'Necesitamos información') return 'amarillo'
  if (estado === 'Resuelta') return 'verde'
  if (estado === 'Rechazada') return 'rojo'
  return 'gris'
}

function abrir(card: typeof cards.value[number]) {
  router.push({ path: '/tecnologia/tickets', query: card.query })
}

onMounted(() => store.load())
</script>

<template>
  <div class="page">
    <div class="page-heading">
      <div>
        <h1>Dashboard</h1>
        <p>Resumen de solicitudes del equipo de Tecnología.</p>
      </div>
      <Button
        label="Nueva solicitud"
        icon="pi pi-plus"
        :disabled="!store.ready"
        @click="$router.push('/tecnologia/solicitudes/nueva')"
      />
    </div>

    <Message v-if="store.error" severity="error" :closable="false">
      {{ store.error }} <Button label="Reintentar" text @click="store.load()" />
    </Message>

    <p v-if="store.loading" role="status">Cargando…</p>

    <div v-else class="dash-grid">
      <button
        v-for="card in cards"
        :key="card.label"
        class="dash-card"
        :class="card.acento"
        @click="abrir(card)"
      >
        <div class="dash-card-top">
          <span class="dash-label">{{ card.label }}</span>
          <i :class="['pi', card.icon, 'dash-icon']" />
        </div>
        <div class="dash-count">{{ card.count }}</div>
        <div class="dash-link">Ver listado →</div>
      </button>
    </div>

    <!-- Actividad reciente como contexto del equipo -->
    <section v-if="store.data.activities.length" class="panel dash-actividad">
      <div class="panel-heading">
        <h2>Actividad reciente</h2>
        <small>Últimas 8 entradas de la bitácora</small>
      </div>
      <div class="timeline">
        <article v-for="event in store.data.activities.slice(0, 8)" :key="event.id">
          <small>{{ event.ticketId?.slice(0, 14) || 'Actividad general' }}</small>
          <p>{{ event.text }}</p>
          <small>{{ new Date(event.createdAt).toLocaleString('es-MX', { dateStyle: 'medium', timeStyle: 'short' }) }} · {{ event.visibility }}</small>
        </article>
      </div>
    </section>
  </div>
</template>

<style scoped>
.dash-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 18px;
  margin-bottom: 28px;
}

.dash-card {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 22px 24px;
  background: white;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  text-align: left;
  cursor: pointer;
  transition: box-shadow .15s, border-color .15s, transform .1s;
  border-left-width: 4px;
}
.dash-card:hover {
  box-shadow: 0 4px 14px rgba(0,0,0,.08);
  transform: translateY(-1px);
}

/* Acentos por estado */
.dash-card.azul    { border-left-color: #3b82f6; }
.dash-card.naranja { border-left-color: #f59e0b; }
.dash-card.amarillo{ border-left-color: #eab308; }
.dash-card.verde   { border-left-color: #22c55e; }
.dash-card.rojo    { border-left-color: #ef4444; }
.dash-card.gris    { border-left-color: #94a3b8; }

.dash-card-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.dash-label { font-size: 13px; font-weight: 600; color: #475569; }
.dash-icon  { font-size: 18px; color: #94a3b8; }

.dash-card.azul     .dash-icon { color: #3b82f6; }
.dash-card.naranja  .dash-icon { color: #f59e0b; }
.dash-card.amarillo .dash-icon { color: #eab308; }
.dash-card.verde    .dash-icon { color: #22c55e; }
.dash-card.rojo     .dash-icon { color: #ef4444; }

.dash-count {
  font-size: 42px;
  font-weight: 700;
  letter-spacing: -1.5px;
  color: #0f172a;
  line-height: 1;
  margin: 8px 0 4px;
}
.dash-link {
  font-size: 12px;
  color: #64748b;
}
.dash-card:hover .dash-link { color: #2563eb; }

.dash-actividad { margin-top: 8px; }

@media (max-width: 900px) {
  .dash-grid { grid-template-columns: repeat(2, 1fr); }
}
@media (max-width: 580px) {
  .dash-grid { grid-template-columns: 1fr; }
  .dash-count { font-size: 34px; }
}
</style>
