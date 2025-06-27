@echo off
REM 参数检查
if "%1"=="" (
    echo [ERROR] Module is missing!
    exit /b 1
)
if "%2"=="" (
    echo [ERROR] BIE argument is missing!
    exit /b 1
)

REM 设置参数
set "MODULE=%1"
set "BIE=%2"

REM 复制文件
XCOPY /Y /S "..\%MODULE%\bin\%BIE%" ".\packages\KingdomMod.%MODULE%\"

REM 读取版本号
for /f "tokens=3 delims=><" %%A in ('findstr "<Version>" "..\ProjectSettings.shared.props"') do (
    set "version=%%A"
)

REM 创建版本目录
if not exist ".\packages\%version%" (
    MKDIR ".\packages\%version%"
)

REM 创建ZIP文件
set "filename=.\packages\%version%\KingdomMod.%MODULE%-%BIE%-v%version%.zip"
7za a -tzip "%filename%" ".\packages\KingdomMod.%MODULE%"
