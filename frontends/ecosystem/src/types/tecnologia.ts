// Catálogo inicial; podrá evolucionar junto con el contrato del backend.
export const categorias = ['Problema','Consulta','Solicitud de ayuda','Sugerencia / mejora'] as const
export const statuses = ['Recibida','En proceso','Necesitamos información','Resuelta','Rechazada'] as const
export type Status = typeof statuses[number]
export interface TicketInput { title:string; description:string; categoria:string; solicitanteId:string; responsableId:string|null }
export interface TicketColaborador { ticketId:string; colaboradorId:string }
export interface Ticket extends TicketInput { id:string; estado:Status; createdAt:string; colaboradores:TicketColaborador[] }
export interface Activity { id:string; ticketId:string|null; text:string; visibility:'Nota interna'|'Respuesta al solicitante'; createdAt:string }
export interface Snapshot { tickets:Ticket[]; activities:Activity[] }
export interface ActivityInput { ticketId:string|null; text:string; visibility:Activity['visibility'] }
