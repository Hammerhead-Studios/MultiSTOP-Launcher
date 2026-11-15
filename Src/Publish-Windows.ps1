$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $root 'MultiSTOP.Launcher\MultiSTOP.Launcher.csproj'
$output = Join-Path $root 'publish'

Write-Host 'Publishing MultiSTOP for Windows x64...' -ForegroundColor Yellow

dotnet publish $project `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output $output `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:IncludeAllContentForSelfExtract=false `
    -p:EnableCompressionInSingleFile=true `
    -p:PublishTrimmed=false `
    -p:DebugType=embedded

$exe = Join-Path $output 'MultiSTOP.exe'
if (-not (Test-Path $exe)) {
    throw "Publish completed without the expected executable: $exe"
}

$otherFiles = @(Get-ChildItem -Path $output -File | Where-Object { $_.Name -ne 'MultiSTOP.exe' })
if ($otherFiles.Count -gt 0) {
    Write-Warning 'The publish folder contains files other than MultiSTOP.exe:'
    $otherFiles | ForEach-Object { Write-Warning $_.Name }
}
else {
    Write-Host 'Single-file publish complete.' -ForegroundColor Green
}

Write-Host "Output: $exe" -ForegroundColor Cyan
Write-Host 'Copy MultiSTOP.exe beside the game folder. The launcher expects .\game\p2008.exe.'
