<img src="docs/icon.png" width="96" align="right" alt="">

# simple-docx

A tiny Windows app for when you just want to quickly look at a `.docx` without waiting for Word.

![DOCX Viewer](docs/screenshot.png)

- Opens instantly, works offline, no Office needed
- Keeps the layout: fonts, colors, tables, pages
- Double-click any `.docx`, drag and drop, or Ctrl+O

## Install

Download [DocxViewer-Setup.exe](https://github.com/Kuchizu/simple-docx/releases/latest/download/DocxViewer-Setup.exe) and run it. No admin rights needed. Uninstall from Settings > Apps.

## Build

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

Output: `dist\DocxViewer-Setup.exe`. Built with [docx-preview](https://github.com/VolodymyrBaydalka/docxjs) and WebView2.
