# Validación de AccesShot 3.0

Fecha: 08/11/2026.

## Compilación y paquete

- SDK .NET: 10.0.403.
- Base portable 2.16 verificada antes de compilar; SHA-256: `b54f2bab06ed18d005d13bf4042e931624015e9d6cbb284907166293997d54a3`.
- `AccessShotStyles`, `AccessShotWindowsApps` y `AccessShotCore`: compilación Release correcta, sin errores.
- Publicación autocontenida `win-x64`: completada con `scripts/Build-3.0.ps1`.
- ZIP inspeccionado: 478 entradas; incluye lanzador, aplicación, runtime, icono, disposición inicial, instrucciones y licencias. No incluye `entries.json` ni datos de usuario.

Paquete: `AccessShot-3.0-portable-Windows-x64.zip`
Tamaño: 72534360 bytes
SHA-256: `ad6c87ad4bbe83d528cba54048680aa9861a9df0171db08c393ff7ce03cd0049`

La compilación, estructura del ZIP y checksum se verificaron para esta entrega. No se ejecutó la suite de pruebas automatizadas durante la publicación.
