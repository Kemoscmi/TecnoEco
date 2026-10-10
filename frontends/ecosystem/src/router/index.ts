import { createRouter, createWebHistory } from 'vue-router'
import TecnologiaView from '@/views/tecnologia/TecnologiaView.vue'
import CrearSolicitudView from '@/views/tecnologia/CrearSolicitudView.vue'
import MisSolicitudesView from '@/views/tecnologia/MisSolicitudesView.vue'
import DashboardView from '@/views/tecnologia/DashboardView.vue'
export default createRouter({history:createWebHistory(),routes:[{path:'/',redirect:'/tecnologia/dashboard'},{path:'/tecnologia/dashboard',component:DashboardView},{path:'/tecnologia/:section(tickets|tablero|bitacora)',component:TecnologiaView},{path:'/tecnologia/solicitudes/nueva',component:CrearSolicitudView},{path:'/mis-solicitudes',component:MisSolicitudesView},{path:'/:pathMatch(.*)*',redirect:'/tecnologia/dashboard'}]})
