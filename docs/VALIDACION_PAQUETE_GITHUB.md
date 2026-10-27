# Comprobación del paquete de GitHub

27/10/2026.

- MIT idéntica al blob verificado de ChromePanelFix.
- ZIP 2.12 idéntico al aprobado: checksum y tamaño correctos.
- Fuentes editables idénticos a los módulos usados, salvo cabecera de autoría.
- Proyectos de módulos/pruebas: XML válido; el nuevo layout de build queda para ejecución en Windows con SDK .NET 8.
- Scripts PowerShell analizados con el parser oficial 7.4.6, sin errores de sintaxis.
- Comandos gh de creación privada, topics y release contrastados con la documentación oficial.
- Exclusiones Git verificadas para datos del usuario, binarios y credenciales; README, fuentes e imágenes no se excluyen.
- Enlaces Markdown locales comprobados.
- Imagen social inspeccionada visualmente.
- No se ha creado el repositorio, hecho push ni publicado la release en esta preparación.

La verificación sintáctica de los scripts no equivale a una publicación de prueba real. La ejecución se realizará desde el entorno local con la cuenta correcta.
