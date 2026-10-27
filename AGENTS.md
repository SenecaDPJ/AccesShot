# AccesShot — Codex local

Lee primero docs/HANDOFF_CODEX.md, docs/DESARROLLO.md y docs/VALIDACION.md.

## Objetivo

Crear y subir **SenecaDPJ/AccesShot**, **privado**, rama `main`, release estable `v2.12`, cuando el usuario solicite efectuar la subida desde su entorno local. El paquete no ha creado todavía el repositorio.

## Invariantes

- La 2.12 está probada y aprobada por David. Publicar su ZIP existente sin recompilar, reempaquetar ni corregir metadatos internos. Verificar SHA-256.
- Firma: © David De Prado · 2026. Licencia MIT confirmada en ChromePanelFix.
- No modificar ChromePanelFix ni otros proyectos.
- No cambiar a público ni añadir colaboradores por iniciativa propia.
- No añadir binarios de distribución ni datos del usuario al historial Git. Los ZIP van como assets de release.
- Repo `AccesShot`; binarios internos `AccessShot`. No renombrarlos en la 2.12.
- Preservar la referencia histórica 2.7_OPERATIVO_v777. Cambios posteriores: otra versión.
- Objetos con fondo/texto personalizados no siguen el tema. Personalizar solo el sobrevuelo no desliga sus colores normales.
- URL con flecha; etiqueta con cápsula; aplicación con rectángulo redondeado.

## Fuentes

Los módulos son fuentes editables. El núcleo recuperado es consulta: no se afirma que su reconstrucción integral esté validada. La reflexión depende de nombres de campos/cierres que puede transformar el descompilador. No activar publicación automática basada en ese snapshot.

Publish-GitHub.ps1 muestra un plan por defecto. -Publish realiza la subida. Si existe el repo o queda una ejecución parcial, inspeccionar antes de continuar; no borrar ni forzar sobrescrituras.
