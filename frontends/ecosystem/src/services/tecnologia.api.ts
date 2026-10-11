import type { Snapshot, TicketInput, Status, ActivityInput, TomarSolicitudInput, ReasignarInput, ColaboradorInput, ResolverInput, RechazarInput } from '@/types/tecnologia'
export const demoMode = import.meta.env.VITE_DATA_MODE !== 'api'
async function request<T>(path:string,method='GET',body?:unknown):Promise<T>{
 const res=await fetch('/api/tecnologia'+path,{method,headers:{'Content-Type':'application/json'},body:body===undefined?undefined:JSON.stringify(body)})
 if(!res.ok){const error=await res.json().catch(()=>null);throw new Error(error?.detail||'No se pudo completar la operación. Revisa la API local.')}
 return await res.json() as T
}
export const api={
 load:()=>request<Snapshot>(''),
 create:(data:TicketInput)=>request<Snapshot>('/tickets','POST',data),
 changeStatus:(id:string,status:Status)=>request<Snapshot>('/tickets/'+id+'/status','PATCH',{estado:status}),
 addActivity:(data:ActivityInput)=>request<Snapshot>('/activities','POST',data),
 // REQ-005
 tomarSolicitud:(id:string,data:TomarSolicitudInput)=>request<Snapshot>('/tickets/'+id+'/tomar','POST',data),
 reasignar:(id:string,data:ReasignarInput)=>request<Snapshot>('/tickets/'+id+'/responsable','PATCH',data),
 agregarColaborador:(id:string,data:ColaboradorInput)=>request<Snapshot>('/tickets/'+id+'/colaboradores','POST',data),
 // REQ-012
 resolver:(id:string,data:ResolverInput)=>request<Snapshot>('/tickets/'+id+'/resolver','POST',data),
 rechazar:(id:string,data:RechazarInput)=>request<Snapshot>('/tickets/'+id+'/rechazar','POST',data),
}
