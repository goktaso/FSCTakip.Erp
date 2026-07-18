<#
.SYNOPSIS
    FSC Takip ERP — kurulum motoru. Ön koşullar, SQL, IIS, şema ve doğrulama.

.DESCRIPTION
    FscErpSetup.exe (Inno Setup) bu scripti çağırır; elle de çalıştırılabilir.
    Idempotent: ikinci çalıştırma güncelleme yapar, mevcut kurulumu bozmaz.
    appsettings.json ve license.lic asla ezilmez.

    Adım sırası docs/ThirdPartyKurulum.md FAZ 1-4'ten birebir alınmıştır; sırası
    keyfi değildir (IIS'ten önce kurulan Hosting Bundle 500.30 verir).

.PARAMETER SqlInstance
    Kullanılacak mevcut SQL örneği (ör. "localhost\SQLEXPRESS"). Boş bırakılırsa
    SQL Server Express kurulur (FAZ 2A); doluysa yalnız veritabanı eklenir (FAZ 2B).

.EXAMPLE
    .\install-engine.ps1 -SourcePath .\app -Port 8080
.EXAMPLE
    .\install-engine.ps1 -Mode Uninstall
#>
[CmdletBinding()]
param(
    [ValidateSet('Install', 'Uninstall')]
    [string] $Mode = 'Install',

    # Yayınlanmış uygulama dosyaları (dotnet publish çıktısı)
    [string] $SourcePath,

    # Ön koşul yükleyicileri (SQLEXPR_x64_ENU.exe, dotnet-hosting-*.exe)
    [string] $PrereqPath,

    [string] $InstallPath = 'C:\inetpub\FscErp\app',
    [string] $DataPath    = '',
    [int]    $Port        = 0,
    [string] $SqlInstance = '',
    [string] $Database    = 'FscErpDb',
    [string] $SiteName    = 'FSC-ERP',
    [string] $AppPoolName = 'FscErpAppPool',

    # Kaldırmada veritabanını da sil (varsayılan: veri korunur)
    [switch] $PurgeData
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$script:StepNo = 0
$script:MarkerPath = Join-Path $env:ProgramData 'ArdFscErp\.init'

# ─────────────────────────────────────────────────────────────────────────────
# Yardımcılar
# ─────────────────────────────────────────────────────────────────────────────

function Write-Step {
    param([string] $Message)
    $script:StepNo++
    # Inno bu satırları yakalayıp ilerleme etiketine basar (bkz. FscErpSetup.iss)
    Write-Host ("[ADIM {0}] {1}" -f $script:StepNo, $Message)
}

function Write-Info { param([string] $M) Write-Host "         $M" }
function Write-Warn { param([string] $M) Write-Host "  [!]    $M" -ForegroundColor Yellow }

function Assert-Administrator {
    $id = [Security.Principal.WindowsIdentity]::GetCurrent()
    $pr = New-Object Security.Principal.WindowsPrincipal($id)
    if (-not $pr.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
        throw 'Bu script yönetici olarak çalıştırılmalıdır.'
    }
}

function Get-MachineKey {
    # LicenseService.GetMachineKey() ile AYNI algoritma olmak zorunda:
    # SHA256(MachineGuid) ilk 16 hex karakter, küçük harf.
    try {
        $guid = (Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Cryptography' -Name MachineGuid).MachineGuid
    } catch {
        $guid = $env:COMPUTERNAME
    }
    $sha   = [Security.Cryptography.SHA256]::Create()
    $bytes = $sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($guid))
    $hex   = -join ($bytes | ForEach-Object { $_.ToString('x2') })
    return $hex.Substring(0, 16)
}

function Get-FirstFile {
    param([string] $Directory, [string] $Pattern, [string] $FriendlyName)
    if (-not $Directory -or -not (Test-Path $Directory)) {
        throw "$FriendlyName bulunamadı: ön koşul klasörü yok ($Directory). Paket eksik derlenmiş olabilir."
    }
    $file = Get-ChildItem -Path $Directory -Filter $Pattern -File -ErrorAction SilentlyContinue |
            Select-Object -First 1
    if (-not $file) {
        throw "$FriendlyName bulunamadı: $Directory içinde '$Pattern' yok."
    }
    return $file.FullName
}

function Invoke-Installer {
    param([string] $Path, [string[]] $Arguments, [string] $FriendlyName)
    Write-Info "$FriendlyName çalıştırılıyor (birkaç dakika sürebilir)..."
    $p = Start-Process -FilePath $Path -ArgumentList $Arguments -Wait -PassThru -NoNewWindow
    if ($p.ExitCode -eq 3010) {
        Write-Warn "$FriendlyName kuruldu ancak yeniden başlatma istiyor. Kurulum sürüyor; bitince sunucuyu yeniden başlatın."
        return
    }
    if ($p.ExitCode -ne 0) {
        throw "$FriendlyName kurulumu başarısız (çıkış kodu $($p.ExitCode)). Ayrıntı için ilgili yükleyicinin kendi log'una bakın."
    }
}

# ─────────────────────────────────────────────────────────────────────────────
# SQL erişimi — SqlServer/SqlPS modülüne bağımlı olmadan (her Windows'ta var)
# ─────────────────────────────────────────────────────────────────────────────

function Invoke-Sql {
    # NOT: parametre adı "Db" OLAMAZ — [Parameter(Mandatory)] kullanan her fonksiyon
    # örtük "advanced function" olur ve PowerShell'in yerleşik ortak parametrelerini
    # (Verbose/Debug/ErrorAction...) kazanır. -Debug'ın gizli takma adı "-db"dir;
    # "Db" adlı bir parametre bu takma adla çakışıp HER çağrıda (açıkça -Db verilmese
    # bile) "parameter alias" hatası fırlatır. Saatlerce "SQL'e bağlanılamıyor" gibi
    # görünen bug'ın gerçek kök nedeni buydu (VM testi 2026-07-18).
    param(
        [Parameter(Mandatory)] [string] $Instance,
        [Parameter(Mandatory)] [string] $Query,
        [string] $DbName = 'master',
        [int]    $TimeoutSec = 60
    )
    $cs = "Server=$Instance;Database=$DbName;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=15"
    $conn = New-Object System.Data.SqlClient.SqlConnection $cs
    try {
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandText    = $Query
        $cmd.CommandTimeout = $TimeoutSec
        return $cmd.ExecuteScalar()
    } finally {
        $conn.Dispose()
    }
}

function Test-SqlConnection {
    param([string] $Instance)
    try {
        $null = Invoke-Sql -Instance $Instance -Query 'SELECT 1'
        $script:LastSqlError = $null
        return $true
    } catch {
        # Gerçek .NET hatasını sakla — çağıran taraf (retry döngüleri) son denemede
        # bunu hata mesajına ekler. Önceden bu bilgi tamamen kayboluyordu (true/false'a
        # indirgeniyordu), bu yüzden servisler "Running" görünse bile asıl sebep hiç
        # görünmüyordu.
        $script:LastSqlError = $_.Exception.Message
        return $false
    }
}

function Get-SqlInstances {
    $key = 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL'
    if (-not (Test-Path $key)) { return @() }
    $props = Get-ItemProperty $key
    return $props.PSObject.Properties |
           Where-Object { $_.Name -notlike 'PS*' } |
           ForEach-Object {
               if ($_.Name -eq 'MSSQLSERVER') { 'localhost' } else { "localhost\$($_.Name)" }
           }
}

# ─────────────────────────────────────────────────────────────────────────────
# Kurulum adımları
# ─────────────────────────────────────────────────────────────────────────────

function Enable-IisFeatures {
    Write-Step 'IIS bileşenleri etkinleştiriliyor'

    # NOT: Hosting Bundle'dan ÖNCE gelmeli. Ters sırada ASP.NET Core modülü IIS'e
    # kayıt olmaz ve site ilk açılışta HTTP 500.30 verir (ThirdPartyKurulum FAZ 3.2/1).
    $features = @(
        'IIS-WebServerRole', 'IIS-WebServer', 'IIS-CommonHttpFeatures',
        'IIS-StaticContent', 'IIS-DefaultDocument', 'IIS-HttpErrors', 'IIS-HttpRedirect',
        'IIS-ApplicationDevelopment', 'IIS-NetFxExtensibility45', 'IIS-ISAPIExtensions',
        'IIS-ISAPIFilter', 'IIS-HttpCompressionStatic', 'IIS-ApplicationInit',
        'IIS-WebServerManagementTools', 'IIS-ManagementScriptingTools'  # sonuncusu WebAdministration modülü için
    )

    $needed = @()
    foreach ($f in $features) {
        $state = Get-WindowsOptionalFeature -Online -FeatureName $f -ErrorAction SilentlyContinue
        if ($state -and $state.State -ne 'Enabled') { $needed += $f }
    }

    if ($needed.Count -eq 0) {
        Write-Info 'IIS bileşenleri zaten etkin.'
    } else {
        Write-Info "Etkinleştirilecek: $($needed -join ', ')"
        Enable-WindowsOptionalFeature -Online -FeatureName $needed -All -NoRestart | Out-Null
    }

    Import-Module WebAdministration -ErrorAction Stop
}

function Install-HostingBundle {
    Write-Step '.NET 8 Hosting Bundle denetleniyor'

    $ancm = Join-Path $env:ProgramFiles 'IIS\Asp.Net Core Module\V2\aspnetcorev2.dll'
    $runtimeOk = $false
    try {
        $runtimeOk = (& dotnet --list-runtimes 2>$null) -match 'Microsoft\.AspNetCore\.App 8\.'
    } catch {
        $runtimeOk = $false
    }

    if ((Test-Path $ancm) -and $runtimeOk) {
        Write-Info 'Hosting Bundle kurulu ve IIS modülü kayıtlı.'
        return
    }

    $installer = Get-FirstFile -Directory $PrereqPath -Pattern 'dotnet-hosting-*win.exe' -FriendlyName '.NET 8 Hosting Bundle'

    if ($runtimeOk -and -not (Test-Path $ancm)) {
        # Runtime var ama IIS modülü yok = bundle IIS'ten önce kurulmuş. Onarım şart.
        Write-Warn 'Hosting Bundle IIS''ten önce kurulmuş — onarılıyor.'
        Invoke-Installer -Path $installer -Arguments @('/repair', '/quiet', '/norestart') -FriendlyName 'Hosting Bundle onarımı'
    } else {
        Invoke-Installer -Path $installer -Arguments @('/install', '/quiet', '/norestart') -FriendlyName 'Hosting Bundle'
    }

    if (-not (Test-Path $ancm)) {
        throw 'Hosting Bundle kuruldu ancak ASP.NET Core IIS modülü bulunamadı. Sunucuyu yeniden başlatıp kurulumu tekrar çalıştırın.'
    }
}

function Install-SqlExpress {
    Write-Step 'SQL Server Express kuruluyor'

    $installer = Get-FirstFile -Directory $PrereqPath -Pattern 'SQLEXPR*.exe' -FriendlyName 'SQL Server Express'

    # Collation kurulumdan sonra değiştirilemez — Turkish_CI_AS burada belirlenir.
    # (Kullanıcı adları tr-TR kurallarıyla büyük harfe çevrilip saklanıyor; farklı
    #  collation'da oturum açma eşleşmesi bozulur. ThirdPartyKurulum FAZ 2A/4.)
    $setupArgs = @(
        '/Q', '/IACCEPTSQLSERVERLICENSETERMS', '/ACTION=Install',
        '/FEATURES=SQLENGINE', '/INSTANCENAME=FSCERP',
        '/SQLCOLLATION=Turkish_CI_AS',
        '/SQLSYSADMINACCOUNTS=BUILTIN\Administrators',
        '/TCPENABLED=1', '/UPDATEENABLED=0',
        '/SQLSVCSTARTUPTYPE=Automatic', '/BROWSERSVCSTARTUPTYPE=Automatic'
    )
    Invoke-Installer -Path $installer -Arguments $setupArgs -FriendlyName 'SQL Server Express'

    # Named instance keşfi (sabit port yok) SQL Server Browser servisine (UDP 1434)
    # bağımlı. Setup.exe /BROWSERSVCSTARTUPTYPE=Automatic ile yalnız başlangıç türünü
    # ayarlıyor, fiilen başlattığı garanti değil — burada açıkça başlatıp doğruluyoruz.
    Set-Service -Name SQLBrowser -StartupType Automatic -ErrorAction SilentlyContinue
    Start-Service -Name SQLBrowser -ErrorAction SilentlyContinue

    $instance = 'localhost\FSCERP'
    $deadline = (Get-Date).AddMinutes(2)
    while (-not (Test-SqlConnection -Instance $instance)) {
        if ((Get-Date) -gt $deadline) {
            $diag = Get-Service -Name 'SQLBrowser', 'MSSQL$FSCERP' -ErrorAction SilentlyContinue |
                    ForEach-Object { "$($_.Name)=$($_.Status)" }
            $diagText = if ($diag) { $diag -join ', ' } else { 'servis durumu okunamadı' }
            throw "SQL Express kuruldu ancak $instance bağlantısı kurulamadı. Servis durumu: $diagText. Son hata: $script:LastSqlError"
        }
        Start-Sleep -Seconds 5
    }
    Write-Info "SQL Express hazır: $instance"
    return $instance
}

function Resolve-SqlInstance {
    if ($SqlInstance) {
        Write-Step "Mevcut SQL örneği kullanılıyor: $SqlInstance"

        # Tek seferlik deneme yeterli değil: sunucu yeni açılmışsa servis "Running"
        # görünse bile motor henüz bağlantı kabul etmiyor olabilir (bkz. Install-SqlExpress
        # içindeki aynı desen). 60 saniye, 5 saniye aralıkla dene.
        $deadline = (Get-Date).AddSeconds(60)
        while (-not (Test-SqlConnection -Instance $SqlInstance)) {
            if ((Get-Date) -gt $deadline) {
                $svcName = "MSSQL`$$($SqlInstance.Split('\')[-1])"
                $diag = Get-Service -Name 'SQLBrowser', $svcName -ErrorAction SilentlyContinue |
                        ForEach-Object { "$($_.Name)=$($_.Status)" }
                $diagText = if ($diag) { $diag -join ', ' } else { 'servis bulunamadı (adı yanlış olabilir)' }
                throw "SQL örneğine bağlanılamadı: $SqlInstance. Örnek adını ve Windows kimlik doğrulama yetkinizi denetleyin. Servis durumu: $diagText. Son hata: $script:LastSqlError"
            }
            Start-Sleep -Seconds 5
        }
        return $SqlInstance
    }

    $existing = @(Get-SqlInstances)
    if ($existing.Count -gt 0) {
        Write-Info "Sunucuda SQL örneği bulundu: $($existing -join ', ') — kullanmak için -SqlInstance ile belirtin."
    }

    # FSCERP adında bir örnek zaten kayıtlıysa (ör. önceki yarım kalan bir kurulum
    # denemesinden) körü körüne yeniden "Install" çağırmadan önce sağlığını kontrol et.
    if ($existing -contains 'localhost\FSCERP') {
        if (Test-SqlConnection -Instance 'localhost\FSCERP') {
            Write-Info 'localhost\FSCERP zaten sağlıklı — yeniden kurulum atlanıyor.'
            return 'localhost\FSCERP'
        }
        Write-Warn 'localhost\FSCERP kayıtlı ama bağlanılamıyor — bozuk/yarım kalıntı olabilir, temizlenip yeniden kurulacak.'
        $installer = Get-FirstFile -Directory $PrereqPath -Pattern 'SQLEXPR*.exe' -FriendlyName 'SQL Server Express'
        $uninstallArgs = @('/Q', '/ACTION=Uninstall', '/INSTANCENAME=FSCERP', '/FEATURES=SQLENGINE')
        try {
            Invoke-Installer -Path $installer -Arguments $uninstallArgs -FriendlyName 'SQL Server Express (bozuk örnek temizliği)'
        } catch {
            Write-Warn "Bozuk örnek temizliği tam başarılı olmadı, yine de yeniden kurulum denenecek: $($_.Exception.Message)"
        }
    }

    return (Install-SqlExpress)
}

function New-FscDatabase {
    param([string] $Instance)
    Write-Step "Veritabanı hazırlanıyor: $Database"

    $exists = Invoke-Sql -Instance $Instance -Query "SELECT DB_ID(N'$Database')"
    if ($exists -isnot [DBNull] -and $null -ne $exists) {
        $coll = Invoke-Sql -Instance $Instance -Query "SELECT CONVERT(nvarchar(128), DATABASEPROPERTYEX(N'$Database','Collation'))"
        Write-Info "Veritabanı zaten var (collation: $coll) — korunuyor."
        if ($coll -ne 'Turkish_CI_AS') {
            Write-Warn "Collation Turkish_CI_AS değil ($coll). Türkçe kullanıcı adlarında oturum açma sorunu çıkabilir (ThirdPartyKurulum EK A)."
        }
        return
    }

    # Sunucu genelinin collation'ı ne olursa olsun veritabanı kendi collation'ını alır;
    # aynı sunucudaki diğer veritabanları etkilenmez (FAZ 2B).
    Invoke-Sql -Instance $Instance -Query "CREATE DATABASE [$Database] COLLATE Turkish_CI_AS" | Out-Null
    Write-Info "Veritabanı oluşturuldu (COLLATE Turkish_CI_AS)."
}

function Grant-AppPoolSqlAccess {
    param([string] $Instance)
    Write-Step 'Uygulama havuzuna veritabanı yetkisi veriliyor'

    # Yeni bir IIS App Pool'un SQL'de varsayılan olarak HİÇBİR erişimi yoktur;
    # bu adım atlanırsa ilk açılışta "Cannot open database" (SQL 4060) alınır.
    $login = "IIS APPPOOL\$AppPoolName"
    $sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'$login')
    CREATE LOGIN [$login] FROM WINDOWS;
"@
    try {
        Invoke-Sql -Instance $Instance -Query $sql | Out-Null

        $sqlUser = @"
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'$login')
    CREATE USER [$login] FOR LOGIN [$login];
ALTER ROLE db_owner ADD MEMBER [$login];
"@
        Invoke-Sql -Instance $Instance -Query $sqlUser -DbName $Database | Out-Null
        Write-Info "$login → $Database db_owner"
    } catch {
        throw @"
Uygulama havuzuna SQL yetkisi verilemedi: $($_.Exception.Message)

Bu adım sysadmin yetkisi ister. Paylaşılan/kurumsal bir SQL sunucusu kullanıyorsanız
DBA'nızdan aşağıdakini çalıştırmasını isteyin, sonra kurulumu tekrar başlatın:

  CREATE LOGIN [$login] FROM WINDOWS;
  USE [$Database];
  CREATE USER [$login] FOR LOGIN [$login];
  ALTER ROLE db_owner ADD MEMBER [$login];
"@
    }
}

function Resolve-DataPath {
    if ($DataPath) { return $DataPath }
    # Belge arşivi uygulama klasörünün DIŞINDA durur ki sürüm güncellemesi ezmesin.
    # Ayrı SABİT veri diski (D:) varsa tercih edilir — ama YALNIZ DriveType=3 (yerel sabit
    # disk). DVD (5) / removable (2) / network (4) KABUL EDİLMEZ: ISO'dan kurulumda D:
    # DVD sürücüsü olabilir ve oraya yazmak "Access denied" verir (VM testi 2026-07-18).
    $dFixed = Get-CimInstance Win32_LogicalDisk -Filter "DeviceID='D:' AND DriveType=3" -ErrorAction SilentlyContinue
    if ($dFixed) { return 'D:\FscErpData' }
    return 'C:\FscErpData'
}

function New-DataFolders {
    param([string] $Root)
    Write-Step "Veri klasörleri hazırlanıyor: $Root"

    foreach ($sub in @('uploads', 'backup', 'logs')) {
        $p = Join-Path $Root $sub
        if (-not (Test-Path $p)) { New-Item -ItemType Directory -Path $p -Force | Out-Null }
    }

    $identity = "IIS APPPOOL\$AppPoolName"
    $acl  = Get-Acl $Root
    $rule = New-Object System.Security.AccessControl.FileSystemAccessRule(
        $identity, 'Modify', 'ContainerInherit,ObjectInherit', 'None', 'Allow')
    $acl.SetAccessRule($rule)
    Set-Acl -Path $Root -AclObject $acl
    Write-Info "$identity → Modify"
}

function Set-AppFiles {
    Write-Step 'Uygulama dosyaları kopyalanıyor'

    if (-not (Test-Path $SourcePath)) { throw "Uygulama kaynak klasörü bulunamadı: $SourcePath" }
    if (-not (Test-Path $InstallPath)) { New-Item -ItemType Directory -Path $InstallPath -Force | Out-Null }

    # Müşteriye özel iki dosya güncellemede KORUNUR (ThirdPartyKurulum FAZ 3.6).
    $protected = @('appsettings.json', 'license.lic')
    $exclude = $protected | Where-Object { Test-Path (Join-Path $InstallPath $_) }

    if ($exclude) { Write-Info "Korunuyor (üzerine yazılmıyor): $($exclude -join ', ')" }

    $roboArgs = @($SourcePath, $InstallPath, '/MIR', '/NFL', '/NDL', '/NJH', '/NJS', '/NP', '/R:2', '/W:2')
    # /MIR hedefte olup kaynakta olmayanı siler — korunacak dosyaları hariç tut.
    foreach ($f in $protected) { $roboArgs += @('/XF', $f) }
    # logs/ klasörü uygulamanın kendi stdout log'unu tutar; aynalamada silinmesin.
    $roboArgs += @('/XD', 'logs')

    & robocopy.exe @roboArgs | Out-Null
    # Robocopy 0-7 arası başarı, 8+ hata anlamına gelir.
    if ($LASTEXITCODE -ge 8) { throw "Dosya kopyalama başarısız (robocopy $LASTEXITCODE)." }
    Write-Info "$InstallPath güncellendi."
}

function Set-AppSettings {
    param([string] $Instance, [string] $Root)
    Write-Step 'appsettings.json yapılandırılıyor'

    $target = Join-Path $InstallPath 'appsettings.json'
    if (Test-Path $target) {
        Write-Info 'Mevcut appsettings.json korunuyor (güncelleme modu).'
        return
    }

    # ConvertTo-Json ters slash'ları kendisi kaçırır — named instance'ta
    # "localhost\FSCERP" JSON'a "localhost\\FSCERP" olarak yazılır (elle yazımda sık hata).
    $cfg = [ordered]@{
        ConnectionStrings = [ordered]@{
            DefaultConnection = "Server=$Instance;Database=$Database;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
            NetsisConnection  = ''
        }
        Database = [ordered]@{
            # İlk açılışta 34 migration uygulanır (Program.cs). DBA'lı kurulumda
            # false yapılıp migration.sql elle çalıştırılabilir.
            AutoMigrate = $true
        }
        Seed = [ordered]@{
            # Müşteri kurulumu boş gelir: sadece admin + şirket kaydı + FSC tipleri.
            # Örnek tedarikçi/müşteri/lot/sipariş BASILMAZ (yalnız geliştirmede true).
            DemoData = $false
        }
        Security = [ordered]@{
            # Intranet kurulumu HTTP üzerinden yayında — HTTPS'e yönlendirme
            # kullanıcıyı ERR_SSL_PROTOCOL_ERROR'a düşürür.
            HttpsRedirection = $false
        }
        Logging = [ordered]@{
            LogLevel = [ordered]@{
                Default                = 'Information'
                'Microsoft.AspNetCore' = 'Warning'
            }
        }
        AllowedHosts   = '*'
        EtlBackground  = [ordered]@{ Enabled = $false; IntervalMinutes = 60 }
        FileStorage    = [ordered]@{
            Root    = (Join-Path $Root 'uploads')
            Folders = [ordered]@{ Invoice = 'invoices'; Dispatch = 'dispatches'; Other = 'other' }
        }
    }

    $json = $cfg | ConvertTo-Json -Depth 6
    # ASP.NET Core yapılandırmayı UTF-8 bekler; PS 5.1 varsayılanı UTF-16'dır.
    [IO.File]::WriteAllText($target, $json, (New-Object Text.UTF8Encoding($false)))
    Write-Info "Bağlantı: Server=$Instance;Database=$Database"
}

function Set-IisSite {
    Write-Step 'IIS sitesi yapılandırılıyor'

    if (-not (Test-Path "IIS:\AppPools\$AppPoolName")) {
        New-WebAppPool -Name $AppPoolName | Out-Null
    }
    # .NET CLR = "No Managed Code": ASP.NET Core kendi runtime'ını taşır, IIS yalnız ters vekil.
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name managedRuntimeVersion -Value ''
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name startMode             -Value 'AlwaysRunning'
    Set-ItemProperty "IIS:\AppPools\$AppPoolName" -Name autoStart             -Value $true

    if (-not (Test-Path "IIS:\Sites\$SiteName")) {
        New-Website -Name $SiteName -PhysicalPath $InstallPath -ApplicationPool $AppPoolName -Port $Port | Out-Null
        Write-Info "Site oluşturuldu: $SiteName → :$Port"
    } else {
        Set-ItemProperty "IIS:\Sites\$SiteName" -Name physicalPath -Value $InstallPath
        Set-ItemProperty "IIS:\Sites\$SiteName" -Name applicationPool -Value $AppPoolName
        Get-WebBinding -Name $SiteName | Remove-WebBinding
        New-WebBinding -Name $SiteName -Protocol http -Port $Port -IPAddress '*'
        Write-Info "Site güncellendi: $SiteName → :$Port"
    }
}

function Set-FirewallRule {
    Write-Step "Güvenlik duvarı kuralı ekleniyor (TCP $Port)"

    # IIS binding'i "Tümü Atanmamış" olsa bile inbound kural yoksa sunucunun KENDİSİ
    # dışında hiçbir istemci bağlanamaz — ve localhost testi bunu yakalamaz.
    $name = "FSC ERP HTTP ($Port)"
    $existing = Get-NetFirewallRule -DisplayName $name -ErrorAction SilentlyContinue
    if ($existing) {
        Write-Info 'Kural zaten var.'
        return
    }
    New-NetFirewallRule -DisplayName $name -Direction Inbound -Protocol TCP `
        -LocalPort $Port -Action Allow -Profile Any | Out-Null
    Write-Info "$name eklendi."
}

function Set-TrialMarker {
    # Deneme başlangıcı. LicenseService bunu DB oluşturma tarihiyle birlikte okur ve
    # ERKEN olanı esas alır; güncellemede üzerine yazmak süreyi sıfırlardı.
    if (Test-Path $script:MarkerPath) { return }
    $dir = Split-Path $script:MarkerPath -Parent
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
    [IO.File]::WriteAllText($script:MarkerPath, (Get-Date).ToUniversalTime().ToString('o'))
    (Get-Item $script:MarkerPath -Force).Attributes = 'Hidden'
}

function Test-SiteResponds {
    Write-Step 'Site başlatılıyor ve doğrulanıyor'

    Start-WebAppPool -Name $AppPoolName -ErrorAction SilentlyContinue
    Start-Website   -Name $SiteName    -ErrorAction SilentlyContinue

    # İlk istek 34 migration + seed çalıştırır; cömert bekle.
    $url      = "http://localhost:$Port/"
    $deadline = (Get-Date).AddMinutes(3)
    $lastErr  = ''

    while ((Get-Date) -lt $deadline) {
        try {
            # NOT: -MaximumRedirection 0 KULLANMA — Windows PowerShell 5.1'in .NET
            # Framework tabanlı Invoke-WebRequest'i bu değerle "Operation is not valid
            # due to the current state of the object" fırlatır; sitenin durumuyla
            # ilgisi yok, salt parametre kombinasyonunun kendi hatası (VM testi
            # 2026-07-18). Yönlendirmeler serbestçe takip edilsin — login/lisans
            # sayfasına düşüp 200 dönmesi zaten "site ayakta" demektir.
            $r = Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 30 -ErrorAction Stop
            Write-Info "Yanıt: HTTP $($r.StatusCode)"
            return
        } catch {
            $resp = $null
            try { $resp = $_.Exception.Response } catch { }
            if ($resp) {
                $code = [int] $resp.StatusCode
                if ($code -ge 200 -and $code -lt 500) {
                    # 3xx/4xx dahi olsa IIS + uygulama ayaktadır (ör. lisans/login yönlendirmesi).
                    Write-Info "Yanıt: HTTP $code (site ayakta)"
                    return
                }
                $lastErr = "HTTP $code"
            } else {
                $lastErr = $_.Exception.Message
            }
            Start-Sleep -Seconds 5
        }
    }

    throw @"
Site $url adresinde yanıt vermedi. Son hata: $lastErr

Tanı için:
  1. $InstallPath\logs\stdout_*.log dosyasındaki en yeni kaydı okuyun.
  2. Sık nedenler: SQL yetkisi (Cannot open database), Hosting Bundle IIS'ten sonra
     kurulmamış (HTTP 500.30), bağlantı dizesindeki örnek adı yanlış.
  Ayrıntı: docs/ThirdPartyKurulum.md EK A.
"@
}

function Write-InstallReport {
    param([string] $Instance, [string] $Root)
    Write-Step 'Kurulum raporu yazılıyor'

    $ip = (Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue |
           Where-Object { $_.InterfaceAlias -notmatch 'Loopback' -and $_.IPAddress -ne '127.0.0.1' } |
           Select-Object -First 1 -ExpandProperty IPAddress)
    if (-not $ip) { $ip = $env:COMPUTERNAME }

    $url        = "http://${ip}:$Port"
    $machineKey = Get-MachineKey
    $trialEnds  = ([DateTime]::Parse((Get-Content $script:MarkerPath -Raw))).AddDays(30).ToString('dd.MM.yyyy')

    $report = @"
FSC TAKİP ERP — KURULUM RAPORU
ARD Sistem ve Danışmanlık
Tarih: $(Get-Date -Format 'dd.MM.yyyy HH:mm')

─── ERİŞİM ────────────────────────────────────────────────
Adres        : $url
               (Adres çubuğuna "http://" ve port dahil yazılmalıdır. Yalnız
                IP yazılırsa tarayıcı HTTPS dener ve ERR_SSL_PROTOCOL_ERROR verir.)
Kullanıcı    : admin
Parola       : admin123

  >>> İLK İŞ: Sağ üst menü → Şifre Değiştir. Varsayılan parola ile bırakmayın. <<<

─── LİSANS ────────────────────────────────────────────────
Durum             : 30 günlük deneme
Deneme bitişi     : $trialEnds
Sunucu kimlik kodu: $machineKey

Kalıcı lisans için yukarıdaki kimlik kodunu ARD Sistem ve Danışmanlık'a iletin.
Gelen license.lic dosyası $url/License/Status ekranından yüklenir.
Süre dolduğunda sistem erişime kapanır; VERİLER KORUNUR.

─── SİSTEM ────────────────────────────────────────────────
Uygulama     : $InstallPath
Veri/belge   : $Root
SQL örneği   : $Instance
Veritabanı   : $Database (COLLATE Turkish_CI_AS)
IIS sitesi   : $SiteName (havuz: $AppPoolName, port: $Port)
Kurulum log'u: $Root\logs\

─── SONRAKİ ADIMLAR ───────────────────────────────────────
1. Şirket Bilgileri girin (Tanımlamalar → Şirket Bilgileri) — belgelerde firma
   ünvanınız ve FSC kodlarınız basılır. Girilmezse varsayılan ARD ünvanı çıkar.
2. Ana veri: FSC tipleri, ürün grupları, depo/makine, tedarikçi (FSC kodlarıyla).
3. Yedekleme planı kurun — $Root\backup ve veritabanı birlikte yedeklenmelidir.
4. Ağdaki BAŞKA bir bilgisayardan $url adresini açarak erişimi doğrulayın.

Ayrıntılı kılavuz: $url/Guide
"@

    $path = Join-Path $Root 'kurulum-raporu.txt'
    [IO.File]::WriteAllText($path, $report, (New-Object Text.UTF8Encoding($true)))

    # Masaüstü kısayolu (tüm kullanıcılar)
    $lnk = Join-Path ([Environment]::GetFolderPath('CommonDesktopDirectory')) 'FSC Takip ERP.url'
    [IO.File]::WriteAllText($lnk, "[InternetShortcut]`r`nURL=$url`r`n")

    Write-Host ''
    Write-Host $report
    Write-Host ''
    Write-Info "Rapor kaydedildi: $path"

    # Inno son sayfasında göstermek için makine tarafından okunabilir çıktı
    Write-Host "RESULT_URL=$url"
    Write-Host "RESULT_MACHINEKEY=$machineKey"
    Write-Host "RESULT_REPORT=$path"
}

# ─────────────────────────────────────────────────────────────────────────────
# Kaldırma
# ─────────────────────────────────────────────────────────────────────────────

function Invoke-Uninstall {
    Write-Step 'FSC Takip ERP kaldırılıyor'

    Import-Module WebAdministration -ErrorAction SilentlyContinue

    if (Test-Path "IIS:\Sites\$SiteName")     { Remove-Website -Name $SiteName; Write-Info 'Site silindi.' }
    if (Test-Path "IIS:\AppPools\$AppPoolName") { Remove-WebAppPool -Name $AppPoolName; Write-Info 'Uygulama havuzu silindi.' }

    Get-NetFirewallRule -DisplayName 'FSC ERP HTTP*' -ErrorAction SilentlyContinue | Remove-NetFirewallRule
    if (Test-Path $InstallPath) { Remove-Item $InstallPath -Recurse -Force }

    $lnk = Join-Path ([Environment]::GetFolderPath('CommonDesktopDirectory')) 'FSC Takip ERP.url'
    if (Test-Path $lnk) { Remove-Item $lnk -Force }

    if ($PurgeData) {
        Write-Warn 'PurgeData verildi — veritabanı ve belge arşivi SİLİNİYOR.'
        $inst = @(Get-SqlInstances) | Select-Object -First 1
        if ($inst) {
            Invoke-Sql -Instance $inst -Query "ALTER DATABASE [$Database] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$Database];" | Out-Null
        }
        $root = Resolve-DataPath
        if (Test-Path $root) { Remove-Item $root -Recurse -Force }
    } else {
        Write-Info "Veritabanı ($Database) ve belge arşivi KORUNDU."
        Write-Info 'Tamamen silmek için: install-engine.ps1 -Mode Uninstall -PurgeData'
    }

    Write-Host ''
    Write-Host 'Kaldırma tamamlandı.'
}

# ─────────────────────────────────────────────────────────────────────────────
# Ana akış
# ─────────────────────────────────────────────────────────────────────────────

Assert-Administrator

if ($Mode -eq 'Uninstall') {
    Invoke-Uninstall
    exit 0
}

$dataRoot = Resolve-DataPath
$logDir   = Join-Path $dataRoot 'logs'
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir -Force | Out-Null }
$transcript = Join-Path $logDir "setup-$(Get-Date -Format 'yyyyMMdd-HHmmss').log"
Start-Transcript -Path $transcript | Out-Null

try {
    if (-not $SourcePath) { $SourcePath = Join-Path $PSScriptRoot 'app' }
    if (-not $PrereqPath) { $PrereqPath = Join-Path $PSScriptRoot 'prereq' }

    if ($Port -le 0) {
        # 80 doluysa (başka web sunucusu/panel) 8080'e düş.
        $busy = Get-NetTCPConnection -LocalPort 80 -State Listen -ErrorAction SilentlyContinue
        if ($busy) { $Port = 8080 } else { $Port = 80 }
    }

    Write-Host '════════════════════════════════════════════════════'
    Write-Host ' FSC Takip ERP — Kurulum'
    Write-Host ' ARD Sistem ve Danışmanlık'
    Write-Host '════════════════════════════════════════════════════'
    Write-Host ''

    Enable-IisFeatures
    Install-HostingBundle
    $instance = Resolve-SqlInstance
    New-FscDatabase       -Instance $instance
    # Set-AppFiles/Set-AppSettings ÖNCE gelmeli: New-Website -PhysicalPath var olan bir
    # klasör ister, $InstallPath'i ilk oluşturan Set-AppFiles'tır (VM testi 2026-07-18 —
    # bu sıralama hatası önceki denemelerde SQL adımında patladığı için hiç görünmemişti).
    Set-AppFiles
    Set-AppSettings       -Instance $instance -Root $dataRoot
    Set-IisSite
    Grant-AppPoolSqlAccess -Instance $instance
    New-DataFolders       -Root $dataRoot
    Set-FirewallRule
    Set-TrialMarker
    Test-SiteResponds
    Write-InstallReport   -Instance $instance -Root $dataRoot

    Write-Host ''
    Write-Host 'KURULUM BAŞARIYLA TAMAMLANDI.' -ForegroundColor Green
    exit 0
}
catch {
    Write-Host ''
    Write-Host 'KURULUM BAŞARISIZ' -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ''
    Write-Host "Ayrıntılı log: $transcript"
    exit 1
}
finally {
    Stop-Transcript | Out-Null
}
