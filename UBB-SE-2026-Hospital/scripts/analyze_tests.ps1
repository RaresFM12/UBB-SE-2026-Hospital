$root = Join-Path -Path (Get-Location) -ChildPath 'Hospital.Tests'
$files = Get-ChildItem -Path $root -Recurse -Filter *.cs
$results = @()
foreach ($f in $files) {
    $lines = Get-Content $f.FullName
    for ($i=0;$i -lt $lines.Length;$i++) {
        if ($lines[$i] -match '^\s*\[TestMethod\]') {
            # find method signature
            $j = $i+1
            while ($j -lt $lines.Length -and $lines[$j].Trim() -eq '') { $j++ }
            if ($j -lt $lines.Length -and $lines[$j] -match 'public\s+(async\s+)?(Task|void)\s+([A-Za-z0-9_]+)') {
                $name = $matches[3]
                # find method body braces
                $k = $j
                while ($k -lt $lines.Length -and $lines[$k] -notmatch '{') { $k++ }
                if ($k -ge $lines.Length) { continue }
                $brace = 0
                $start = -1
                for ($m=$k;$m -lt $lines.Length;$m++) {
                    $line = $lines[$m]
                    foreach ($ch in $line.ToCharArray()) {
                        if ($ch -eq '{') { if ($start -eq -1) { $start = $m }; $brace++ }
                        if ($ch -eq '}') { $brace-- }
                    }
                    if ($start -ne -1 -and $brace -eq 0) { $end = $m; break }
                }
                if ($start -eq -1 -or $end -eq $null) { continue }
                $body = $lines[$start..$end] -join "`n"
                $assertCount = ([regex]::Matches($body,'\bAssert\.|Assert\b','IgnoreCase')).Count
                $verifyCount = ([regex]::Matches($body,'\.Verify\(','IgnoreCase')).Count
                $mockUsage = ([regex]::Matches($body,'new\s+Mock<','IgnoreCase')).Count
                $newRepo = ([regex]::Matches($body,'new\s+\w+Repository','IgnoreCase')).Count
                $dbCtx = ([regex]::Matches($body,'new\s+HospitalDbContext','IgnoreCase')).Count
                $io = ([regex]::Matches($body,'File\.|HttpClient|SendAsync|HttpWebRequest|Process\.Start','IgnoreCase')).Count
                $underscoreCount = ($name -split '_').Count - 1
                $results += [PSCustomObject]@{
                    File = $f.FullName; Name = $name; Underscores = $underscoreCount; Asserts = $assertCount; Verifies = $verifyCount; MockNew = $mockUsage; NewRepo = $newRepo; DbCtx = $dbCtx; IO = $io
                }
            }
        }
    }
}
$total = $results.Count
Write-Output "TotalTests: $total"
$multiAssert = $results | Where-Object { $_.Asserts -gt 1 -or $_.Verifies -gt 1 }
$noAssert = $results | Where-Object { $_.Asserts -eq 0 -and $_.Verifies -eq 0 }
$badNames = $results | Where-Object { $_.Underscores -gt 2 }
$nonUnit = $results | Where-Object { $_.NewRepo -gt 0 -or $_.DbCtx -gt 0 -or $_.IO -gt 0 }
Write-Output "Tests with >1 assert/verify: $($multiAssert.Count)"
Write-Output "Tests with 0 assert/verify: $($noAssert.Count)"
Write-Output "Tests with name underscores >2: $($badNames.Count)"
Write-Output "Potential non-unit tests (new repo/db/io): $($nonUnit.Count)"
if ($multiAssert.Count -gt 0) { Write-Output "-- Multi-assert tests:"; $multiAssert | ForEach-Object { Write-Output "$($_.Name) in $($_.File) Asserts=$($_.Asserts) Verifies=$($_.Verifies)" } }
if ($noAssert.Count -gt 0) { Write-Output "-- No-assert tests:"; $noAssert | ForEach-Object { Write-Output "$($_.Name) in $($_.File)" } }
if ($badNames.Count -gt 0) { Write-Output "-- Bad name tests:"; $badNames | ForEach-Object { Write-Output "$($_.Name) in $($_.File)" } }
if ($nonUnit.Count -gt 0) { Write-Output "-- Potential non-unit tests:"; $nonUnit | ForEach-Object { Write-Output "$($_.Name) in $($_.File) NewRepo=$($_.NewRepo) DbCtx=$($_.DbCtx) IO=$($_.IO)" } }
