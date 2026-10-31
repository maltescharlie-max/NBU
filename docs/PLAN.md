# Plan inicial del proyecto NBU

## Identidad

- **Nombre:** NBU — Nicaragua Becas Universitarias
- **Eslogan:** Tu esfuerzo abre oportunidades
- **Colores de referencia:** azul marino, turquesa y dorado, según el logo proporcionado.

## Usuarios y pantallas

### Estudiante

- Inicio de sesión
- Inicio con resumen de beca, próximo pago, noticias y eventos
- Becas disponibles y mis solicitudes
- Mis pagos e historial de depósitos
- Noticias y eventos
- Notificaciones
- Mi perfil
- Cambio de contraseña

### Personal encargado

- Panel principal
- Administración de estudiantes
- Administración de becas y revisión de solicitudes
- Registro y consulta de pagos
- Administración de noticias y eventos
- Revisión de notificaciones
- Acceso administrativo en Windows Forms

La interfaz web debe adaptarse a pantallas pequeñas. En celular, priorizar navegación sencilla, tarjetas legibles y controles cómodos de tocar. La aplicación Windows Forms estará enfocada en las tareas de administración.

El inicio de sesión separa los roles `Admin` y `Student`. Los módulos de administración exigen el rol de administración; el portal limita solicitudes, pagos y notificaciones a la persona autenticada. Las cuentas ficticias de muestra se crean solo en Desarrollo.

## Módulos funcionales

1. **Estudiantes**
2. **Becas y solicitudes**
3. **Pagos de becas**
4. **Noticias y eventos**, con notificaciones

Cada módulo implementará listar (Index), crear (Create), editar (Edit) y eliminar (Delete), de acuerdo con la guía académica.

## Modelo de datos inicial

- **Estudiantes:** código, nombres, apellidos, cédula, correo, teléfono, carrera, universidad y estado.
- **Becas:** nombre, descripción, requisitos, monto, fechas de inicio y cierre, estado.
- **Solicitudes:** estudiante, beca, fecha, estado y observaciones.
- **Pagos:** solicitud beneficiada, monto, fecha del depósito, período y estado.
- **Publicaciones:** tipo (noticia, evento o aviso), título, contenido, fecha de publicación y fecha del evento.
- **Notificaciones:** estudiante destinatario, mensaje, fecha y estado de lectura.

### Relaciones

- Un estudiante puede enviar varias solicitudes.
- Una beca puede recibir varias solicitudes.
- Una solicitud aceptada puede tener varios pagos.
- Una publicación puede generar notificaciones para varios estudiantes.

El personal registra o actualiza pagos; el estudiante solo los consulta.

La pantalla estudiantil presenta “Transferencias bancarias” como una opción deshabilitada. No está conectada a servicios financieros y no envía ni recibe dinero.

## Reparto de trabajo sugerido

Confirmar los responsables con el equipo antes de empezar a editar archivos compartidos.

- **Integrante A:** aplicación web ASP.NET Core MVC, navegación adaptable y estilos Bootstrap.
- **Integrante B:** aplicación Windows Forms y pantallas administrativas.
- **Trabajo conjunto:** esquema SQL Server, entidades y DbContext compartidos, integración, revisión y entregables.

Cada integrante debe trabajar en una rama propia, realizar cambios pequeños y abrir un pull request para integrar su trabajo a `main`. No compartir contraseñas; cada equipo configura su conexión local a SQL Server.

## Entregables de la guía

- Código fuente web y de escritorio en GitHub
- Script SQL de la base de datos
- Documentación técnica
- Manual de usuario
- Presentación
- Ejecutable de escritorio

## Equipo y ficha del proyecto

- **Proyecto:** NBU — Nicaragua Becas Universitarias
- **Grupo:** INS-1111
- **Integrantes:** Carlos Jose Maltes Martinez, Verno Jose Chaves Lopez, Dereck Sahid Davila Duarte e Ismael Exequiel Noguera Arias.
- **Docente:** Ing. Carlos Gaitán.
- **Fecha de inicio registrada:** 28/10/2026.
- **Fecha de entrega:** por definir.
