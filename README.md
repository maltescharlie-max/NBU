# NBU — Nicaragua Becas Universitarias

**Eslogan:** Tu esfuerzo abre oportunidades

Plataforma académica para gestionar becas universitarias y mantener informados a los estudiantes. Incluye una aplicación web adaptable a celulares y una aplicación Windows Forms para el personal.

## Módulos

1. **Estudiantes:** datos y estado de estudiantes beneficiarios.
2. **Becas y solicitudes:** convocatorias, solicitudes y estado de revisión.
3. **Pagos de becas:** registro y consulta de depósitos, períodos y estados.
4. **Noticias y eventos:** publicaciones universitarias, avisos y notificaciones.

Cada módulo administrativo implementa listar, crear, editar y eliminar. El portal de estudiante permite consultar convocatorias, enviar una solicitud, revisar su historial, ver pagos, noticias, eventos y notificaciones propias.

## Tecnologías

- C# 12 y .NET 8
- ASP.NET Core MVC y Bootstrap para la aplicación web
- Windows Forms para la aplicación de escritorio
- Entity Framework Core 8 y SQL Server
- Git y GitHub

## Estado y pagos

El sitio web usa los roles **Admin** y **Student**. Los módulos administrativos y Windows Forms requieren una cuenta administradora. El portal de estudiante filtra solicitudes, pagos y notificaciones según la cuenta iniciada.

El personal registra depósitos que ya fueron realizados por fuera del sistema. La opción “Transferencias bancarias” del portal estudiantil aparece deshabilitada y solo como referencia; no se conecta a ningún banco y no mueve dinero.

## Documentación

- [Ficha técnica y plan del proyecto](docs/PLAN.md)
- [Script SQL Server idempotente](database/NBU.sql)
- [Documentación técnica](entregables/Documentacion-Tecnica-NBU.docx)
- [Documentación técnica en PDF](entregables/Documentacion-Tecnica-NBU.pdf)
- [Ficha técnica en Word](entregables/Ficha-Tecnica-NBU.docx)
- [Ficha técnica en PDF](entregables/Ficha-Tecnica-NBU.pdf)
- [Manual de usuario](entregables/Manual-de-Usuario-NBU.docx)
- [Manual de usuario en PDF](entregables/Manual-de-Usuario-NBU.pdf)
- [Presentación](entregables/Presentacion-NBU.pptx)

## Ejecutar localmente

Se requiere el SDK de .NET 8 y SQL Server Express o LocalDB. La configuración incluida apunta a la instancia `SQLEXPRESS22`; si tu instancia tiene otro nombre, actualiza `NBU.Web/appsettings.json` y `NBU.Desktop/appsettings.json`. Windows Forms también lee la variable de entorno `NBU_SQL_CONNECTION`.

1. Inicia la instancia de SQL Server configurada.
2. Desde la raíz del repositorio, ejecuta `dotnet run --project NBU.Web` en modo Desarrollo. El portal aplica las migraciones y agrega datos ficticios de demostración.
3. Abre la dirección local que aparezca en la terminal.
4. Ejecuta `dotnet run --project NBU.Desktop` para abrir la aplicación de escritorio.

Para generar el ejecutable de Windows x64, ejecuta en PowerShell desde la raíz del repositorio:

```powershell
dotnet publish NBU.Desktop\NBU.Desktop.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o entregables\escritorio
```

El archivo `entregables/escritorio/NBU.Desktop.exe` incluye el runtime de .NET. La carpeta de publicación local se excluye del repositorio porque el ejecutable autocontenido es de gran tamaño; los documentos, el código fuente y la presentación sí se versionan.

Las cuentas de muestra se crean únicamente en Desarrollo:

| Perfil | Correo | Contraseña inicial |
| --- | --- | --- |
| Administración | `admin@nbu.test` | `NbuAdmin2026!` |
| Estudiante de ejemplo | `ana.mendez@example.test` | `NbuEstudiante2026!` |

Los datos son ficticios para demostración. Después del primer ingreso se puede cambiar la contraseña desde la cuenta. No uses estas contraseñas en un despliegue real.

Para instalar el esquema con SSMS, selecciona la base `NBU` y ejecuta `database/NBU.sql`. La aplicación web también aplica las migraciones automáticamente cuando el entorno es Desarrollo.
