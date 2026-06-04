$root = Join-Path -Path (Get-Location) -ChildPath 'Hospital.Tests'
$files = Get-ChildItem -Path $root -Recurse -Filter *.cs
$methods = @()
foreach ($f in $files) {
    $lines = Get-Content $f.FullName
    for ($i = 0; $i -lt $lines.Length; $i++) {
        if ($lines[$i] -match '^\s*\[TestMethod\]') {
            $j = $i + 1
            while ($j -lt $lines.Length -and $lines[$j].Trim() -eq '') { $j++ }
            if ($j -lt $lines.Length -and $lines[$j] -match 'public\s+(async\s+)?(Task|void)\s+([A-Za-z0-9_]+)') {
                $methods += [PSCustomObject]@{ Name = $matches[3]; File = $f.FullName }
            }
        }
    }
}
$total = $methods.Count
$uniqueNames = $methods | Select-Object -ExpandProperty Name
$uniqueCount = ($uniqueNames | Sort-Object | Get-Unique).Count
Write-Output "TotalTestMethods: $total"
Write-Output "UniqueMethodNames: $uniqueCount"
$dups = $methods | Group-Object Name | Where-Object { $_.Count -gt 1 }
if ($dups.Count -eq 0) {
    Write-Output 'No duplicate test method names found.'
} else {
    Write-Output "Duplicate test method names found:"
    foreach ($d in $dups) {
        Write-Output "Name: $($d.Name) Count: $($d.Count)"
        foreach ($g in $d.Group) { Write-Output " - $($g.File)" }
    }
}
