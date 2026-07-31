SETLOCAL

REM 動作条件設定

REM 設定ファイルパス
SET CONFIGFILE=..\DBsettingForBatch.ini

REM 設定ファイルが存在するか確認する
if not exist %CONFIGFILE% (
    echo ERROR: Not found %CONFIGFILE%
    exit /b 1
)

REM 設定ファイルを読み込む
for /f "usebackq tokens=1,* delims==" %%a in ("%CONFIGFILE%") do (
REM # 環境変数として登録する
    call set %%a=%%b
)

REM 既存データ削除
REM del /F .\*.sql

REM データ作成
sqlcmd -S %HOSTNAME% -d %DBNAME% %DBAuthentication% -I -Q "SET NOCOUNT ON; EXECUTE dbo.adm_S_VBストアドプロシジャ定義SQL作成 " -Y0 -y0 -b -o "AppCommonsClass_DBAccess_Stored.vb" 

endlocal

PAUSE
