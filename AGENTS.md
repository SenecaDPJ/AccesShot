# AccesShot — proyecto local

Lee `docs/DESARROLLO.md`, `docs/VALIDACION.md` y, para publicar la versión actual, `docs/ENTREGA_3.0.md`.

## Estado

- Repositorio público: `SenecaDPJ/AccesShot`, rama predeterminada `main`.
- La release estable más reciente es `v3.0`, con paquete portable para Windows 11 x64 y documentación en español e inglés.
- `main` contiene las fuentes, README, notas y documentos de entrega correspondientes a `v3.0`.
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

Para futuras publicaciones, integrar primero fuentes, ambos README y documentos de entrega en `main`; solo después crear el tag y la release desde el commit integrado. Verificar que la página principal y ambos README enlazan a la nueva versión, que GitHub la marca como **Latest** y que el SHA del asset coincide. No publicar ni alterar GitHub sin instrucción explícita; no borrar ni forzar sobrescrituras.
