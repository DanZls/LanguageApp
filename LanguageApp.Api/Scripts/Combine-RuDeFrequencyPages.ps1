param(
    [string]$SourceDir = "Resources/Processed/DictionaryRuDeFrequencyPages-50",
    [string]$DestinationDir = "Resources/Processed/DictionaryRuDeFrequencyPages-100",
    [int]$FileLimit = 0
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$sourcePath = Resolve-Path -Path $SourceDir
$destinationPath = Join-Path -Path (Get-Location) -ChildPath $DestinationDir

if (-not (Test-Path -Path $destinationPath)) {
    New-Item -ItemType Directory -Path $destinationPath -Force | Out-Null
}

$files = Get-ChildItem -Path $sourcePath -File -Filter "*.txt" |
    Where-Object { $_.BaseName -match "^\d+$" } |
    Sort-Object { [int]$_.BaseName }

if ($files.Count -eq 0) {
    throw "No numeric .txt files found in '$sourcePath'."
}

if ($FileLimit -gt 0) {
    $files = $files | Select-Object -First $FileLimit
}

if ($files.Count % 2 -ne 0) {
    throw "Expected an even number of files, found $($files.Count)."
}

# Optional safety check for the exact expected input size.
if ($files.Count -ne 200) {
    Write-Warning "Expected 200 files, found $($files.Count). Proceeding anyway."
}

for ($i = 0; $i -lt $files.Count; $i += 2) {
    $leftFile = $files[$i]
    $rightFile = $files[$i + 1]
    $outputIndex = [int]($i / 2) + 1
    $outputFile = Join-Path -Path $destinationPath -ChildPath ("{0}.txt" -f $outputIndex)

    # Preserve source encoding by combining raw bytes instead of decoding text.
    $leftBytes = [System.IO.File]::ReadAllBytes($leftFile.FullName)
    $rightBytes = [System.IO.File]::ReadAllBytes($rightFile.FullName)

    # Trim trailing CR/LF bytes from the left part, then insert exactly one CRLF.
    $leftEnd = $leftBytes.Length
    while ($leftEnd -gt 0 -and ($leftBytes[$leftEnd - 1] -eq 10 -or $leftBytes[$leftEnd - 1] -eq 13)) {
        $leftEnd--
    }

    $combined = New-Object byte[] ($leftEnd + 2 + $rightBytes.Length)
    if ($leftEnd -gt 0) {
        [System.Array]::Copy($leftBytes, 0, $combined, 0, $leftEnd)
    }
    $combined[$leftEnd] = 13
    $combined[$leftEnd + 1] = 10
    [System.Array]::Copy($rightBytes, 0, $combined, $leftEnd + 2, $rightBytes.Length)

    [System.IO.File]::WriteAllBytes($outputFile, $combined)

    Write-Host "Created $outputFile from $($leftFile.Name) + $($rightFile.Name)"
}

Write-Host "Done. Created $($files.Count / 2) files in '$destinationPath'."