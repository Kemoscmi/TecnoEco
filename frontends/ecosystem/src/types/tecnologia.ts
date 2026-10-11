// Catálogo inicial; podrá evolucionar junto con el contrato del backend.
export const categorias = ['Problema','Consulta','Solicitud de ayuda','Sugerencia / mejora'] as const
export const statuses = ['Recibida','En proceso','Necesitamos información','Resuelta','Rechazada'] as const
export type Status = typeof statuses[number]
export const tiposTrabajoTecnico = ['Bug','Historia de usuario','Tarea técnica','Mejora'] as const
export type TipoTrabajoTecnico = typeof tiposTrabajoTecnico[number]
export interface TicketColaborador { ticketId:string; colaboradorId:string }
export interface TrabajoTecnicoRelacionado { ticketId:string; trabajoTecnicoId:string; tipoTrabajoTecnico:TipoTrabajoTecnico; relacionadoPorId:string; createdAt:string }
export interface Ticket { id:string; title:string; description:string; categoria:string; solicitanteId:string; responsableId:string|null; estado:Status; abandonoRecibidaAt:string|null; eliminadoAt:string|null; eliminadoPorId:string|null; createdAt:string; colaboradores:TicketColaborador[]; trabajosTecnicos:TrabajoTecnicoRelacionado[] }
export interface Activity { id:string; ticketId:string|null; text:string; visibility:'Nota interna'|'Respuesta al solicitante'; createdAt:string }
// REQ-010: metadatos de un archivo adjunto (sin URL hasta definir storage)
export interface AdjuntoMeta { nombre:string; tipo:string; tamaño:number }
export interface Adjunto extends AdjuntoMeta { id:string; ticketId:string; activityId:string|null; visibility:'publica'|'interna'; url:string|null; createdAt:string }
export interface Snapshot { tickets:Ticket[]; activities:Activity[]; adjuntos:Adjunto[] }
// Inputs
export interface TicketInput { title:string; description:string; categoria:string; solicitanteId:string; adjuntos?:AdjuntoMeta[] }
export interface ActivityInput { ticketId:string|null; text:string; visibility:Activity['visibility']; adjuntos?:AdjuntoMeta[] }
// REQ-005
export interface TomarSolicitudInput { actorId:string }
export interface ReasignarInput { actorId:string; nuevoResponsableId:string }
export interface ColaboradorInput { colaboradorId:string }
// REQ-012
export interface ResolverInput { mensaje:string }
export interface RechazarInput { motivo:string }
// REQ-013
export interface EditarSolicitudInput { title:string; description:string; categoria:string; actorId:string }
export interface EliminarSolicitudInput { actorId:string }
export interface RelacionarTrabajoTecnicoInput { trabajoTecnicoId:string; tipoTrabajoTecnico:TipoTrabajoTecnico; actorId:string }
