export const statuses = ['Nuevo','En proceso','En QA','Resuelto'] as const
export const types = ['Bug','Soporte','Historia de usuario','Tarea técnica'] as const
export type Status = typeof statuses[number]
export interface TicketInput { title:string; description:string; type:typeof types[number]; origin:'Cliente'|'Interno'; priority:'Alta'|'Media'|'Baja'; requester:string; assignee:string }
export interface Ticket extends TicketInput { id:string; status:Status; createdAt:string }
export interface Activity { id:string; ticketId:string|null; text:string; visibility:'Nota interna'|'Respuesta al solicitante'; createdAt:string }
export interface Snapshot { tickets:Ticket[]; activities:Activity[] }
export interface ActivityInput { ticketId:string|null; text:string; visibility:Activity['visibility'] }
