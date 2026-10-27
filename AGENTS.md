# AccesShot — proyecto local

Lee `docs/DESARROLLO.md`, `docs/VALIDACION.md` y, para publicar la versión actual, `docs/ENTREGA_2.15.md`.

## Estado

- Repositorio privado: `SenecaDPJ/AccesShot`, rama predeterminada `main`.
- La release más reciente de GitHub es `v2.14`. Su README en `main` todavía enlaza la descarga `v2.13`; comprobar ambas páginas después de cualquier entrega.
- AccesShot 2.15 está en preparación local. Antes de crear la release, integrar sus fuentes y sus README en `main`; el tag `v2.15` debe apuntar al commit ya integrado.
- No publicar, cambiar la visibilidad, añadir colaboradores ni forzar ramas sin instrucción explícita del usuario.

## Invariantes

- La 2.12 aprobada se conserva byte por byte. Su ZIP no se recompila ni reempaqueta.
- Firma: © David De Prado · 2026. Licencia MIT confirmada.
- No modificar ChromePanelFix ni otros proyectos.
- No añadir binarios de distribución ni datos del usuario al historial Git. Los ZIP se adjuntan como assets de release.
- Repo `AccesShot`; binarios históricos `AccessShot`. Mantener compatibilidad.
- Preservar la referencia histórica 2.7_OPERATIVO_v777. Cambios posteriores requieren otra versión.
- Objetos con fondo/texto personalizados no siguen el tema. Personalizar solo el sobrevuelo no desliga sus colores normales.
- URL con flecha; etiqueta con cápsula; aplicación con rectángulo redondeado.

## Fuentes

Los módulos son fuentes editables. El núcleo recuperado es consulta: no se afirma que su reconstrucción integral esté validada. La reflexión depende de nombres de campos/cierres que puede transformar el descompilador. No activar publicación automática basada en ese snapshot.

Para la 2.15, integrar primero fuentes y ambos README en `main`; solo después crear el tag y la release desde el commit integrado. Verificar que la página principal y los dos README enlazan a `v2.15`, que GitHub la marca como **Latest** y que el SHA del asset coincide. No publicar ni alterar GitHub sin instrucción explícita; no borrar ni forzar sobrescrituras.
