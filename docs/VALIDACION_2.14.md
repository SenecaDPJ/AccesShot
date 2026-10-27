# Validación de AccesShot 2.14

## Corrección

El selector de aplicaciones buscaba el campo `<>4__this`, pero la fuente recuperada del núcleo lo conserva como `_003C_003E4__this`. La búsqueda devolvía una referencia nula al crear un acceso de tipo aplicación.

## Comprobaciones

- Compilación Release de `AccessShotStyles`, `AccessShotWindowsApps` y `AccessShotCore` sin errores.
- Prueba de integración: el contexto real de `MainForm.EditEntry` se pasa a `WindowsAppsPicker.Attach`; se confirma el botón «Buscar apps de Windows…» en el diálogo.
- Prueba de arranque y de la disposición inicial: cuatro objetos de ejemplo, dos fuera de la vista y aviso visible.
- Guardado y reapertura en perfil aislado; el diseño existente se conserva.
- Distribución win-x64 autocontenida, datos de usuario excluidos del ZIP.

La consulta de aplicaciones instaladas requiere Windows. La prueba de integración cubre la conexión entre el núcleo y el selector; la enumeración real de aplicaciones se ejecuta al abrir el selector en Windows.