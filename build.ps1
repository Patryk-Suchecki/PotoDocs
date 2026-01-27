# build.ps1
$ErrorActionPreference = "Stop"

# --- 1. Konfiguracja ---
$propsFile = "Directory.Build.props"
$outputDir = "docker_builds"

# --- 2. Pobieranie wersji ---
if (-not (Test-Path $propsFile)) {
    Write-Error "BLAD: Nie znaleziono pliku $propsFile w tym folderze."
    exit 1
}

try {
    [xml]$xml = Get-Content $propsFile
    $version = $xml.Project.PropertyGroup.Version
} catch {
    Write-Error "BLAD: Problem z odczytem pliku XML."
    exit 1
}

if ([string]::IsNullOrWhiteSpace($version)) {
    Write-Error "BLAD: Nie znaleziono numeru wersji w pliku $propsFile"
    exit 1
}

Write-Host ""
Write-Host "ROZPOCZYNAM BUDOWANIE WERSJI: $version" -ForegroundColor Cyan
Write-Host "==========================================" -ForegroundColor Gray

# --- 3. Przygotowanie folderu wyjściowego ---
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
    Write-Host "Utworzono folder: $outputDir" -ForegroundColor DarkGray
}

# --- 4. WEB (PotoDocs.Blazor) ---
Write-Host ""
Write-Host "[1/4] Budowanie obrazu WEB (Blazor)..." -ForegroundColor Yellow
docker build -t potodocs-web:$version -f PotoDocs.Blazor/Dockerfile .

Write-Host "[2/4] Zapisywanie WEB do pliku .tar..." -ForegroundColor Magenta
$webPath = Join-Path $outputDir "potodocs-web_$version.tar"
docker save -o $webPath potodocs-web:$version

# --- 5. API (PotoDocs.API) ---
Write-Host ""
Write-Host "[3/4] Budowanie obrazu API..." -ForegroundColor Yellow
docker build -t potodocs-api:$version -f PotoDocs.API/Dockerfile .

Write-Host "[4/4] Zapisywanie API do pliku .tar..." -ForegroundColor Magenta
$apiPath = Join-Path $outputDir "potodocs-api_$version.tar"
docker save -o $apiPath potodocs-api:$version

# --- 6. Podsumowanie ---
Write-Host ""
Write-Host "SUKCES! Wersja $version gotowa." -ForegroundColor Green
Write-Host "Twoje pliki znajduja sie w folderze $outputDir :"
Write-Host "   -> $webPath" -ForegroundColor White
Write-Host "   -> $apiPath" -ForegroundColor White