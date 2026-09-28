import { createRouter, createWebHistory } from 'vue-router'
import TecnologiaView from '@/views/tecnologia/TecnologiaView.vue'
export default createRouter({history:createWebHistory(),routes:[{path:'/',redirect:'/tecnologia/tickets'},{path:'/tecnologia/:section(tickets|tablero|bitacora)',component:TecnologiaView},{path:'/:pathMatch(.*)*',redirect:'/tecnologia/tickets'}]})
