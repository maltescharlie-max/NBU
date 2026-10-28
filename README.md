# NBU — Nicaragua Becas Universitarias

**Eslogan:** Tu esfuerzo abre oportunidades

Plataforma académica para gestionar becas universitarias y mantener informados a estudiantes beneficiarios. El proyecto incluye una aplicación web adaptable a celulares y una aplicación de escritorio para el personal encargado.

## Alcance acordado

La aplicación tendrá cuatro módulos, cada uno con operaciones para listar, crear, editar y eliminar:

1. **Estudiantes:** datos y estado de los estudiantes beneficiarios.
2. **Becas y solicitudes:** becas disponibles, solicitudes y su estado de revisión.
3. **Pagos de becas:** registro y consulta de depósitos, períodos y estados de pago.
4. **Noticias y eventos:** publicaciones universitarias y avisos para estudiantes.

Los estudiantes podrán consultar sus solicitudes, pagos, noticias, eventos y notificaciones. El personal encargado administrará los registros. Los pagos se registrarán en el sistema; no se conectará a un banco para realizar transferencias.

## Tecnologías requeridas

- C# 12 y .NET 8
- ASP.NET Core MVC para la aplicación web
- Windows Forms para la aplicación de escritorio
- Entity Framework Core 8
- SQL Server compartido por ambas aplicaciones
- Bootstrap para el diseño web adaptable
- Git y GitHub para el control de versiones

## Documentos

- [Plan del proyecto y reparto inicial](docs/PLAN.md)
- [Script SQL Server](database/NBU.sql)

## Estado

Panel web adaptable y los cuatro módulos de administración implementados: estudiantes, becas y solicitudes, pagos, y noticias y eventos. Las publicaciones pueden generar notificaciones en la plataforma para estudiantes activos. Los pagos solo se asocian a solicitudes aceptadas. El inicio de sesión y las vistas de Windows Forms siguen pendientes.

## Ejecutar localmente

Se requiere el SDK de .NET 8 y SQL Server LocalDB (o una instancia SQL Server configurada). La cadena de conexión de desarrollo está en `NBU.Web/appsettings.json`; ajústala a la instancia disponible en cada computadora. No uses credenciales reales en archivos versionados.

Abre `NBU.sln` en Visual Studio o ejecuta `dotnet run --project NBU.Web` desde la raíz del repositorio.
