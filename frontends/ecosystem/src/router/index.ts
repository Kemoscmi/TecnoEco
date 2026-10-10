import { createRouter, createWebHistory } from 'vue-router'
import TecnologiaView from '@/views/tecnologia/TecnologiaView.vue'
import CrearSolicitudView from '@/views/tecnologia/CrearSolicitudView.vue'
export default createRouter({history:createWebHistory(),routes:[{path:'/',redirect:'/tecnologia/tickets'},{path:'/tecnologia/:section(tickets|tablero|bitacora)',component:TecnologiaView},{path:'/tecnologia/solicitudes/nueva',component:CrearSolicitudView},{path:'/:pathMatch(.*)*',redirect:'/tecnologia/tickets'}]})
