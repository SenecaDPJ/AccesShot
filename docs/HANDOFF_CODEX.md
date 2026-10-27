# Traspaso a Codex local — GitHub

**Autor:** David De Prado / SenecaDPJ.  
**Nuevo repo:** SenecaDPJ/AccesShot. **Visibilidad:** privado.  
**Rama:** main. **Tag:** v2.12. **Fecha:** 27/10/2026.

No se ha creado el repositorio en esta preparación. La consulta autenticada realizada no lo encontró; volver a comprobar al efectuar la subida.

## Carpetas del paquete

- `repo/`: abrir esta carpeta como proyecto en Codex local.
- `release/AccessShot-2.12-apps-Windows-portable-Windows-x64.zip`: aplicación original aprobada.
- `release/AccesShot-2.12-licencias.zip`: MIT y avisos complementarios.
- `release/SHA256SUMS.txt`: hashes de los assets.

## Publicación

1. Extraer el paquete entero y mantener repo/release como hermanas.
2. Leer AGENTS.md, DESARROLLO.md y VALIDACION.md.
3. Ejecutar scripts/Verify-Release.ps1 y revisar docs/project.json.
4. Comprobar Git, GitHub CLI, identidad de commit y cuenta autenticada SenecaDPJ. No inventar correo ni cambiar credenciales globales.
5. scripts/Publish-GitHub.ps1 muestra el plan.
6. Al realizar la subida solicitada, scripts/Publish-GitHub.ps1 -Publish crea el repo privado, sube main y crea la release.
7. Confirmar que el repo sigue siendo privado y que existen los assets.
8. En Settings → General → Social preview, seleccionar assets/social-preview.png. No está automatizado por el script.

Si queda una subida parcial, inspeccionar antes de reintentar. No borrar el repo ni forzar pushes para reiniciar.

SHA-256 del ZIP aprobado: `c8f8658f8ed9c4f3a2ef8eec69fea86eab0f354b9ca4b0c82f1ca87bb4c1f6c6`.

## Nombres y preservación

AccesShot es el nuevo nombre del repositorio. AccessShot es el histórico de los binarios y datos por usuario. Conservarlos en la 2.12.

La 2.7_OPERATIVO_v777 permanece preservada aparte como referencia histórica. Las 2.4 y 2.6 fueron rechazadas; no publicarlas como estables.

## Mensaje para retomar

«Lee AGENTS.md y docs/HANDOFF_CODEX.md. Publica este paquete como SenecaDPJ/AccesShot privado, rama main y release v2.12. Usa el ZIP ya validado, verifica su checksum y conserva MIT y mi firma. No recompiles la aplicación para esta primera subida.»
