# AccesShot 2.13

[Español](README.md) · [English](README.en.md)

![AccesShot](assets/social-preview.png)

**Your apps, links and labels, arranged on your own canvas.**

AccesShot brings your programs, modern Windows apps, and web pages together on one screen. Create shortcuts, arrange them with labels and layers, and customize colors and themes. It is portable for Windows 11 x64.

## Quick start

1. Download the [2.13 release](https://github.com/SenecaDPJ/AccesShot/releases/tag/v2.13); this private repository requires access.
2. Extract the complete ZIP and open **AccessShot.exe**.
3. Keep **Aplicacion** beside it. The required runtime is included.

## Features

- Rounded app tiles, capsule labels and arrow-shaped URLs.
- Local modern-app search, with filters for Photos, Snipping Tool, Phone Link and WhatsApp when installed for the current user.
- Move, resize, edit text and colors; move forward/backward or bring to front/send to back.
- Default colors follow the theme until background or text is customized. Hover colors are independently configurable.
- Confirmation before opening URL shortcuts or the GitHub profile.

Per-user data: `%APPDATA%\AccessShot\entries.json`. Keep personal paths and URLs out of Git. New profiles receive a public sample layout, including one small label and URL outside the initial view; existing layouts are preserved.

## Release and source status

The 2.13 core is rebuilt from adapted recovered sources in `src/Core` and tested for the new starter layout. The 2.12 release remains available unchanged. See [validation](docs/VALIDACION_2.13.md).

Version 2.12 was tested and approved by the author. Its release ZIP is preserved byte-for-byte. Editable module and native launcher sources are included, alongside recovered core reference material. The 2.13 rebuilt core has been tested for startup and the starter layout; a complete regression review of every historical function is still pending. See [development notes](docs/DESARROLLO.md).

Repository: **AccesShot**. Historical executable names remain **AccessShot** for compatibility.

Copyright © 2026 David De Prado (Seneca DPJ). [MIT](LICENSE). [GitHub profile](https://github.com/SenecaDPJ).

Independent project, not affiliated with Microsoft or WhatsApp. Third-party components retain their own licenses.
