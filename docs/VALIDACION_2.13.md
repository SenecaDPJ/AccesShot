# Validación de AccesShot 2.13

## Alcance

La 2.13 reconstruye el núcleo desde `src/Core` con los módulos originales `src/Styles` y `src/WindowsApps`, en .NET 8 para Windows x64. Conserva el lanzador nativo y el supervisor ya validados en la 2.12. La 2.12 original y su tag no se modifican.

## Pruebas realizadas

- Compilación de los tres proyectos sin errores.
- Prueba aislada: con un perfil vacío se cargan cuatro objetos de ejemplo. La etiqueta y la URL situadas fuera de la vista inicial miden 90 × 50.
- Con un área de 1103 × 516 se cuentan dos objetos fuera de la vista y aparece el aviso.
- El guardado crea `entries.json` en un perfil de prueba; la reapertura lee los cuatro objetos existentes.
- El ejecutable publicado arranca con el runtime incluido sin que .NET 8 esté instalado globalmente.
- Los archivos de usuario `%APPDATA%\AccessShot\entries.json` no entran en el ZIP ni en Git.

Estas pruebas verifican el cambio de disposición inicial y el arranque. No sustituyen una revisión visual completa ni una prueba manual de todas las funciones históricas en el equipo del autor.
