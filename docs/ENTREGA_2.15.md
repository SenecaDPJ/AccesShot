# Entrega de AccesShot 2.15 a GitHub

## Hallazgo que corrige este flujo

GitHub muestra `v2.14` como última release, pero el README que se ve desde `main` conserva un enlace de descarga a `v2.13`. La etiqueta y la release 2.14 existen, y `main` está dos commits por delante de la etiqueta; esos dos commits solo modifican `README.md` y `README.en.md`. El código de 2.14 está en el ancestro de `main`; lo que quedó desactualizado fue la ruta de descarga de la página principal.

## Orden obligatorio para 2.15

1. Revisar y compilar todas las fuentes y los documentos de esta versión.
2. Generar el ZIP local desde la release 2.14 verificada. Revisar que incluye la compilación 2.15, los ejecutables de inicio y la carpeta `Aplicacion`.
3. Integrar en la rama `main` el código, README en español e inglés, notas y validación 2.15. La integración debe ser un avance normal; no forzar la rama ni reemplazar el historial.
4. Abrir `README.md` y `README.en.md` desde `main` y confirmar que ambos muestran 2.15 y enlazan a `releases/tag/v2.15`.
5. Crear el tag `v2.15` apuntando al commit que ya está en `main` y publicar el ZIP con su SHA-256.
6. Marcar la release 2.15 como **Latest** y comprobar desde la página del repositorio, la página Releases y ambos README que la descarga conduce a la 2.15.
7. Descargar el asset publicado y verificar su SHA-256 frente al archivo local.

No crear la release antes de integrar la rama. No dar por completa la subida si solo existe el tag o si el README principal sigue enlazando a 2.13/2.14.

## Cómo compilar y empaquetar

`scripts/Build-2.15.ps1` requiere el ZIP portable exacto de la release 2.14, cuyo SHA-256 es `80631d5e79dc568dc66416a6dedb9e6febca715f91a62dbcc25e22727e59c24f`, y una carpeta de salida nueva. El script se detiene si el ZIP base no coincide.

Ejemplo:

```powershell
.\scripts\Build-2.15.ps1 -BaseZip "C:\ruta\AccessShot-2.14-portable-Windows-x64.zip" -OutputDirectory "C:\ruta\salida-nueva"
```

El script compila los proyectos, publica el núcleo autocontenido para `win-x64`, mantiene el lanzador nativo de la base aprobada, lo conecta al anfitrión 2.15 compilado y genera un ZIP con su SHA-256. Puede recibir `-NuGetPackagesPath` para reutilizar una caché NuGet local cuando la red no esté disponible. No publica por sí solo en GitHub.

Paquete local preparado: `AccessShot-2.15-portable-Windows-x64.zip`, 72,526,745 bytes, SHA-256 `517dcdfdab0a1a960c0b1b9c83c1d57f216e1f4aa1955bdb422ba8e1ecf1b4ed`.
