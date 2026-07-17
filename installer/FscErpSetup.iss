; ═══════════════════════════════════════════════════════════════════════════
;  FSC Takip ERP — Kurulum Sihirbazı
;  ARD Sistem ve Danışmanlık
;
;  Bu dosya ince bir sarmalayıcıdır: kullanıcıdan SQL örneği ve port bilgisini
;  alır, gerisini install-engine.ps1 yapar. Kurulum mantığı buraya yazılmaz.
;
;  Derleme: installer\build-installer.ps1 (ISCC.exe'yi çağırır)
; ═══════════════════════════════════════════════════════════════════════════

#define AppName        "FSC Takip ERP"
#define AppPublisher   "ARD Sistem ve Danışmanlık"
#define AppURL         "https://ardsistem.com.tr"
#ifndef AppVersion
  #define AppVersion   "1.0.0"
#endif

[Setup]
; Ürünün kalıcı kimliği — Windows bu değere bakıp güncelleme mi yeni kurulum mu
; olduğuna karar verir. ASLA değiştirme: değişirse aynı ürün ikinci kez kurulu
; görünür ve müşteride iki ayrı kayıt oluşur.
AppId={{6FF3DC0F-9109-4C92-8744-879C4605DAD2}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
DefaultDirName=C:\inetpub\FscErp
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
DisableDirPage=yes
; Yollar bu .iss dosyasının bulunduğu klasöre görelidir (installer\).
; build-installer.ps1 aynı konumları kullanır — değiştirirseniz ikisini birlikte değiştirin.
OutputDir=dist
OutputBaseFilename=FscErpSetup-{#AppVersion}
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
; Sunucu kurulumu — masaüstü olmayan Server Core dışında her yerde çalışır
MinVersion=10.0
UninstallDisplayName={#AppName}
SetupLogging=yes

[Languages]
; Turkish.isl Inno Setup 6 ile birlikte gelir. Default.isl İNGİLİZCEDİR —
; "tr" adını verip Default.isl göstermek sihirbazı İngilizce bırakır.
Name: "tr"; MessagesFile: "compiler:Languages\Turkish.isl"

[Files]
; Uygulama (dotnet publish çıktısı) — build-installer.ps1 hazırlar
Source: "build\app\*";    DestDir: "{app}\_stage\app";    Flags: recursesubdirs createallsubdirs ignoreversion
; Ön koşul yükleyicileri — tam çevrimdışı kurulum için pakete gömülü
Source: "build\prereq\*"; DestDir: "{app}\_stage\prereq"; Flags: recursesubdirs createallsubdirs ignoreversion
; Kurulum motoru
Source: "install-engine.ps1"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Kurulum Raporu"; Filename: "{code:GetReportPath}"
Name: "{group}\{#AppName} Kaldır"; Filename: "{uninstallexe}"

[Run]
; Motor Inno'nun ilerleme sayfasından çalışır (aşağıdaki CurStepChanged'e bak).

[UninstallRun]
Filename: "powershell.exe"; \
  Parameters: "-NoProfile -ExecutionPolicy Bypass -File ""{app}\install-engine.ps1"" -Mode Uninstall {code:GetPurgeFlag}"; \
  Flags: runhidden waituntilterminated; RunOnceId: "FscErpUninstall"

[Code]
var
  SqlPage:      TInputOptionWizardPage;
  PortPage:     TInputQueryWizardPage;
  DonePage:     TOutputMsgMemoWizardPage;
  SqlInstances: TStringList;
  ResultUrl:    String;
  ResultKey:    String;
  ResultReport: String;
  PurgeData:    Boolean;

function GetReportPath(Param: String): String;
begin
  if ResultReport <> '' then Result := ResultReport
  else Result := 'C:\FscErpData\kurulum-raporu.txt';
end;

function GetPurgeFlag(Param: String): String;
begin
  if PurgeData then Result := '-PurgeData' else Result := '';
end;

{ ── Sunucudaki mevcut SQL örneklerini registry'den oku ──────────────────── }
procedure LoadSqlInstances;
var
  Names: TArrayOfString;
  I: Integer;
begin
  SqlInstances := TStringList.Create;
  if RegGetValueNames(HKEY_LOCAL_MACHINE,
       'SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL', Names) then
  begin
    for I := 0 to GetArrayLength(Names) - 1 do
    begin
      if CompareText(Names[I], 'MSSQLSERVER') = 0 then
        SqlInstances.Add('localhost')
      else
        SqlInstances.Add('localhost\' + Names[I]);
    end;
  end;
end;

{ ── 80 portu dolu mu? Doluysa 8080 öner ────────────────────────────────── }
function SuggestPort: String;
var
  ResultCode: Integer;
begin
  { netstat çıktısını PowerShell ile denetle; hata olursa güvenli varsayılan 8080 }
  if Exec('powershell.exe',
       '-NoProfile -Command "if (Get-NetTCPConnection -LocalPort 80 -State Listen -EA SilentlyContinue) { exit 1 } else { exit 0 }"',
       '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    if ResultCode = 0 then Result := '80' else Result := '8080';
  end
  else
    Result := '8080';
end;

procedure InitializeWizard;
var
  I: Integer;
begin
  LoadSqlInstances;

  { ── SQL seçim sayfası ── }
  SqlPage := CreateInputOptionPage(wpWelcome,
    'Veritabanı Sunucusu',
    'FSC Takip ERP verilerini nerede saklayacak?',
    'Sunucunuzda zaten bir SQL Server varsa onu kullanabilirsiniz — mevcut veritabanlarınız etkilenmez. ' +
    'Yoksa kurulum ücretsiz SQL Server Express''i sizin için kurar.',
    True, False);

  SqlPage.Add('SQL Server Express kur (önerilen — sunucuda SQL yoksa)');
  for I := 0 to SqlInstances.Count - 1 do
    SqlPage.Add('Mevcut örneği kullan: ' + SqlInstances[I]);

  { Sunucuda SQL varsa ilkini öner, yoksa Express kurulumunu seç }
  if SqlInstances.Count > 0 then
    SqlPage.SelectedValueIndex := 1
  else
    SqlPage.SelectedValueIndex := 0;

  { ── Port sayfası ── }
  PortPage := CreateInputQueryPage(SqlPage.ID,
    'Erişim Portu',
    'Kullanıcılar sisteme hangi porttan bağlanacak?',
    'Varsayılan 80''dir. Sunucuda başka bir web uygulaması 80''i kullanıyorsa kurulum 8080 önerir.' + #13#10 +
    'Kullanıcılar adresi tarayıcıya "http://sunucu-adresi:port" biçiminde yazacaktır.');
  PortPage.Add('Port:', False);
  PortPage.Values[0] := SuggestPort;

  { ── Bitiş sayfası ── }
  DonePage := CreateOutputMsgMemoPage(wpInfoAfter,
    'Kurulum Tamamlandı', 'Sisteme erişim bilgileri',
    'Aşağıdaki bilgiler ayrıca kurulum raporuna kaydedildi. Kalıcı lisans için sunucu kimlik kodunu ARD''ye iletin.',
    '');
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  Port: Integer;
begin
  Result := True;
  if CurPageID = PortPage.ID then
  begin
    Port := StrToIntDef(PortPage.Values[0], -1);
    if (Port < 1) or (Port > 65535) then
    begin
      MsgBox('Geçerli bir port numarası girin (1-65535).', mbError, MB_OK);
      Result := False;
    end;
  end;
end;

function GetSelectedSqlInstance: String;
begin
  { 0 = Express kur; 1..n = mevcut örnek }
  if SqlPage.SelectedValueIndex = 0 then
    Result := ''
  else
    Result := SqlInstances[SqlPage.SelectedValueIndex - 1];
end;

{ ── Motorun çıktısını okuyup sonuç değerlerini ayıkla ──────────────────── }
procedure ParseEngineOutput(const LogPath: String);
var
  Lines: TArrayOfString;
  I: Integer;
  Line: String;
begin
  if not LoadStringsFromFile(LogPath, Lines) then Exit;
  for I := 0 to GetArrayLength(Lines) - 1 do
  begin
    Line := Trim(Lines[I]);
    if Pos('RESULT_URL=', Line) = 1 then
      ResultUrl := Copy(Line, Length('RESULT_URL=') + 1, Length(Line))
    else if Pos('RESULT_MACHINEKEY=', Line) = 1 then
      ResultKey := Copy(Line, Length('RESULT_MACHINEKEY=') + 1, Length(Line))
    else if Pos('RESULT_REPORT=', Line) = 1 then
      ResultReport := Copy(Line, Length('RESULT_REPORT=') + 1, Length(Line));
  end;
end;

procedure RunEngine;
var
  Cmd, OutFile, Instance: String;
  ResultCode: Integer;
begin
  OutFile  := ExpandConstant('{tmp}\fscerp-engine.out');
  Instance := GetSelectedSqlInstance;

  WizardForm.StatusLabel.Caption :=
    'Ön koşullar, SQL Server ve veritabanı kuruluyor — bu adım 10-15 dakika sürebilir...';
  WizardForm.ProgressGauge.Style := npbstMarquee;

  { Motorun tüm çıktısı dosyaya alınır; sonuç satırları oradan okunur.
    Hata durumunda dosya kullanıcıya gösterilir. }
  Cmd := '-NoProfile -ExecutionPolicy Bypass -File "' + ExpandConstant('{app}\install-engine.ps1') + '"' +
         ' -SourcePath "' + ExpandConstant('{app}\_stage\app') + '"' +
         ' -PrereqPath "' + ExpandConstant('{app}\_stage\prereq') + '"' +
         ' -InstallPath "' + ExpandConstant('{app}\app') + '"' +
         ' -Port ' + PortPage.Values[0];
  if Instance <> '' then
    Cmd := Cmd + ' -SqlInstance "' + Instance + '"';

  { PowerShell'i cmd üzerinden çağırıp çıktıyı yakala }
  Cmd := '/C powershell.exe ' + Cmd + ' > "' + OutFile + '" 2>&1';

  if not Exec(ExpandConstant('{cmd}'), Cmd, '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    MsgBox('Kurulum motoru başlatılamadı.', mbCriticalError, MB_OK);
    Abort;
  end;

  ParseEngineOutput(OutFile);

  if ResultCode <> 0 then
  begin
    MsgBox('Kurulum tamamlanamadı.' + #13#10#13#10 +
           'Ayrıntılı hata kaydı açılacak. Sorun sürerse bu dosyayı ARD''ye iletin:' + #13#10 +
           OutFile, mbCriticalError, MB_OK);
    ShellExec('open', 'notepad.exe', '"' + OutFile + '"', '', SW_SHOW, ewNoWait, ResultCode);
    Abort;
  end;

  WizardForm.ProgressGauge.Style := npbstNormal;
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    RunEngine;

    DonePage.RichEditViewer.Lines.Text :=
      'ERİŞİM ADRESİ' + #13#10 +
      '  ' + ResultUrl + #13#10 +
      '  (Adres çubuğuna "http://" ve port dahil yazılmalıdır.)' + #13#10#13#10 +
      'İLK GİRİŞ' + #13#10 +
      '  Kullanıcı: admin' + #13#10 +
      '  Parola   : admin123' + #13#10 +
      '  >>> Giriş yapar yapmaz parolayı değiştirin. <<<' + #13#10#13#10 +
      'LİSANS — 30 GÜNLÜK DENEME BAŞLADI' + #13#10 +
      '  Sunucu kimlik kodu: ' + ResultKey + #13#10 +
      '  Bu kodu ARD Sistem ve Danışmanlık''a iletin; size özel license.lic' + #13#10 +
      '  dosyası gönderilecektir. Süre dolduğunda VERİLER KORUNUR.' + #13#10#13#10 +
      'DOĞRULAMA' + #13#10 +
      '  Kurulumu ağdaki BAŞKA bir bilgisayardan adresi açarak doğrulayın —' + #13#10 +
      '  sunucunun kendisinden yapılan test güvenlik duvarı eksiğini yakalamaz.' + #13#10#13#10 +
      'Rapor: ' + ResultReport;
  end;
end;

{ ── Kaldırma: veri korunsun mu? ────────────────────────────────────────── }
function InitializeUninstall: Boolean;
begin
  PurgeData := MsgBox(
    'FSC Takip ERP kaldırılacak.' + #13#10#13#10 +
    'Veritabanı ve belge arşivi (irsaliye/fatura PDF''leri) de silinsin mi?' + #13#10#13#10 +
    'HAYIR (önerilen): veriler korunur, yalnız uygulama kaldırılır.' + #13#10 +
    'EVET: tüm FSC verileri KALICI olarak silinir — FSC denetimi 5 yıl geriye' + #13#10 +
    'belge isteyebilir, önce yedek aldığınızdan emin olun.',
    mbConfirmation, MB_YESNO or MB_DEFBUTTON2) = IDYES;
  Result := True;
end;

[Messages]
tr.WelcomeLabel2=Bu sihirbaz [name/ver] uygulamasını sunucunuza kuracaktır.%n%nGerekli tüm bileşenler (IIS, .NET 8, SQL Server Express) pakete dahildir — internet bağlantısı gerekmez.%n%nKurulum 10-15 dakika sürer.
tr.FinishedHeadingLabel=FSC Takip ERP kuruldu
