param (
    [int]$h = 4000,
    
    # name of the output image
    [string]$filesHashesPath = "${PSScriptRoot}/../reference-files/file-hashes/tiberian-dawn/nod.json"
)

$fileHashes = (Get-Content -Raw $filesHashesPath) | ConvertFrom-Json

Get-ChildItem "${PSScriptRoot}/../MixFileExtractor/bin/Debug/net6.0/DATA" | ForEach-Object {
    $md5Hash = (Get-FileHash -Algorithm MD5 $_).Hash;
    $referenceHash = $fileHashes."$($_.Name)"

    if ($referenceHash -eq $null) {
        Write-Host -NoNewline "HASH MISS"
    } elseif ($md5Hash -eq $referenceHash) {
        Write-Host -NoNewline "HASH PASS"
    } else {
        Write-Host -NoNewline "HASH FAIL"
    }

    Write-Host " -> $($_.Name) [${referenceHash} | ${md5Hash}]"
} 
