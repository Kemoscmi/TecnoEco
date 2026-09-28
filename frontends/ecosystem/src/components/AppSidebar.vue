<script setup lang="ts">
import { RouterLink } from 'vue-router'
const links=[{to:'tickets',label:'Tickets',icon:'pi-ticket'},{to:'tablero',label:'Tablero de trabajo',icon:'pi-objects-column'},{to:'bitacora',label:'Bitácora general',icon:'pi-history'}]
</script>
<template><aside class="app-sidebar"><div class="brand"><i class="pi pi-microchip"/> <span>Tecnología<small>ECOSYSTEM</small></span></div><div class="nav-caption">CENTRO DE TRABAJO</div><nav aria-label="Tecnología"><RouterLink v-for="link in links" :key="link.to" :to="'/tecnologia/'+link.to"><i :class="['pi',link.icon]"/><span>{{ link.label }}</span></RouterLink></nav><div class="sidebar-footer">Reportes, avances y soluciones.<br><small>Espacio del equipo interno</small></div></aside></template>
