$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
$WebView2 = '1.0.4314.35'
$JSZip = '3.10.2'

$csc = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$obj = "$PSScriptRoot\obj"
$app = "$obj\app"
$dist = "$PSScriptRoot\dist"
New-Item -ItemType Directory -Force $app, $dist | Out-Null

$nupkg = "$obj\webview2-$WebView2.zip"
if (-not (Test-Path $nupkg)) {
    Invoke-WebRequest -UseBasicParsing "https://www.nuget.org/api/v2/package/Microsoft.Web.WebView2/$WebView2" -OutFile $nupkg
}
Expand-Archive $nupkg "$obj\webview2" -Force
$wv = "$obj\webview2"
Copy-Item "$wv\lib\net462\Microsoft.Web.WebView2.Core.dll", "$wv\lib\net462\Microsoft.Web.WebView2.WinForms.dll",
    "$wv\runtimes\win-x64\native\WebView2Loader.dll", "$PSScriptRoot\src\index.html" $app
Invoke-WebRequest -UseBasicParsing "https://cdn.jsdelivr.net/npm/jszip@$JSZip/dist/jszip.min.js" -OutFile "$app\jszip.min.js"
Copy-Item "$PSScriptRoot\vendor\docx-preview.min.js" $app

& $csc /nologo /codepage:65001 /target:winexe /win32icon:"$PSScriptRoot\src\app.ico" /win32manifest:"$PSScriptRoot\src\app.manifest" /platform:x64 /out:"$app\DocxViewer.exe" /r:System.Windows.Forms.dll /r:System.Drawing.dll `
    /r:"$app\Microsoft.Web.WebView2.Core.dll" /r:"$app\Microsoft.Web.WebView2.WinForms.dll" "$PSScriptRoot\src\DocxViewer.cs"
if ($LASTEXITCODE) { exit $LASTEXITCODE }

$resources = Get-ChildItem $app -File | ForEach-Object { "/resource:$($_.FullName),$($_.Name)" }
& $csc /nologo /codepage:65001 /target:winexe /win32icon:"$PSScriptRoot\src\app.ico" /win32manifest:"$PSScriptRoot\src\app.manifest" /out:"$dist\DocxViewer-Setup.exe" /r:System.Windows.Forms.dll $resources "$PSScriptRoot\src\Setup.cs"
if ($LASTEXITCODE) { exit $LASTEXITCODE }
Write-Host "Built $dist\DocxViewer-Setup.exe"
