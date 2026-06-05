$desktopProxyDir = "c:\Users\Gaboruu\Informatica\ISS\UBB-SE-2026-Hospital\UBB-SE-2026-Hospital\Hospital.Desktop\Proxy"
$webServicesDir = "c:\Users\Gaboruu\Informatica\ISS\UBB-SE-2026-Hospital\UBB-SE-2026-Hospital\Hospital.Web\Services"
$sharedProxiesDir = "c:\Users\Gaboruu\Informatica\ISS\UBB-SE-2026-Hospital\UBB-SE-2026-Hospital\Hospital.Shared\Proxies"

if (-not (Test-Path $sharedProxiesDir)) {
    New-Item -ItemType Directory -Path $sharedProxiesDir | Out-Null
}

# 1. Move Desktop Proxies
Get-ChildItem -Path $desktopProxyDir -Filter "*.cs" | Where-Object { $_.Name -ne "ProxyBase.cs" } | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $content = $content -replace "namespace Hospital\.Desktop\.Proxy;", "namespace Hospital.Shared.Proxies;"
    $content = $content -replace "ProxyBase\(httpClient\)", "ApiClientBase(httpClient)"
    Set-Content -Path (Join-Path $sharedProxiesDir $_.Name) -Value $content
}

# 2. Move Web ApiClients
Get-ChildItem -Path $webServicesDir -Filter "*ApiClient.cs" | Where-Object { $_.Name -ne "HospitalApiClientBase.cs" } | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $content = $content -replace "namespace Hospital\.Web\.Services;", "namespace Hospital.Shared.Proxies;"
    $content = $content -replace "HospitalApiClientBase\(httpClient, httpContextAccessor\)", "ApiClientBase(httpClient)"
    $content = $content -replace "HospitalApiClientBase", "ApiClientBase"
    # Remove IHttpContextAccessor from constructor
    $content = $content -replace ", IHttpContextAccessor httpContextAccessor", ""
    
    # We will rename it if it clashes, but they don't clash by filename (*Proxy vs *ApiClient)
    Set-Content -Path (Join-Path $sharedProxiesDir $_.Name) -Value $content
}

# Move Web ApiClient interfaces
Get-ChildItem -Path $webServicesDir -Filter "I*ApiClient.cs" | ForEach-Object {
    $content = Get-Content $_.FullName -Raw
    $content = $content -replace "namespace Hospital\.Web\.Services;", "namespace Hospital.Shared.Proxies;"
    Set-Content -Path (Join-Path $sharedProxiesDir $_.Name) -Value $content
}

# 3. Update DI
$desktopDI = "c:\Users\Gaboruu\Informatica\ISS\UBB-SE-2026-Hospital\UBB-SE-2026-Hospital\Hospital.Desktop\DependencyInjection\ServiceCollectionExtensions.cs"
if (Test-Path $desktopDI) {
    $diContent = Get-Content $desktopDI -Raw
    $diContent = $diContent -replace "using Hospital\.Desktop\.Proxy;", "using Hospital.Shared.Proxies;"
    Set-Content -Path $desktopDI -Value $diContent
}

$webDI = "c:\Users\Gaboruu\Informatica\ISS\UBB-SE-2026-Hospital\UBB-SE-2026-Hospital\Hospital.Web\DependencyInjection\ServiceCollectionExtensions.cs"
if (Test-Path $webDI) {
    $webDiContent = Get-Content $webDI -Raw
    if (-not ($webDiContent -match "using Hospital.Shared.Proxies;")) {
        $webDiContent = $webDiContent -replace "using Hospital\.Web\.Services;", "using Hospital.Web.Services;`nusing Hospital.Shared.Proxies;"
        Set-Content -Path $webDI -Value $webDiContent
    }
}
