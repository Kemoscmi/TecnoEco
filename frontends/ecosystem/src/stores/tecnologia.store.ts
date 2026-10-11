import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api,demoMode } from '@/services/tecnologia.api'
import type { Snapshot,TicketInput,Status,ActivityInput,Activity,TomarSolicitudInput,ReasignarInput,ColaboradorInput,Adjunto,AdjuntoMeta } from '@/types/tecnologia'
const key='tecnologia-ecosystem.demo.req001.v2'
function seed():Snapshot {const now=new Date().toISOString();return {tickets:[
{id:'TEC-DEMO01',title:'Error al guardar una solicitud',description:'Al confirmar la solicitud aparece un error. Revisar el flujo de guardado.',categoria:'Problema',estado:'Recibida',solicitanteId:'demo-cliente-01',responsableId:null,colaboradores:[],createdAt:now},
{id:'TEC-DEMO02',title:'Consultar reportes por cliente',description:'Como responsable de soporte quiero filtrar reportes por cliente para dar seguimiento. Criterio: mostrar únicamente los reportes del cliente seleccionado.',categoria:'Problema',estado:'Recibida',solicitanteId:'demo-cliente-01',responsableId:null,colaboradores:[],createdAt:now},
{id:'TEC-DEMO03',title:'Validar la recuperación de acceso',description:'Verificar el flujo de recuperación de contraseña y documentar los resultados.',categoria:'Problema',estado:'Recibida',solicitanteId:'demo-cliente-01',responsableId:null,colaboradores:[],createdAt:now}
],activities:[{id:'demo-activity',ticketId:'TEC-DEMO01',text:'Se reprodujo el error. Desarrollo inició la revisión.',visibility:'Nota interna',createdAt:now},{id:'demo-maintenance',ticketId:null,text:'Revisión preventiva de servicios completada.',visibility:'Nota interna',createdAt:now}],adjuntos:[]}}
export const useTecnologiaStore=defineStore('tecnologia',()=>{
 const data=ref<Snapshot>({tickets:[],activities:[],adjuntos:[]});const loading=ref(false);const ready=ref(false);const error=ref('')
 async function load(){loading.value=true;error.value='';ready.value=false;try{if(demoMode){const saved=localStorage.getItem(key);const parsed=saved?JSON.parse(saved):seed();if(!Array.isArray(parsed.tickets)||!Array.isArray(parsed.activities))throw new Error('Los datos locales no son válidos.');if(!Array.isArray(parsed.adjuntos))parsed.adjuntos=[];data.value=parsed}else data.value=await api.load();ready.value=true}catch(e){error.value=e instanceof Error?e.message:'Error al cargar datos'}finally{loading.value=false}}
 async function mutate(local:(draft:Snapshot)=>void,remote:()=>Promise<Snapshot>){if(!ready.value)throw new Error('Carga los datos antes de continuar.');if(demoMode){const draft=structuredClone(JSON.parse(JSON.stringify(data.value))) as Snapshot;if(!Array.isArray(draft.adjuntos))draft.adjuntos=[];local(draft);localStorage.setItem(key,JSON.stringify(draft));data.value=draft}else data.value=await remote()}
 function activity(input:ActivityInput):Activity{return {...input,id:crypto.randomUUID(),createdAt:new Date().toISOString()}}
 function newAdjunto(ticketId:string,activityId:string|null,meta:AdjuntoMeta,visibility:'publica'|'interna'):Adjunto{return{id:crypto.randomUUID(),ticketId,activityId,...meta,visibility,url:null,createdAt:new Date().toISOString()}}
 async function create(input:TicketInput){const id='TEC-'+crypto.randomUUID().replaceAll('-','').toUpperCase();await mutate(d=>{d.tickets.unshift({...input,id,responsableId:null,estado:'Recibida',colaboradores:[],createdAt:new Date().toISOString()});d.activities.unshift(activity({ticketId:id,text:'Ticket creado. Pendiente de revisión.',visibility:'Nota interna'}));for(const meta of(input.adjuntos??[]))d.adjuntos.push(newAdjunto(id,null,meta,'publica'))},()=>api.create(input))}
 async function changeStatus(id:string,status:Status){await mutate(d=>{const ticket=d.tickets.find(t=>t.id===id);if(!ticket)throw new Error('Ticket no encontrado');const before=ticket.estado;if(before===status)return;ticket.estado=status;d.activities.unshift(activity({ticketId:id,text:'Estado actualizado: '+before+' → '+status,visibility:'Nota interna'}))},()=>api.changeStatus(id,status))}
 async function addActivity(input:ActivityInput){const act=activity(input);await mutate(d=>{d.activities.unshift(act);if(input.ticketId){const vis=input.visibility==='Respuesta al solicitante'?'publica':'interna';for(const meta of(input.adjuntos??[]))d.adjuntos.push(newAdjunto(input.ticketId,act.id,meta,vis))}},()=>api.addActivity(input))}
 // REQ-005
 async function tomarSolicitud(id:string,input:TomarSolicitudInput){
  await mutate(d=>{
   const t=d.tickets.find(t=>t.id===id);if(!t)throw new Error('Ticket no encontrado');
   t.responsableId=input.actorId;
   if(t.estado==='Recibida'){t.estado='En proceso';d.activities.unshift(activity({ticketId:id,text:`${input.actorId} tomó la solicitud. Estado: Recibida → En proceso.`,visibility:'Nota interna'}));}
   else{d.activities.unshift(activity({ticketId:id,text:`${input.actorId} tomó la solicitud.`,visibility:'Nota interna'}));}
  },()=>api.tomarSolicitud(id,input))
 }
 async function reasignar(id:string,input:ReasignarInput){
  await mutate(d=>{
   const t=d.tickets.find(t=>t.id===id);if(!t)throw new Error('Ticket no encontrado');
   const anterior=t.responsableId??'(sin responsable)';
   t.responsableId=input.nuevoResponsableId;
   d.activities.unshift(activity({ticketId:id,text:`Reasignada por ${input.actorId}. Responsable: ${anterior} → ${input.nuevoResponsableId}.`,visibility:'Nota interna'}));
  },()=>api.reasignar(id,input))
 }
 async function agregarColaborador(id:string,input:ColaboradorInput){
  await mutate(d=>{
   const t=d.tickets.find(t=>t.id===id);if(!t)throw new Error('Ticket no encontrado');
   if(t.colaboradores.some(c=>c.colaboradorId===input.colaboradorId))throw new Error('El colaborador ya está asociado a esta solicitud.');
   t.colaboradores.push({ticketId:id,colaboradorId:input.colaboradorId});
   d.activities.unshift(activity({ticketId:id,text:`Colaborador agregado: ${input.colaboradorId}.`,visibility:'Nota interna'}));
  },()=>api.agregarColaborador(id,input))
 }
 return {data,loading,ready,error,load,create,changeStatus,addActivity,tomarSolicitud,reasignar,agregarColaborador}
})
