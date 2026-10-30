# Demostración de NBU según la ficha de evaluación

## Identificación

- Categoría: Becas universitarias.
- Nombre oficial: NBU — Nicaragua Becas Universitarias.
- Lenguaje: C# 12.
- Web: ASP.NET Core MVC con Bootstrap (.NET 8).
- Escritorio: Windows Forms (.NET 8).
- Datos: Entity Framework Core 8 y SQL Server.
- Base configurada: `NBU`, en la instancia `SQLEXPRESS22`.

## Qué mostrar en la defensa

1. Abrir `NBU.sln` y ubicar `NBU.Web`, `NBU.Desktop` y `NBU.Data`.
2. En SSMS, conectarse con Autenticación de Windows al servidor ` .\SQLEXPRESS22` (sin el espacio inicial) y mostrar la base `NBU` y sus tablas.
3. Ejecutar la aplicación de escritorio e iniciar sesión como administrador.
4. Crear o editar un registro en un módulo administrativo, por ejemplo una beca o una noticia.
5. Abrir el sitio web e ir al módulo correspondiente para mostrar el mismo registro.
6. Si se modifica desde la web, volver al escritorio y comprobar que también aparece allí.
7. Explicar con palabras propias qué hace el código de conexión, dónde están los modelos y qué métodos realizan las operaciones.

## Archivos que pide la ficha

- Solución y código fuente: `NBU.sln`, `NBU.Web`, `NBU.Desktop` y `NBU.Data`.
- Script de base de datos: `database/NBU.sql`.
- Manual, documentación técnica, ficha y presentación: carpeta `entregables`.
- Ejecutable Windows: `entregables/escritorio/NBU.Desktop.exe` (se genera/publica localmente y puede no estar incluido en GitHub por su tamaño).

## Antes de la evaluación

- Confirmar en ambas configuraciones que la conexión apunte a la misma instancia y base `NBU`.
- Tener SQL Server encendido antes de iniciar las aplicaciones.
- Preparar un registro de prueba que se pueda crear o editar sin afectar datos reales.
- No afirmar que se procesan transferencias bancarias: el sistema solo registra depósitos realizados externamente.
- Marcar en la ficha únicamente las funciones que se hayan demostrado durante la evaluación.

La prueba decisiva de integración es hacer un cambio desde una aplicación y consultarlo desde la otra. Las capturas y los documentos ayudan, pero no sustituyen esa demostración.
