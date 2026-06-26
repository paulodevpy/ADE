param(
    [string]$Projeto = ".",
    [string]$Destino = ".\ADE_Projeto_Analise.zip"
)

$Projeto = (Resolve-Path $Projeto).Path

$temp = Join-Path $env:TEMP ("ADE_EXPORT_" + [guid]::NewGuid())

New-Item -ItemType Directory -Force -Path $temp | Out-Null

$IgnorarPastas = @(
    "\bin\",
    "\obj\",
    "\.git\",
    "\.vs\",
    "\packages\",
    "\node_modules\"
)

$IgnorarArquivos = @(
    "logo.png",
    "ade.png",
    "ChatGPT*.png",
    "logo_ADE*.png"
)

$Extensoes = @(
    ".cs",
    ".xaml",
    ".csproj",
    ".json",
    ".xml",
    ".md",
    ".ps1",
    ".config",
    ".props",
    ".targets",
    ".ico"
)

Get-ChildItem $Projeto -Recurse -File |

Where-Object {

    $arquivo = $_.FullName

    $ok = $false

    foreach($e in $Extensoes){
        if($_.Extension.ToLower() -eq $e){
            $ok = $true
            break
        }
    }

    if(-not $ok){
        return $false
    }

    foreach($i in $IgnorarPastas){
        if($arquivo -like "*$i*"){
            return $false
        }
    }

    foreach($i in $IgnorarArquivos){
        if($_.Name -like $i){
            return $false
        }
    }

    return $true

} |

ForEach-Object {

    $relativo = $_.FullName.Substring($Projeto.Length).TrimStart("\")
    $destinoArquivo = Join-Path $temp $relativo

    $pasta = Split-Path $destinoArquivo

    if(!(Test-Path $pasta)){
        New-Item -ItemType Directory -Force -Path $pasta | Out-Null
    }

    Copy-Item $_.FullName $destinoArquivo
}

if(Test-Path $Destino){
    Remove-Item $Destino -Force
}

@'
# Imagens removidas do pacote

As imagens foram removidas automaticamente para reduzir o tamanho do arquivo exportado.

Arquivos removidos:

- logo.png
- ade.png
- logo_ADE*.png
- ChatGPT*.png

Esses arquivos permanecem no projeto original.
'@ | Set-Content (Join-Path $temp "IMAGENS_REMOVIDAS.md") -Encoding UTF8

Compress-Archive `
    -Path (Join-Path $temp "*") `
    -DestinationPath $Destino `
    -CompressionLevel Optimal

Remove-Item $temp -Recurse -Force

Write-Host ""
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "EXPORTAÇÃO CONCLUÍDA" -ForegroundColor Green
Write-Host "Arquivo:" -NoNewline
Write-Host " $Destino" -ForegroundColor Yellow
Write-Host "======================================" -ForegroundColor Cyan