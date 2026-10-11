namespace Tecnologia.Domain;
public static class TiposNotificacion {
 public const string TecnologiaRespondio = "TecnologiaRespondio";
 public const string NecesitamosInformacion = "NecesitamosInformacion";
 public const string SolicitudResuelta = "SolicitudResuelta";
 public const string SolicitudRechazada = "SolicitudRechazada";
 public const string UsuarioRespondioAResponsable = "UsuarioRespondioAResponsable";
 public static readonly string[] Todos = [TecnologiaRespondio,NecesitamosInformacion,SolicitudResuelta,SolicitudRechazada,UsuarioRespondioAResponsable];
}
