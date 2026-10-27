# Validación de AccesShot 2.15

Estado al 27/10/2026: fuentes y README integrados en `main`; tag `v2.15` publicado como **Latest** con el ZIP portable y su suma SHA-256.

## Comprobación realizada

- Compilación Release de los tres proyectos: 0 errores. Una compilación no incremental del núcleo reportó 15 advertencias; la última compilación incremental terminó sin advertencias.
- ZIP generado desde el paquete 2.14 aprobado (SHA-256 base `80631d5e79dc568dc66416a6dedb9e6febca715f91a62dbcc25e22727e59c24f`).
- El ZIP incluye los dos lanzadores, la carpeta `Aplicacion` con runtime `win-x64`, las licencias y `LEEME.txt`; no incluye `entries.json` ni datos de usuario.
- El lanzador de la raíz apunta a `Aplicacion\\AccessShot.exe`; ese archivo coincide byte por byte con el anfitrión compilado de la 2.15 y está enlazado a `AccessShotCore.dll`. Se retiró `AccessShot.dll` heredado para evitar iniciar código anterior.
- ZIP publicado: `AccessShot-2.15-portable-Windows-x64.zip`; tamaño: 72,526,745 bytes; SHA-256: `517dcdfdab0a1a960c0b1b9c83c1d57f216e1f4aa1955bdb422ba8e1ecf1b4ed`.
- `README.md` y `README.en.md` de `main` muestran 2.15 y enlazan a `releases/tag/v2.15`.
- La release `v2.15` apunta al commit integrado en `main`; GitHub muestra el estado **Latest** y contiene el ZIP y `SHA256SUMS_v2.15.txt`.

## Validación funcional pendiente

- Revisión visual y funcional en Windows 11: lienzo, orientación, selección de grupos, deshacer, barra inferior y confirmación de borrado.
- Verificar migración de JSON 2.12–2.14, estilos, capas y copia `.pre-2.15.bak`.
- Verificar deshacer tras reiniciar y tras cambios externos del JSON.
- Verificar creación y restauración de copias con espacios, tildes, colisiones y fallos de escritura.
- Verificar interacción con el selector de aplicaciones modernas de Windows.