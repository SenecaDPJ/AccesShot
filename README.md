# AccesShot 2.16

[Español](README.md) · [English](README.en.md)

![AccesShot](assets/social-preview.png)

**Tu PC a un clic 🥷🏻**

Aplicaciones, enlaces y etiquetas, organizados en un lienzo a medida de tus pantallas.

AccesShot reúne tus programas, aplicaciones modernas de Windows y páginas web en una sola pantalla. Crea accesos, organízalos con etiquetas y capas, y personaliza su aspecto con colores y temas. Es portátil y está diseñado para Windows 11 x64.

- 🖱️Ten a mano y como tú quieres todos tus accesos sin importar su origen.
- 🗂️Organízate como quieras sin molestas limitaciones.
- 🖥️Lienzo personalizable donde el límite está en tus monitores disponibles.
- 🎨Tunea tus accesos con colores, efectos, tamaños y capas.

**Windows 11 x64 · Portátil · v2.16 · MIT**

## Descargar e iniciar

[Descargar la versión 2.16](https://github.com/SenecaDPJ/AccesShot/releases/tag/v2.16)

La descarga está disponible en la versión 2.16. Necesitas acceso a este repositorio privado para verla.

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
- Movimiento en grupo: arrastra un rectángulo en MOVER o ELIMINAR; Ctrl + clic añade o quita objetos.
- Giros de 90°. Las formas y flechas giran; el texto permanece horizontal. Las etiquetas pueden crearse verticales.
- Confirmación detallada antes de borrar varios objetos. Ctrl + Z recupera la última edición del lienzo.
- Redimensionado, colores configurables, capas y texto ajustable.
- Copias JSON desde el botón del lienzo; Opciones permite cambiar la carpeta y restaurar una copia.
- Ctrl + O abre Opciones dentro de AccesShot.
- Temas oscuro y claro, selección y color al pasar el ratón.
- Confirmación antes de abrir URL o GitHub.

## Apps modernas de Windows

En **CREAR → APLICACIÓN → Buscar apps de Windows…** puedes buscar aplicaciones instaladas para tu usuario, incluidas Fotos, Recortes, Enlace móvil y WhatsApp. El selector clásico de archivos sigue disponible.

## Datos

La configuración se guarda por usuario en `%APPDATA%\AccessShot\entries.json`. En un perfil nuevo, AccesShot crea ese archivo con la disposición inicial de `assets/initial-layout.json`; si el archivo ya existe, conserva los accesos guardados. La última operación deshacible y las preferencias de copia se guardan en archivos auxiliares de esa carpeta. Al migrar por primera vez, AccesShot conserva una copia `.pre-2.15.bak`. La disposición inicial del autor se incluye en este repositorio privado a petición suya. Las copias JSON personales pueden incluir rutas y URL; guárdalas en una carpeta privada y no las subas a Git.

Una copia contiene los accesos y su disposición, no los programas ni los archivos a los que apuntan. En otro PC algunas rutas quizá necesiten ajustes.

## Estado del proyecto

La 2.16 coloca el aviso de desbordamiento en la parte superior del lienzo, sin tapar la firma, restaura el icono de la ventana y actualiza la disposición inicial para perfiles nuevos. Las configuraciones existentes se conservan. Consulta [la guía de entrega](docs/ENTREGA_2.16.md), [validación](docs/VALIDACION_2.16.md) y [desarrollo](docs/DESARROLLO.md).

Las releases 2.12, 2.13, 2.14 y 2.15 se conservan. La aplicación continúa siendo portable para Windows 11 x64.

## Autoría y licencia

© David De Prado · 2026 · [SenecaDPJ](https://github.com/SenecaDPJ) · [MIT](LICENSE).

Proyecto independiente, sin afiliación con Microsoft ni WhatsApp. Consulta [avisos de terceros](THIRD_PARTY_NOTICES.md).
