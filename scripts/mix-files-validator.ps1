Get-ChildItem "..\MixFileExtractor\bin\Debug\net6.0\DATA" | ForEach-Object {
    $referenceFile = "..\example\DATA\$($_.Name)";

    if (-not (Test-Path $referenceFile -ErrorAction Ignore)) {
        Write-Host -NoNewline "MISSING"
    } elseif ((Get-FileHash $_).Hash -eq (Get-FileHash $referenceFile).Hash) {
        Write-Host -NoNewline "GD_HASH"
    } else {
        Write-Host -NoNewline "BD_HASH"
    }

    Write-Host " -> $($_.Name)"
}
