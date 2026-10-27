# AccesShot 2.12

[Español](README.md) · [English](README.en.md)

![AccesShot](assets/social-preview.png)

**Tus aplicaciones, tus enlaces y tus etiquetas, organizados en un lienzo a tu medida.**

Lanzador portátil para Windows 11 x64. Crea accesos a aplicaciones clásicas y modernas, añade URL y organiza el espacio con etiquetas, colores y capas.

**Windows 11 x64 · Portátil · v2.12 · MIT**

## Descargar e iniciar

[Descargar la versión 2.12](https://github.com/SenecaDPJ/AccesShot/releases/tag/v2.12)

El enlace estará disponible tras crear la release en este repositorio privado; requiere acceso al repositorio.

1. Descarga el ZIP de aplicación y extrae todo su contenido.
2. Abre **AccessShot.exe**, visible en la primera carpeta.
3. Mantén **Aplicacion** junto al ejecutable. El runtime necesario está incluido.

## Funciones

| Objeto | Forma | Familia de color |
|---|---|---|
| Aplicación | Rectángulo redondeado | Naranja |
| Etiqueta | Cápsula | Magenta |
| URL | Punta de flecha | Cian |

- Aplicaciones tradicionales, apps modernas de Windows y direcciones web.
- Movimiento, redimensionado, texto y colores configurables.
- **MOVER ADELANTE**, **MOVER ATRAS**, **PONER EL 1º** y **PONER EL ÚLTIMO**; capas guardadas.
- Contornos limpios al superponer objetos, sin rectángulos negros alrededor.
- Temas oscuro y claro, selección y color al pasar el ratón.
- **RECARGAR**, banner de autoría y confirmación antes de abrir URL o GitHub.

## Apps modernas de Windows

**CREAR → APLICACIÓN → Buscar apps de Windows…**

Busca por nombre o usa **Fotos**, **Recortes**, **Enlace móvil** y **WhatsApp**. Solo aparecen aplicaciones instaladas para tu usuario. **Actualizar** vuelve a consultar el catálogo; desmarca **Solo apps modernas** para incluir las clásicas registradas. Se conserva el selector de archivos habitual.

## Colores predeterminados y personalizados

Los objetos con colores predeterminados se adaptan al tema. Al aceptar un fondo o texto personalizado, el objeto conserva sus propios colores al cambiar de tema y después de reiniciar.

**Restaurar colores del tema** recupera el estilo predeterminado. El sobrevuelo se puede personalizar y restablecer por separado; cambiarlo no desliga por sí solo los colores normales del tema.

Para objetos anteriores a la 2.11, la condición inicial se deduce de los colores guardados porque esas versiones no conservaban un historial de personalización.

## Datos

La disposición se guarda por usuario en `%APPDATA%\AccessShot\entries.json`. Puede contener rutas y URL personales: no lo incluyas en Git.

El buscador consulta el catálogo local de Windows. URL y GitHub piden confirmación antes de abrir el navegador; las aplicaciones iniciadas pueden tener sus propias conexiones a Internet.

## Estado del proyecto

**La 2.12 ha sido probada y aprobada por el autor.** Se publica el ZIP original, sin alterar sus bytes: [validación](docs/VALIDACION.md).

Se incluyen fuentes editables de los módulos, el lanzador nativo y material recuperado del núcleo para consulta. La reconstrucción integral del núcleo aún no está validada: [desarrollo](docs/DESARROLLO.md).

El repositorio se llama **AccesShot**. Los archivos internos mantienen **AccessShot** por compatibilidad con la versión aprobada.

## Autoría y licencia

**© David De Prado · 2026**  
[SenecaDPJ](https://github.com/SenecaDPJ) · Licencia [MIT](LICENSE).

Proyecto independiente, sin afiliación con Microsoft ni WhatsApp. [Avisos de terceros](THIRD_PARTY_NOTICES.md).
