<#
.SYNOPSIS
    FscErpSetup.exe paketini üretir. ARD tarafında, internet erişimi olan makinede çalışır.

.DESCRIPTION
    Karmaşıklık bilerek burada toplanmıştır: ön koşullar burada indirilip SHA256 ile
    doğrulanır, sahaya tek bir çevrimdışı EXE gider. Müşteri sunucusunun internete
    çıkması gerekmez — ERP sunucusunda istenmeyen bir şeydir zaten.

    Gereksinimler:
      - .NET 8 SDK
      - Inno Setup 6 (https://jrsoftware.org/isdl.php) — ISCC.exe PATH'te veya
        varsayılan konumunda

.PARAMETER SkipPrereq
    Ön koşul indirmeyi atla (build\prereq zaten doluysa hızlı yeniden derleme).

.EXAMPLE
    .\build-installer.ps1 -Version 1.0.0
#>
[CmdletBinding()]
param(
    [string] $Version = '1.0.0',
    [switch] $SkipPrereq,
    [switch] $SkipPublish
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$RepoRoot   = Split-Path $PSScriptRoot -Parent
$BuildDir   = Join-Path $PSScriptRoot 'build'
$AppDir     = Join-Path $BuildDir 'app'
$PrereqDir  = Join-Path $BuildDir 'prereq'
$DistDir    = Join-Path $PSScriptRoot 'dist'
$WebUiProj  = Join-Path $RepoRoot 'FSCTakip.WebUI\FSCTakip.WebUI.csproj'

# ─────────────────────────────────────────────────────────────────────────────
# Ön koşullar.
#
# Sha256 = '' ise script dosyayı indirir, hash'ini hesaplar ve DURUR: değeri
# buraya yapıştırmanızı ister. Bu bilinçli bir insan onayı kapısıdır — hash'i
# sabitlemeden önce dosyayı resmi Microsoft kaynağından indirdiğinizi doğrulayın.
# Sabitlendikten sonra her derleme aynı dosyayı aldığını kanıtlar; Microsoft yeni
# yama yayınlayıp içerik değişirse script yine durur ve sizi haberdar eder.
#
# Uydurma/tahmini hash YAZMAYIN — doğrulama varmış görüntüsü verip hiçbir şey
# doğrulamaz. Boş bırakmak dürüst, uydurmak tehlikelidir.
#
# ⚠️ SQL Express yeniden dağıtımı: EXE'ye gömmeden önce kullandığınız sürümün
#    lisans şartlarını teyit edin (bkz. installer/README.md).
# ─────────────────────────────────────────────────────────────────────────────
$Prereqs = @(
    @{
        Name   = '.NET 8 Hosting Bundle'
        File   = 'dotnet-hosting-8.0.11-win.exe'
        Url    = 'https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/8.0.11/dotnet-hosting-8.0.11-win.exe'
        # Sabitlendi 2026-07-17 — Authenticode: Valid, CN=Microsoft Corporation
        Sha256 = '71778D44FF786667C24C6ACBFF93FE76891D8F98E30898DCEE97CF03F02BB0BE'
    }
)

# SQL Express doğrudan indirilmez — bkz. Get-SqlExpressPackage.
# Microsoft'un yayınladığı `SQL2022-SSEI-Expr.exe` bir ÇEVRİMİÇİ İNDİRİCİDİR (~6 MB):
# çalıştığı makinede internete çıkıp asıl paketi çeker. Pakete onu gömmek çevrimdışı
# kurulumu imkânsız kılardı — müşteri sunucusu internet arardı.
# Doğru yol: SSEI'yi BURADA /Action=Download ile çalıştırıp gerçek çevrimdışı paketi
# (SQLEXPR_x64_ENU.exe) ürettirmek. Kurulum motorunun aradığı dosya budur.
$SqlSseiUrl  = 'https://go.microsoft.com/fwlink/?linkid=2216019'  # SQL Server 2022 Express SSEI
$SqlSseiFile = 'SQL2022-SSEI-Expr.exe'
$SqlPackage  = 'SQLEXPR_x64_ENU.exe'
# Sabitlendi 2026-07-17 — 714 MB tam çevrimdışı paket, Authenticode: Valid, CN=Microsoft Corporation
$SqlPackageSha256 = '74AA90C11202A5524E769B9BC22531BAEF22D91E9B2D2E8C3CB99E89A65C5297'

function Write-Step { param([string] $M) Write-Host "`n=== $M ===" -ForegroundColor Cyan }
function Write-Ok   { param([string] $M) Write-Host "  [OK] $M" -ForegroundColor Green }
function Write-Warn { param([string] $M) Write-Host "  [!]  $M" -ForegroundColor Yellow }

function Find-Iscc {
    $cmd = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    foreach ($p in @(
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "$env:ProgramFiles\Inno Setup 6\ISCC.exe")) {
        if (Test-Path $p) { return $p }
    }
    throw @'
Inno Setup 6 bulunamadı. https://jrsoftware.org/isdl.php adresinden kurun
(varsayılan konuma kurmak yeterli; PATH'e eklemeye gerek yok).
'@
}

# ─────────────────────────────────────────────────────────────────────────────
# 1. Uygulamayı yayınla
# ─────────────────────────────────────────────────────────────────────────────
function Invoke-Publish {
    Write-Step 'Uygulama yayınlanıyor (dotnet publish -c Release)'

    if (Test-Path $AppDir) { Remove-Item $AppDir -Recurse -Force }
    New-Item -ItemType Directory -Path $AppDir -Force | Out-Null

    & dotnet publish $WebUiProj -c Release -o $AppDir --nologo
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish başarısız (çıkış kodu $LASTEXITCODE)." }

    Write-Ok "Yayınlandı: $AppDir"
}

# ─────────────────────────────────────────────────────────────────────────────
# 2. Sızıntı denetimi — müşteri paketine ASLA girmemesi gerekenler
# ─────────────────────────────────────────────────────────────────────────────
function Test-NoSecretLeak {
    Write-Step 'Sızıntı denetimi'

    # appsettings.json gitignored AMA geliştirme makinesinde diskte durur ve
    # publish onu çıktıya kopyalar — yani ARD'nin kendi bağlantı dizesi müşteri
    # paketine sızar. Kurulum motoru appsettings.json'ı sıfırdan üretir; burada
    # publish çıktısından siliyoruz.
    $leaky = Join-Path $AppDir 'appsettings.json'
    if (Test-Path $leaky) {
        $content = Get-Content $leaky -Raw
        Remove-Item $leaky -Force
        Write-Ok 'appsettings.json publish çıktısından çıkarıldı (kurulumda üretilir).'
        if ($content -match 'Server\s*=\s*([^;"]+)') {
            Write-Warn "Çıkarılan dosyadaki bağlantı: Server=$($Matches[1]) — pakete girmedi."
        }
    }

    # Development ayarları da müşteride işi yok
    $dev = Join-Path $AppDir 'appsettings.Development.json'
    if (Test-Path $dev) { Remove-Item $dev -Force; Write-Ok 'appsettings.Development.json çıkarıldı.' }

    # Kullanıcı yüklemeleri — bir müşterinin belgeleri diğerine GİTMEZ.
    # Web SDK wwwroot altındaki her şeyi yayına koyar; .gitignore'un etkisi yoktur.
    # Kök çözüm csproj'daki <Content Remove="wwwroot\uploads\**" />; burası ikinci
    # savunma hattı — csproj değişirse veya publish önbelleği bayatlarsa yakalar.
    $uploads = Join-Path $AppDir 'wwwroot\uploads'
    if (Test-Path $uploads) {
        $uf = @(Get-ChildItem $uploads -Recurse -File -ErrorAction SilentlyContinue)
        Remove-Item $uploads -Recurse -Force
        Write-Warn "wwwroot\uploads pakete girmişti — $($uf.Count) dosya çıkarıldı. csproj'daki Content Remove kuralını denetleyin."
    }

    # ETL geçici Excel'leri de müşteride işi yok
    foreach ($tmp in @('wwwroot\temp', 'App_Data')) {
        $p = Join-Path $AppDir $tmp
        if (Test-Path $p) { Remove-Item $p -Recurse -Force; Write-Ok "$tmp çıkarıldı." }
    }

    # license.lic müşteriye özel — pakette olmamalı (ThirdPartyKurulum FAZ 3.6)
    $lics = @(Get-ChildItem -Path $AppDir -Filter '*.lic' -Recurse -File -ErrorAction SilentlyContinue)
    if ($lics.Count -gt 0) {
        throw "Publish çıktısında lisans dosyası var: $($lics.FullName -join ', '). Pakete girmemeli — csproj'da neden kopyalandığını denetleyin."
    }

    # ARD özel anahtarı hiçbir koşulda pakete giremez
    $keys = @(Get-ChildItem -Path $AppDir -Include '*.pem', '*private*key*' -Recurse -File -ErrorAction SilentlyContinue)
    if ($keys.Count -gt 0) {
        throw "Publish çıktısında anahtar dosyası var: $($keys.FullName -join ', '). Lisans özel anahtarı ASLA dağıtılmaz."
    }

    Write-Ok 'Paketle gönderilmemesi gereken dosya bulunmadı.'
}

# ─────────────────────────────────────────────────────────────────────────────
# 3. Ön koşullar — indir + SHA256 doğrula
# ─────────────────────────────────────────────────────────────────────────────
# SQL Express'in gerçek çevrimdışı paketini üretir.
# Dönüş: sabitlenmesi gereken hash (henüz sabitlenmemişse), aksi halde $null.
function Get-SqlExpressPackage {
    $target = Join-Path $PrereqDir $SqlPackage

    if (Test-Path $target) {
        $hash = (Get-FileHash $target -Algorithm SHA256).Hash
        if ($SqlPackageSha256) {
            if ($hash -eq $SqlPackageSha256) {
                Write-Ok "SQL Server Express — mevcut, hash doğru."
                return $null
            }
            Remove-Item $target -Force
            throw "SQL Express paketi hash tutmuyor (beklenen $SqlPackageSha256, bulunan $hash). Dosya silindi; tekrar çalıştırın."
        }
        return $hash   # sabitlenmemiş — çağıran insan onayı isteyecek
    }

    # 1) SSEI indiricisini al (küçük)
    $ssei = Join-Path $PrereqDir $SqlSseiFile
    if (-not (Test-Path $ssei)) {
        Write-Host "  SQL Express indiricisi alınıyor..."
        $ProgressPreference = 'SilentlyContinue'
        Invoke-WebRequest -Uri $SqlSseiUrl -OutFile $ssei -UseBasicParsing
    }

    $sig = Get-AuthenticodeSignature $ssei
    if ($sig.Status -ne 'Valid') {
        Remove-Item $ssei -Force
        throw "SQL Express indiricisinin imzası geçersiz ($($sig.Status)). Dosya silindi — kaynağı doğrulayın."
    }
    Write-Ok "İndirici imzası geçerli: $($sig.SignerCertificate.Subject.Split(',')[0])"

    # 2) SSEI'yi ÇEVRİMDIŞI PAKET ÜRETMEK için çalıştır (kurulum yapmaz, indirir)
    Write-Host '  SQL Express çevrimdışı paketi indiriliyor (~280 MB, birkaç dakika)...'
    $p = Start-Process -FilePath $ssei `
        -ArgumentList @('/Action=Download', '/MediaType=Core', "/MediaPath=$PrereqDir", '/Quiet') `
        -Wait -PassThru -NoNewWindow
    if ($p.ExitCode -ne 0) {
        throw "SQL Express paketi indirilemedi (çıkış kodu $($p.ExitCode))."
    }

    if (-not (Test-Path $target)) {
        $found = (Get-ChildItem $PrereqDir -Filter 'SQLEXPR*.exe' -File | ForEach-Object Name) -join ', '
        throw @"
Beklenen paket üretilmedi: $SqlPackage
Klasördeki SQLEXPR dosyaları: $(if ($found) { $found } else { '(yok)' })

Kurulum motoru 'SQLEXPR*.exe' desenini arar — dosya adı farklıysa
install-engine.ps1 içindeki Get-FirstFile desenini de güncelleyin.
"@
    }

    # İndirici artık gerekmez — pakete girmesin (müşteri sunucusunda internet aramasın)
    Remove-Item $ssei -Force

    Write-Ok "SQL Express çevrimdışı paketi hazır: $SqlPackage"
    return (Get-FileHash $target -Algorithm SHA256).Hash
}

function Get-Prereqs {
    Write-Step 'Ön koşullar hazırlanıyor'

    if (-not (Test-Path $PrereqDir)) { New-Item -ItemType Directory -Path $PrereqDir -Force | Out-Null }

    $unpinned = @()

    foreach ($p in $Prereqs) {
        $dest = Join-Path $PrereqDir $p.File

        if (Test-Path $dest) {
            $hash = (Get-FileHash $dest -Algorithm SHA256).Hash
            if ($p.Sha256 -and $hash -eq $p.Sha256) {
                Write-Ok "$($p.Name) — mevcut, hash doğru."
                continue
            }
            if ($p.Sha256) {
                Write-Warn "$($p.Name) — hash tutmuyor, yeniden indiriliyor."
                Remove-Item $dest -Force
            }
        }

        if (-not (Test-Path $dest)) {
            Write-Host "  $($p.Name) indiriliyor... ($($p.Url))"
            $sw = [Diagnostics.Stopwatch]::StartNew()
            try {
                # Invoke-WebRequest büyük dosyada yavaş; BITS varsa onu kullan
                Start-BitsTransfer -Source $p.Url -Destination $dest -ErrorAction Stop
            } catch {
                $ProgressPreference = 'SilentlyContinue'
                Invoke-WebRequest -Uri $p.Url -OutFile $dest -UseBasicParsing
            }
            $sw.Stop()
            Write-Host ("    indirildi ({0} sn)" -f [int]$sw.Elapsed.TotalSeconds)
        }

        $hash = (Get-FileHash $dest -Algorithm SHA256).Hash

        if (-not $p.Sha256) {
            # Henüz sabitlenmemiş — insan onayı iste.
            $unpinned += [pscustomobject]@{ Name = $p.Name; File = $p.File; Hash = $hash }
            continue
        }

        if ($hash -ne $p.Sha256) {
            Remove-Item $dest -Force
            throw @"
$($p.Name) SHA256 doğrulaması BAŞARISIZ — dosya pakete alınmadı.

  Beklenen : $($p.Sha256)
  İndirilen: $hash

Microsoft yeni bir yama sürümü yayınlamış olabilir. Dosyayı resmi kaynaktan
indirdiğinizi doğruladıktan sonra bu script içindeki Sha256 değerini güncelleyin.
Doğrulanmamış bir yükleyiciyi müşteri paketine koymayın.
"@
        }
        Write-Ok "$($p.Name) — hash doğrulandı."
    }

    # SQL Express — SSEI aracılığıyla gerçek çevrimdışı paketi üret
    $sqlHash = Get-SqlExpressPackage
    if ($sqlHash) {
        $unpinned += [pscustomobject]@{ Name = 'SQL Server Express (SqlPackageSha256)'; File = $SqlPackage; Hash = $sqlHash }
    }

    if ($unpinned.Count -gt 0) {
        $lines = $unpinned | ForEach-Object { "  $($_.Name)`n    Sha256 = '$($_.Hash)'" }
        throw @"
Sabitlenmemiş ön koşul var — derleme bilerek durduruldu.

Aşağıdaki dosyalar indirildi ancak hash'leri henüz bu script'e yazılmadı:

$($lines -join "`n")

YAPILACAK:
  1. Dosyaların imzasını doğrulayın:
       Get-AuthenticodeSignature '$PrereqDir\<dosya>' | Format-List Status, SignerCertificate
     Status = Valid ve imzalayan Microsoft olmalı.
  2. Yukarıdaki Sha256 değerlerini bu script'e yazın:
       - '(SqlPackageSha256)' etiketli olan → `$SqlPackageSha256 değişkenine
       - diğerleri                          → `$Prereqs listesindeki ilgili girdiye
  3. build-installer.ps1'i tekrar çalıştırın.

Bu kapı bilinçlidir: müşteri sunucusuna kuracağınız yükleyicinin ne olduğunu
bir kere insan gözüyle onaylamanız gerekir.
"@
    }

    $total = (Get-ChildItem $PrereqDir -File | Measure-Object -Property Length -Sum).Sum
    Write-Ok ("Ön koşul toplamı: {0:N0} MB" -f ($total / 1MB))
}

# ─────────────────────────────────────────────────────────────────────────────
# 4. Inno Setup ile derle
# ─────────────────────────────────────────────────────────────────────────────
function Invoke-Compile {
    Write-Step 'Kurulum paketi derleniyor (Inno Setup)'

    $iscc = Find-Iscc
    if (-not (Test-Path $DistDir)) { New-Item -ItemType Directory -Path $DistDir -Force | Out-Null }

    # ISCC'nin stdout'u (boş satırlar dahil) doğrudan ekrana gitmeli — Out-Host ile.
    # Aksi halde fonksiyonun dönüş akışına karışır ve `$exe = Invoke-Compile` tek path
    # yerine [çıktı satırları..., path] dizisi olur; sonraki Get-FileHash boş string'e takılır.
    & $iscc "/DAppVersion=$Version" (Join-Path $PSScriptRoot 'FscErpSetup.iss') | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "Inno Setup derlemesi başarısız (çıkış kodu $LASTEXITCODE)." }

    $exe = Join-Path $DistDir "FscErpSetup-$Version.exe"
    if (-not (Test-Path $exe)) { throw "Beklenen çıktı üretilmedi: $exe" }
    return $exe
}

# ─────────────────────────────────────────────────────────────────────────────
# Ana akış
# ─────────────────────────────────────────────────────────────────────────────
Write-Host '════════════════════════════════════════════════════'
Write-Host " FSC Takip ERP — Kurulum Paketi Üretici  (v$Version)"
Write-Host '════════════════════════════════════════════════════'

if (-not $SkipPublish) { Invoke-Publish } else { Write-Warn 'Publish atlandı (-SkipPublish).' }
Test-NoSecretLeak
if (-not $SkipPrereq)  { Get-Prereqs }  else { Write-Warn 'Ön koşul indirme atlandı (-SkipPrereq).' }

$exe = Invoke-Compile

$hash = (Get-FileHash $exe -Algorithm SHA256).Hash
$size = (Get-Item $exe).Length / 1MB

Write-Host ''
Write-Host '════════════════════════════════════════════════════' -ForegroundColor Green
Write-Host ' PAKET HAZIR' -ForegroundColor Green
Write-Host '════════════════════════════════════════════════════' -ForegroundColor Green
Write-Host "  Dosya  : $exe"
Write-Host ("  Boyut  : {0:N1} MB" -f $size)
Write-Host "  SHA256 : $hash"
Write-Host ''
Write-Host '  Teslim kaydı için SHA256 değerini saklayın.'
Write-Host '  Kurulumu ÖNCE temiz bir sanal makinede doğrulayın — müşteri sahası prova yeri değildir.'
Write-Host ''

# Teslim kaydı
$manifest = Join-Path $DistDir "FscErpSetup-$Version.sha256.txt"
[IO.File]::WriteAllText($manifest, "$hash  FscErpSetup-$Version.exe`r`nÜretim: $(Get-Date -Format 'dd.MM.yyyy HH:mm')`r`n",
    (New-Object Text.UTF8Encoding($true)))
