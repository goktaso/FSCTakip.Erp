#requires -Version 5.1
<#
    FSC Takip ERP - Genel guncelleme script'i (her surum icin tekrar kullanilir)

    ONEMLI: Uygulama Program.cs'te "Database:AutoMigrate=true" (musteri kurulumlarinin
    varsayilani) ile ACILISTA KENDI KENDINI MIGRATE EDER. Bu yuzden bu script SADECE:
      1) DB'yi guvenlik icin yedekler (migration something ters giderse geri donulebilsin)
      2) Mevcut uygulama klasorunu yedekler (rollback icin)
      3) IIS site/pool'u durdurur
      4) Yeni yayinlanmis dosyalari uzerine kopyalar (appsettings.json ve license.lic
         DOKUNULMAZ - musteriye ozel, KORUNUR)
      5) IIS site/pool'u baslatir
      6) Siteyi bekler (ilk istekte migration+seed calisir, bu yuzden pay birakilir)

    SSMS/sqlcmd GEREKTIRMEZ. AutoMigrate=false olan (DBA'li) kurulumlarda migration
    ayri ele alinmali - bkz. apply-vm-update.ps1 (tek seferlik/manuel SQL script yolu).

    Kullanim (Yonetici PowerShell):
        cd "<update-paketinin-oldugu-klasor>"
        .\update-engine.ps1

    Paket klasor yapisi (script ile ayni yerde):
        app\              <- dotnet publish ciktisi (yeni surum)
        version.txt       <- yeni surum numarasi (ornek: 1.1.0)
#>

param(
    [string] $InstallPath = 'C:\inetpub\FscErp\app',
    [string] $SiteName    = 'FSC-ERP',
    [string] $AppPoolName = 'FscErpAppPool'
)

$ErrorActionPreference = 'Stop'

function Write-Step($m) { Write-Host "`n=== $m ===" -ForegroundColor Cyan }
function Write-Info($m) { Write-Host "  $m" -ForegroundColor Gray }
function Write-Ok($m)   { Write-Host "  $m" -ForegroundColor Green }
function Write-Err($m)  { Write-Host "  $m" -ForegroundColor Red }

$scriptDir  = Split-Path -Parent $MyInvocation.MyCommand.Path
$packageApp = Join-Path $scriptDir 'app'
$versionTxt = Join-Path $scriptDir 'version.txt'

if (-not (Test-Path $packageApp)) {
    Write-Err "'$packageApp' bulunamadi. Bu script'in yaninda 'app' klasoru (dotnet publish ciktisi) olmali."
    exit 1
}
if (-not (Test-Path $InstallPath)) {
    Write-Err "Kurulu uygulama '$InstallPath' adresinde bulunamadi. -InstallPath parametresiyle dogru yolu verin."
    exit 1
}

$newVersion = if (Test-Path $versionTxt) { (Get-Content $versionTxt -Raw).Trim() } else { 'bilinmiyor' }
$curVerFile = Join-Path $InstallPath 'VERSION.txt'
$curVersion = if (Test-Path $curVerFile) { (Get-Content $curVerFile -Raw).Trim() } else { 'bilinmiyor' }

Write-Host ""
Write-Host "=== FSC Takip ERP - Guncelleme ===" -ForegroundColor Cyan
Write-Host "  Mevcut surum : $curVersion"
Write-Host "  Yeni surum   : $newVersion"
Write-Host ""

if ($curVersion -eq $newVersion -and $newVersion -ne 'bilinmiyor') {
    $ans = Read-Host "Surumler ayni gorunuyor ($newVersion). Yine de devam edilsin mi? (E/H)"
    if ($ans -notmatch '^[EeYy]') { Write-Info 'Iptal edildi.'; exit 0 }
}

# -- 1) DB YEDEK (ADO.NET ile - SSMS/sqlcmd gerekmez) ------------------------
Write-Step '1/6: Veritabani yedekleniyor'
$appsettingsPath = Join-Path $InstallPath 'appsettings.json'
if (-not (Test-Path $appsettingsPath)) {
    Write-Err "appsettings.json bulunamadi ($appsettingsPath). Baglanti bilgisi okunamadi."
    exit 1
}
$appCfg = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
$connStr = $appCfg.ConnectionStrings.DefaultConnection
if ($connStr -match 'Server=([^;]+)') { $sqlInstance = $Matches[1] } else { $sqlInstance = $null }
if ($connStr -match 'Database=([^;]+)') { $dbName = $Matches[1] } else { $dbName = $null }
if (-not $sqlInstance -or -not $dbName) {
    Write-Err "appsettings.json'daki baglanti dizesinden Server/Database okunamadi."
    exit 1
}
Write-Info "Instance: $sqlInstance | DB: $dbName"

$backupDir = Join-Path $env:USERPROFILE "Desktop\FscErpYedek"
New-Item -ItemType Directory -Force -Path $backupDir | Out-Null

$csMaster = "Server=$sqlInstance;Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=30"
$conn = New-Object System.Data.SqlClient.SqlConnection $csMaster
try {
    $conn.Open()

    # BACKUP DATABASE dosyayi PowerShell'i calistiran kullanici degil, SQL Server
    # SERVIS HESABI yazar (ornek: NT SERVICE\MSSQL$FSCERP). Bu hesabin Desktop
    # klasorune yazma izni yoktur ("Operating system error 5 - Access is denied").
    # SQL'in kendi varsayilan yedek klasoru servis hesabinca zaten yazilabilir -
    # onu kullan.
    $cmdPath = $conn.CreateCommand()
    $cmdPath.CommandText = "SELECT CAST(SERVERPROPERTY('InstanceDefaultBackupPath') AS NVARCHAR(400))"
    $sqlBackupDir = $cmdPath.ExecuteScalar()
    if ([string]::IsNullOrWhiteSpace($sqlBackupDir)) {
        throw "SQL Server varsayilan yedek klasoru okunamadi."
    }
    $dbBackupFile = Join-Path $sqlBackupDir "FscErpDb_$(Get-Date -Format 'yyyyMMdd_HHmmss').bak"

    $cmd = $conn.CreateCommand()
    $cmd.CommandTimeout = 300
    # WITH COMPRESSION SQL Express'te desteklenmez (Standard/Enterprise ozelligi) - kullanma.
    $cmd.CommandText = "BACKUP DATABASE [$dbName] TO DISK = N'$dbBackupFile' WITH INIT"
    $cmd.ExecuteNonQuery() | Out-Null
    Write-Ok "DB yedeklendi: $dbBackupFile"
} catch {
    Write-Err "DB yedekleme basarisiz: $($_.Exception.Message)"
    exit 1
} finally {
    $conn.Dispose()
}

# appcmd.exe kullan - WebAdministration modulunun Stop/Start-WebItem cmdlet'leri
# bazi sunucularda COM sinifi kayit hatasi verebiliyor ("Class not registered",
# CLSID 688EEEE5-...). appcmd native bir exe, bu COM bagimliligina ihtiyac duymaz.
$appcmd = Join-Path $env:windir 'System32\inetsrv\appcmd.exe'

# -- 2) SITE DURDUR ------------------------------------------------------------
# Uygulama klasoru yedeginden ONCE durdurulmali: site calisirken w3wp.exe
# logs\*.log gibi dosyalari acik/kilitli tutar, Compress-Archive bu dosyalarda
# "being used by another process" hatasi verir (2026-07-19 VM testinde bulundu -
# yedek kismen alinmis olabiliyordu, sansla script durmadi ama yedek eksikti).
Write-Step '2/6: IIS site durduruluyor'
& $appcmd stop site "$SiteName" | Out-Null
& $appcmd stop apppool "$AppPoolName" | Out-Null
Start-Sleep -Seconds 3
Write-Ok 'Site durduruldu.'

# -- 3) UYGULAMA KLASORU YEDEK ------------------------------------------------
Write-Step '3/6: Mevcut uygulama klasoru yedekleniyor'
$appBackupZip = Join-Path $backupDir "AppFolder_$(Get-Date -Format 'yyyyMMdd_HHmmss').zip"
Compress-Archive -Path (Join-Path $InstallPath '*') -DestinationPath $appBackupZip -Force
Write-Ok "Uygulama klasoru yedeklendi: $appBackupZip"

# -- 4) DOSYALARI KOPYALA (musteriye ozel dosyalar KORUNUR) ------------------
Write-Step '4/6: Yeni dosyalar kopyalaniyor (appsettings.json ve license.lic korunuyor)'
try {
    # robocopy /MIR KULLANILMIYOR - hedefte olup kaynakta olmayan dosyalar (ornek:
    # musterinin appsettings.json'u, license.lic'i) SILINMESIN diye. Sadece
    # ekleme/uzerine-yazma yapilir.
    $roboArgs = @(
        $packageApp, $InstallPath,
        '/E',                                   # alt klasorler dahil (bos olanlar da)
        '/XF', 'appsettings.json', 'license.lic',
        '/NFL', '/NDL', '/NJH', '/NJS'
    )
    robocopy @roboArgs | Out-Null
    # robocopy exit code 0-7 basaridir (8+ hata); PowerShell $LASTEXITCODE'u kontrol et
    if ($LASTEXITCODE -ge 8) { throw "robocopy hata kodu: $LASTEXITCODE" }
    Write-Ok 'Dosyalar kopyalandi.'
} catch {
    Write-Err "Dosya kopyalama basarisiz: $($_.Exception.Message)"
    Write-Info "Geri almak icin: Site'i durdurun, '$appBackupZip' icerigini '$InstallPath' uzerine acin."
    exit 1
}

if ($newVersion -ne 'bilinmiyor') {
    Set-Content -Path $curVerFile -Value $newVersion -Encoding UTF8
}

# -- 5) SITE BASLAT ------------------------------------------------------------
Write-Step '5/6: IIS site baslatiliyor'
& $appcmd start apppool "$AppPoolName" | Out-Null
& $appcmd start site "$SiteName" | Out-Null
Write-Ok 'Site baslatildi.'

# -- 6) SAGLIK KONTROLU (ilk istek migration+seed calistirir, pay birak) -----
Write-Step '6/6: Site dogrulaniyor (ilk istek DB migration calistirabilir, bekleniyor...)'
$bindingPort = 8080
try {
    # appcmd list site ciktisindan port'u regex ile cek - Get-WebBinding ayni COM
    # provider'a bagimli, o da bozuk olabilir.
    $siteInfo = & $appcmd list site "$SiteName"
    if ($siteInfo -match ':(\d+):') { $bindingPort = $Matches[1] }
} catch { }

$url = "http://localhost:$bindingPort/"
$deadline = (Get-Date).AddMinutes(3)
$ok = $false
while ((Get-Date) -lt $deadline) {
    try {
        $r = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 30 -ErrorAction Stop
        Write-Ok "Yanit: HTTP $($r.StatusCode) - site ayakta."
        $ok = $true
        break
    } catch {
        $resp = $null
        try { $resp = $_.Exception.Response } catch { }
        if ($resp) {
            $code = [int] $resp.StatusCode
            if ($code -ge 200 -and $code -lt 500) {
                Write-Ok "Yanit: HTTP $code - site ayakta (login/lisans yonlendirmesi olabilir)."
                $ok = $true
                break
            }
        }
        Start-Sleep -Seconds 5
    }
}

Write-Host ""
if ($ok) {
    Write-Host "=== GUNCELLEME TAMAMLANDI ($curVersion -> $newVersion) ===" -ForegroundColor Green
    Write-Host "DB yedegi : $dbBackupFile"
    Write-Host "App yedegi: $appBackupZip"
} else {
    Write-Err "Site $url adresinde yanit vermedi. stdout loglarini kontrol edin: $InstallPath\logs\"
    Write-Info "Geri almak icin: Site'i durdurun, DB'yi '$dbBackupFile' ile geri yukleyin, '$appBackupZip' icerigini '$InstallPath' uzerine acin."
    exit 1
}

