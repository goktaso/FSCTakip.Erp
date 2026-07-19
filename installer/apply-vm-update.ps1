#requires -Version 5.1
<#
    FSC Takip ERP - VM/musteri guncelleme scripti (2026-07-19)
    SSMS veya sqlcmd GEREKTIRMEZ - System.Data.SqlClient (.NET Framework'un parcasi,
    her Windows'ta hazir gelir) kullanir.

    Ne yapar (sirayla):
      1) FscErpDb'yi yedekler (.bak dosyasi)
      2) vm-update-2026-07-19.sql migration script'ini calistirir (idempotent - guvenli)

    Kullanim: Bu dosyayi ve yaninda duran "vm-update-2026-07-19.sql" dosyasini
    ayni klasorde tutup, PowerShell'i YONETICI olarak acip su sekilde calistirin:

        cd "<bu-dosyanin-oldugu-klasor>"
        .\apply-vm-update.ps1

    Sorulan sorulara Enter'a basarsaniz varsayilan degerler kullanilir.
#>

$ErrorActionPreference = 'Stop'

Write-Host ""
Write-Host "=== FSC Takip ERP - VM Guncelleme (DB) ===" -ForegroundColor Cyan
Write-Host ""

$Instance = Read-Host "SQL Server instance adi [varsayilan: localhost\FSCERP]"
if ([string]::IsNullOrWhiteSpace($Instance)) { $Instance = 'localhost\FSCERP' }

$DbName = Read-Host "Veritabani adi [varsayilan: FscErpDb]"
if ([string]::IsNullOrWhiteSpace($DbName)) { $DbName = 'FscErpDb' }

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$sqlFile   = Join-Path $scriptDir 'vm-update-2026-07-19.sql'

if (-not (Test-Path $sqlFile)) {
    Write-Host "HATA: '$sqlFile' bulunamadi. Bu .ps1 dosyasiyla ayni klasorde olmali." -ForegroundColor Red
    exit 1
}

# ── 1) YEDEK ────────────────────────────────────────────────────────────────
$backupDir = Join-Path $env:USERPROFILE "Desktop\FscErpDb_Yedek"
New-Item -ItemType Directory -Force -Path $backupDir | Out-Null
$backupFile = Join-Path $backupDir "FscErpDb_$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"

Write-Host ""
Write-Host "Adim 1/2: Veritabani yedekleniyor -> $backupFile" -ForegroundColor Yellow

$csMaster = "Server=$Instance;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=30"
$connMaster = New-Object System.Data.SqlClient.SqlConnection $csMaster
try {
    $connMaster.Open()
    $cmd = $connMaster.CreateCommand()
    $cmd.CommandTimeout = 300
    $cmd.CommandText = "BACKUP DATABASE [$DbName] TO DISK = N'$backupFile' WITH INIT, COMPRESSION"
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Host "Yedek tamamlandi." -ForegroundColor Green
} catch {
    Write-Host "YEDEKLEME BASARISIZ: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Devam etmeden once bu hatayi cozun (yanlis instance adi olabilir)." -ForegroundColor Red
    $connMaster.Dispose()
    exit 1
} finally {
    if ($connMaster.State -eq 'Open') { $connMaster.Dispose() }
}

# ── 2) MIGRATION SCRIPT ──────────────────────────────────────────────────────
Write-Host ""
Write-Host "Adim 2/2: Guncelleme script'i calistiriliyor..." -ForegroundColor Yellow

$fullScript = Get-Content -Path $sqlFile -Raw
# EF Core script'i "GO" satirlariyla batch'lere ayrilir; SqlCommand tek seferde
# "GO" iceren metni calistiramaz (bu sqlcmd/SSMS'e ozel bir ayirici), bu yuzden
# elle boluyoruz.
$batches = [System.Text.RegularExpressions.Regex]::Split($fullScript, '(?im)^\s*GO\s*$')

$csTarget = "Server=$Instance;Database=$DbName;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=30"
$connTarget = New-Object System.Data.SqlClient.SqlConnection $csTarget
$batchNo = 0
$applied = 0
try {
    $connTarget.Open()
    foreach ($batch in $batches) {
        $batchNo++
        $trimmed = $batch.Trim()
        if ([string]::IsNullOrWhiteSpace($trimmed)) { continue }
        $cmd = $connTarget.CreateCommand()
        $cmd.CommandTimeout = 120
        $cmd.CommandText = $trimmed
        try {
            $cmd.ExecuteNonQuery() | Out-Null
            $applied++
        } catch {
            Write-Host "HATA (batch $batchNo): $($_.Exception.Message)" -ForegroundColor Red
            Write-Host "Yedek dosyasi burada, geri yuklemek isterseniz: $backupFile" -ForegroundColor Yellow
            $connTarget.Dispose()
            exit 1
        }
    }
    Write-Host "Tamamlandi. $applied batch calistirildi." -ForegroundColor Green
} finally {
    if ($connTarget.State -eq 'Open') { $connTarget.Dispose() }
}

Write-Host ""
Write-Host "=== DB guncellemesi bitti. Simdi uygulama dosyalarini kopyalayip IIS'i yeniden baslatin. ===" -ForegroundColor Cyan
