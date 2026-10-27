# Validación de AccesShot 2.16

Fecha: 27/10/2026.

## Cambios incluidos

- El aviso de desbordamiento se coloca bajo la barra de herramientas y recalcula su posición cuando cambia el tamaño de ventana o de barra.
- El icono del formulario se obtiene del ejecutable, que incorpora `assets/AccesShot.ico`.
- En perfiles nuevos, AccesShot usa `assets/initial-layout.json` y lo copia a `%APPDATA%\AccessShot\entries.json`. Solo lo hace cuando no existe configuración previa; los perfiles existentes no se sobrescriben.
- El JSON inicial contiene cuatro accesos y metadatos de estilo, y sus dos direcciones HTTP/HTTPS no contienen credenciales de URL ni parámetros de consulta.

## Compilación y paquete

- SDK: .NET 10.0.403; el proyecto apunta a .NET 8 para Windows.
- `AccessShotStyles` y `AccessShotWindowsApps`: compilación correcta, 0 advertencias y 0 errores.
- `AccessShotCore`: compilación correcta, 15 advertencias y 0 errores.
- Publicación autocontenida `win-x64`: completada.
- El ZIP contiene el lanzador portable, la aplicación 2.16, el icono y `Aplicacion/assets/initial-layout.json`.
- El icono extraído del ejecutable compilado se verificó visualmente.

Paquete: `AccessShot-2.16-portable-Windows-x64.zip`  
Tamaño: 72902014 bytes  
SHA-256: `b54f2bab06ed18d005d13bf4042e931624015e9d6cbb284907166293997d54a3`

## Alcance pendiente

No se inició el ejecutable después del cambio de disposición inicial. La selección de valores iniciales se validó por código y compilación, pero aún requiere comprobación de primer inicio. El candidato sigue sin publicarse en GitHub.