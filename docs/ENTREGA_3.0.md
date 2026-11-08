# Entrega de AccesShot 3.0

## Publicación

- Rama: `main`.
- Tag y release estable: `v3.0`, desde el commit integrado en `main`.
- Release marcada como **Latest** en GitHub.
- Activos: `AccessShot-3.0-portable-Windows-x64.zip` y `SHA256SUMS_v3.0.txt`.
- SHA-256 del ZIP: `ad6c87ad4bbe83d528cba54048680aa9861a9df0171db08c393ff7ce03cd0049`.
- Tamaño del ZIP: 72534360 bytes.

La release 3.0 conserva las versiones 2.12 a 2.16. El paquete no se añade al historial Git; se distribuye como activo de la release.

## Reproducción

Usa `scripts/Build-3.0.ps1` con el ZIP 2.16 cuyo SHA-256 es `b54f2bab06ed18d005d13bf4042e931624015e9d6cbb284907166293997d54a3`. El script compila los tres proyectos, publica para `win-x64` como aplicación autocontenida y crea el ZIP y el archivo de checksum. El procedimiento de compilación y los resultados están en [VALIDACION_3.0.md](VALIDACION_3.0.md).

Después de cualquier republicación, comprobar que GitHub muestra `v3.0` como **Latest**, que README en español e inglés enlazan al ZIP y que el digest SHA-256 del activo coincide con el checksum publicado.
