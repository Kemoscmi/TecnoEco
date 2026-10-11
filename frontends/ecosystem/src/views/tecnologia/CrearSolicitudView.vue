<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import Button from 'primevue/button'
import InputText from 'primevue/inputtext'
import Textarea from 'primevue/textarea'
import Select from 'primevue/select'
import { useToast } from 'primevue/usetoast'
import { useTecnologiaStore } from '@/stores/tecnologia.store'
import { categorias } from '@/types/tecnologia'

// REQ-002: el solicitante debe obtenerse del usuario autenticado.
// Sin autenticación implementada, se usa un ID simulado en modo demo.
// En producción este valor vendrá del perfil del usuario en sesión.
const DEMO_SOLICITANTE_ID = 'usuario-demo'

const store = useTecnologiaStore()
const router = useRouter()
const toast = useToast()
const busy = ref(false)

interface Form { titulo: string; descripcion: string; categoria: string }
const form = reactive<Form>({ titulo: '', descripcion: '', categoria: categorias[0] })

// REQ-010: archivos seleccionados (metadata únicamente; sin storage hasta REQ-010 fase 2)
const archivos = ref<File[]>([])
const fileInput = ref<HTMLInputElement | null>(null)

function onFileChange(e: Event) {
  const input = e.target as HTMLInputElement
  if (!input.files) return
  const nuevos = Array.from(input.files).filter(f => !archivos.value.some(a => a.name === f.name && a.size === f.size))
  archivos.value = [...archivos.value, ...nuevos]
  input.value = ''
}
function quitarArchivo(idx: number) { archivos.value = archivos.value.filter((_, i) => i !== idx) }
function formatSize(b: number) { return b < 1024 ? b + ' B' : b < 1048576 ? (b/1024).toFixed(1) + ' KB' : (b/1048576).toFixed(1) + ' MB' }

const tituloValido = () => form.titulo.trim().length > 0 && form.titulo.trim().length <= 140
const descripcionValida = () => form.descripcion.trim().length > 0 && form.descripcion.trim().length <= 10000

async function enviar() {
  if (!tituloValido() || !descripcionValida()) return
  busy.value = true
  try {
    await store.create({
      title: form.titulo.trim(),
      description: form.descripcion.trim(),
      categoria: form.categoria,
      solicitanteId: DEMO_SOLICITANTE_ID,
      adjuntos: archivos.value.map(f => ({ nombre: f.name, tipo: f.type || 'application/octet-stream', tamaño: f.size })),
    })
    toast.add({ severity: 'success', summary: 'Solicitud creada', detail: 'Nació en estado Recibida.', life: 3000 })
    router.push('/tecnologia/tickets')
  } catch (e) {
    toast.add({ severity: 'error', summary: 'No se pudo crear la solicitud', detail: e instanceof Error ? e.message : 'Intenta de nuevo.', life: 6000 })
  } finally {
    busy.value = false
  }
}

function cancelar() { router.push('/tecnologia/tickets') }
</script>

<template>
  <div class="page">
    <div class="crear-header">
      <div>
        <h1>Nueva solicitud</h1>
        <p>Describe la necesidad con claridad. El equipo de Tecnología la revisará al recibirla.</p>
      </div>
    </div>

    <div class="crear-layout">
      <form class="crear-form panel" @submit.prevent="enviar">

        <!-- Título: primer elemento visible, tamaño prominente -->
        <div class="campo-titulo">
          <label for="titulo">Título <span class="requerido">*</span></label>
          <InputText
            id="titulo"
            v-model="form.titulo"
            class="input-titulo"
            placeholder="Resume la necesidad en una línea"
            maxlength="140"
            required
            autofocus
          />
          <small class="contador" :class="{ limite: form.titulo.length > 120 }">
            {{ form.titulo.length }} / 140
          </small>
        </div>

        <!-- Descripción: elemento principal del formulario -->
        <div class="campo-descripcion">
          <label for="descripcion">Descripción <span class="requerido">*</span></label>
          <small class="hint-desc">Explica el contexto, los pasos para reproducir si aplica y el resultado esperado.</small>
          <Textarea
            id="descripcion"
            v-model="form.descripcion"
            class="textarea-principal"
            placeholder="Escribe aquí toda la información relevante…"
            :rows="10"
            maxlength="10000"
            required
          />
          <small class="contador" :class="{ limite: form.descripcion.length > 9500 }">
            {{ form.descripcion.length }} / 10 000
          </small>
        </div>

        <!-- Campos secundarios -->
        <div class="campos-secundarios">
          <div>
            <label for="categoria">Categoría</label>
            <Select
              inputId="categoria"
              v-model="form.categoria"
              :options="[...categorias]"
              class="select-categoria"
            />
          </div>

          <!-- REQ-010: adjuntos -->
          <div class="adjuntos-campo">
            <label>Adjuntos</label>
            <div class="adjuntos-drop" @click="fileInput?.click()">
              <i class="pi pi-paperclip"/>
              <span>Haz clic para adjuntar archivos</span>
              <small>Los archivos se registran como metadatos · descarga pendiente de storage</small>
            </div>
            <input ref="fileInput" type="file" multiple class="file-hidden" @change="onFileChange"/>
            <ul v-if="archivos.length" class="archivos-lista">
              <li v-for="(f, i) in archivos" :key="i" class="archivo-chip">
                <i class="pi pi-file"/>
                <span>{{ f.name }}</span>
                <small>{{ formatSize(f.size) }}</small>
                <button type="button" class="quitar-btn" @click="quitarArchivo(i)">
                  <i class="pi pi-times"/>
                </button>
              </li>
            </ul>
          </div>
        </div>

        <!-- Solicitante simulado (REQ-002: debe venir del usuario autenticado) -->
        <div class="solicitante-demo">
          <i class="pi pi-user-edit" />
          <div>
            <strong>{{ DEMO_SOLICITANTE_ID }}</strong>
            <small>Modo demo · En producción se usará tu perfil autenticado.</small>
          </div>
        </div>

        <div class="actions">
          <Button label="Cancelar" severity="secondary" text :disabled="busy" @click="cancelar" />
          <Button
            type="submit"
            label="Crear solicitud"
            icon="pi pi-send"
            :loading="busy"
            :disabled="!tituloValido() || !descripcionValida()"
          />
        </div>
      </form>

      <!-- Panel lateral con orientación -->
      <aside class="orientacion panel">
        <div class="panel-heading"><h2>¿Cómo escribir una buena solicitud?</h2></div>
        <div class="orientacion-body">
          <div class="tip">
            <i class="pi pi-align-left" />
            <div>
              <strong>Título claro</strong>
              <p>Una sola oración que indique qué necesitas o qué falla.</p>
            </div>
          </div>
          <div class="tip">
            <i class="pi pi-list" />
            <div>
              <strong>Contexto suficiente</strong>
              <p>¿Cuándo ocurre? ¿Qué pasos llevan al problema? ¿Qué esperabas que pasara?</p>
            </div>
          </div>
          <div class="tip">
            <i class="pi pi-tag" />
            <div>
              <strong>Categoría adecuada</strong>
              <p><em>Problema</em> si algo no funciona. <em>Consulta</em> si tienes una pregunta. <em>Solicitud de ayuda</em> para apoyo puntual. <em>Sugerencia / mejora</em> para propuestas.</p>
            </div>
          </div>
          <div class="tip">
            <i class="pi pi-info-circle" />
            <div>
              <strong>Estado inicial</strong>
              <p>Toda solicitud nace en <strong>Recibida</strong>. El equipo de Tecnología la tomará desde ahí.</p>
            </div>
          </div>
        </div>
      </aside>
    </div>
  </div>
</template>

<style scoped>
.crear-header { margin-bottom: 28px; }
.crear-header p { margin: 0; color: #64748b; }

.crear-layout {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 300px;
  gap: 22px;
  align-items: start;
}

.crear-form { padding: 28px 32px; }

.campo-titulo { margin-bottom: 4px; }
.campo-titulo label { font-size: 13px; font-weight: 700; margin-bottom: 8px; }
.input-titulo { width: 100%; font-size: 17px !important; }

.campo-descripcion { margin-top: 26px; }
.campo-descripcion label { font-size: 13px; font-weight: 700; margin-bottom: 4px; }
.hint-desc { display: block; margin-bottom: 8px; line-height: 1.5; }
.textarea-principal { width: 100%; font-size: 14px !important; line-height: 1.7 !important; resize: vertical; }

.contador { display: block; text-align: right; margin-top: 5px; }
.contador.limite { color: #dc2626; }

.campos-secundarios {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 0 24px;
  margin-top: 26px;
}
.select-categoria { width: 100%; }

.adjuntos-campo label { font-size: 12px; font-weight: 600; margin-bottom: 7px; display: block; }
.adjuntos-drop {
  display: flex; flex-direction: column; align-items: center; gap: 5px;
  padding: 16px; border: 1.5px dashed #93c5fd; border-radius: 8px;
  color: #3b82f6; font-size: 13px; background: #f0f9ff; cursor: pointer;
  transition: background .15s, border-color .15s; text-align: center;
}
.adjuntos-drop:hover { background: #dbeafe; border-color: #3b82f6; }
.adjuntos-drop i { font-size: 18px; margin-bottom: 2px; }
.adjuntos-drop small { color: #64748b; font-size: 11px; }
.file-hidden { display: none; }
.archivos-lista { list-style: none; margin: 8px 0 0; padding: 0; display: flex; flex-direction: column; gap: 5px; }
.archivo-chip {
  display: flex; align-items: center; gap: 8px;
  padding: 7px 10px; background: #f8fafc; border: 1px solid #e2e8f0;
  border-radius: 6px; font-size: 12px;
}
.archivo-chip i { color: #3b82f6; font-size: 13px; }
.archivo-chip span { flex: 1; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.archivo-chip small { color: #94a3b8; white-space: nowrap; }
.quitar-btn { border: 0; background: none; color: #94a3b8; cursor: pointer; padding: 2px; line-height: 1; }
.quitar-btn:hover { color: #ef4444; }

.solicitante-demo {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 26px;
  padding: 13px 16px;
  background: #f0f9ff;
  border: 1px solid #bae6fd;
  border-radius: 8px;
  font-size: 13px;
}
.solicitante-demo i { color: #0369a1; font-size: 18px; flex-shrink: 0; }
.solicitante-demo strong { display: block; color: #0c4a6e; }
.solicitante-demo small { color: #0369a1; margin-top: 2px; }

.requerido { color: #dc2626; margin-left: 2px; }

/* Panel de orientación */
.orientacion { overflow: hidden; }
.orientacion .panel-heading { padding: 18px 20px; }
.orientacion-body { padding: 18px 20px; display: flex; flex-direction: column; gap: 20px; }
.tip { display: flex; gap: 12px; align-items: flex-start; }
.tip i { color: #3b82f6; font-size: 16px; margin-top: 2px; flex-shrink: 0; }
.tip strong { display: block; font-size: 13px; margin-bottom: 4px; }
.tip p { margin: 0; font-size: 12px; color: #64748b; line-height: 1.6; }

@media (max-width: 900px) {
  .crear-layout { grid-template-columns: 1fr; }
  .orientacion { display: none; }
}
@media (max-width: 600px) {
  .crear-form { padding: 20px; }
  .campos-secundarios { grid-template-columns: 1fr; }
}
</style>
