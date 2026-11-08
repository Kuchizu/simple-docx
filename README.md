<p align="center"><img src="docs/icon.png" width="96" alt=""></p>

<h1 align="center">simple-docx</h1>

<p align="center">A 1 MB <code>.docx</code> viewer for Windows. No Office, no cloud.</p>

<p align="center"><a href="https://github.com/Kuchizu/simple-docx/releases/latest/download/DocxViewer-Setup.exe"><b>Download DocxViewer-Setup.exe</b></a></p>

![DOCX Viewer opening a document](docs/demo.gif)

- Double-click any `.docx`, drag and drop, or Ctrl+O
- Pages, fonts, tables and footnotes laid out close to Word
- Export to PDF in one click
- Documents are rendered on your PC and never uploaded
- English and Russian interface, picked from your system language

## Speed

From launch to a rendered document, median of 5 repeat opens on an i7-12700H:

| Document | Time |
| --- | --- |
| 3 pages | 0.35 s |
| 23 pages | 0.66 s |
| 76 pages | 0.82 s |

## Install

Run the installer. No admin rights needed. Uninstall from Settings > Apps. Requires the WebView2 Runtime, which is built into Windows 11.

- The installer is not signed yet, so Windows may show a SmartScreen warning: choose More info > Run anyway.
- If Word is your default app for `.docx`, Windows asks you to pick DOCX Viewer once in Settings.
- For unattended installs use `DocxViewer-Setup.exe /silent`.

## Build

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

Output: `dist\DocxViewer-Setup.exe`. Built with WebView2 and a patched [docx-preview](https://github.com/Kuchizu/docxjs/tree/simple-docx) bundled in `vendor/`.
