# Fuentes y desarrollo

| Ruta | Contenido | Estado |
|---|---|---|
| src/Styles | Policy.cs y Visuals.cs | Fuentes editables del módulo usado en la 2.12. |
| src/WindowsApps | ModernApps.cs | Fuente editable del módulo de la 2.12. |
| tests | Pruebas de reglas de estilos y apps | Ejecutadas contra los módulos en el entorno de preparación. |
| src/native | C del lanzador | Fuente disponible; la construcción original requiere generar imports/recursos. |
| reference/core-recovered | Recuperación del núcleo 2.12 con ILSpy | Consulta, no reconstrucción validada. |
| reference/wrapper-recovered | Recuperación del supervisor de icono/aviso | Consulta, no reconstrucción validada. |

## Compilar módulos en Windows

Requiere SDK .NET 8 para desarrollo. El usuario final tiene el runtime incluido.

```powershell
.\scripts\Build-Modules.ps1
```

El script compila los módulos y ejecuta sus pruebas sin reemplazar la release aprobada. Una futura recompilación se probará en una copia separada y con otra versión.

## Límite del núcleo recuperado

El núcleo y supervisor se entregaron en parte mediante modificación de ensamblados. No se conservó un proyecto fuente integral de origen. ILSpy permite consultarlos, pero puede transformar nombres de cierres, campos y parámetros. El selector usa algunos de esos nombres por reflexión: hay que reconciliarlos antes de reconstruir el núcleo.

El target que infiere el descompilador en sus proyectos de referencia no es la configuración real. La distribución usa .NET 8. Los csproj recuperados se conservan como texto de referencia, fuera del build de módulos.

La primera subida es del binario validado, documentación y fuentes disponibles. No se presenta como una reconstrucción integral reproducible.

Datos: `%APPDATA%\AccessShot\entries.json`. Los metadatos `__AccessShotStyle` conservan la condición de tema y sobrevuelo junto al objeto, también al cambiar su capa.

Las apps modernas guardan `shell:AppsFolder\<AppID>`. URL y GitHub conservan confirmación antes de abrir el navegador. Los números de metadatos internos históricos no se deben «corregir» en el ZIP validado.

## Reconstrucción 2.13

`src/Core` contiene el núcleo recuperado de la 2.12, adaptado y compilado para .NET 8. La 2.13 incorpora una disposición pública de ejemplo solo cuando aún no existe `entries.json`. La compilación de los módulos y el núcleo, así como la prueba de arranque y guardado en un perfil aislado, están registradas en [VALIDACION_2.13.md](VALIDACION_2.13.md). El lanzador nativo y el supervisor de la 2.12 se conservan.

La reconstrucción requiere el SDK de .NET 8 o un SDK más reciente que pueda restaurar sus paquetes de referencia desde NuGet. Para compilar los tres proyectos, ejecuta `scripts/Build-2.13.ps1`. La publicación portátil usa `dotnet publish` del núcleo para `win-x64` con `--self-contained true` y combina los dos ejecutables de inicio de la 2.12. La 2.12 archivada no se altera.