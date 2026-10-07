import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api,demoMode } from '@/services/tecnologia.api'
import type { Snapshot,TicketInput,Status,ActivityInput,Activity } from '@/types/tecnologia'
const key='tecnologia-ecosystem.demo.req001.v2'
function seed():Snapshot {const now=new Date().toISOString();return {tickets:[
{id:'TEC-DEMO01',title:'Error al guardar una solicitud',description:'Al confirmar la solicitud aparece un error. Revisar el flujo de guardado.',categoria:'Problema',estado:'Recibida',solicitanteId:'demo-cliente-01',responsableId:null,colaboradores:[],createdAt:now},
{id:'TEC-DEMO02',title:'Consultar reportes por cliente',description:'Como responsable de soporte quiero filtrar reportes por cliente para dar seguimiento. Criterio: mostrar únicamente los reportes del cliente seleccionado.',categoria:'Problema',estado:'Recibida',solicitanteId:'demo-cliente-01',responsableId:null,colaboradores:[],createdAt:now},
{id:'TEC-DEMO03',title:'Validar la recuperación de acceso',description:'Verificar el flujo de recuperación de contraseña y documentar los resultados.',categoria:'Problema',estado:'Recibida',solicitanteId:'demo-cliente-01',responsableId:null,colaboradores:[],createdAt:now}
],activities:[{id:'demo-activity',ticketId:'TEC-DEMO01',text:'Se reprodujo el error. Desarrollo inició la revisión.',visibility:'Nota interna',createdAt:now},{id:'demo-maintenance',ticketId:null,text:'Revisión preventiva de servicios completada.',visibility:'Nota interna',createdAt:now}]}}
export const useTecnologiaStore=defineStore('tecnologia',()=>{
 const data=ref<Snapshot>({tickets:[],activities:[]});const loading=ref(false);const ready=ref(false);const error=ref('')
 async function load(){loading.value=true;error.value='';ready.value=false;try{if(demoMode){const saved=localStorage.getItem(key);const parsed=saved?JSON.parse(saved):seed();if(!Array.isArray(parsed.tickets)||!Array.isArray(parsed.activities))throw new Error('Los datos locales no son válidos.');data.value=parsed}else data.value=await api.load();ready.value=true}catch(e){error.value=e instanceof Error?e.message:'Error al cargar datos'}finally{loading.value=false}}
 async function mutate(local:(draft:Snapshot)=>void,remote:()=>Promise<Snapshot>){if(!ready.value)throw new Error('Carga los datos antes de continuar.');if(demoMode){const draft=structuredClone(JSON.parse(JSON.stringify(data.value))) as Snapshot;local(draft);localStorage.setItem(key,JSON.stringify(draft));data.value=draft}else data.value=await remote()}
 function activity(input:ActivityInput):Activity{return {...input,id:crypto.randomUUID(),createdAt:new Date().toISOString()}}
 async function create(input:TicketInput){const id='TEC-'+crypto.randomUUID().replaceAll('-','').toUpperCase();await mutate(d=>{d.tickets.unshift({...input,id,estado:'Recibida',colaboradores:[],createdAt:new Date().toISOString()});d.activities.unshift(activity({ticketId:id,text:'Ticket creado. Pendiente de revisión.',visibility:'Nota interna'}))},()=>api.create(input))}
 async function changeStatus(id:string,status:Status){await mutate(d=>{const ticket=d.tickets.find(t=>t.id===id);if(!ticket)throw new Error('Ticket no encontrado');const before=ticket.estado;if(before===status)return;ticket.estado=status;d.activities.unshift(activity({ticketId:id,text:'Estado actualizado: '+before+' → '+status,visibility:'Nota interna'}))},()=>api.changeStatus(id,status))}
 async function addActivity(input:ActivityInput){await mutate(d=>d.activities.unshift(activity(input)),()=>api.addActivity(input))}
 return {data,loading,ready,error,load,create,changeStatus,addActivity}
})
