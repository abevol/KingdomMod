@echo off
cd /d %~dp0

set BIE=%1
if "%BIE%"=="" (goto ARG_NOT_EXIST)

REM 打包每个模块
call handle_module.bat BetterPayableUpgrade %BIE%
call handle_module.bat DevTools %BIE%
call handle_module.bat OverlayMap %BIE%
call handle_module.bat StaminaBar %BIE%

REM 打包所有模块为一个综合包
for /f "tokens=3 delims=><" %%A in ('findstr "<Version>" "..\ProjectSettings.shared.props"') do (
    set "version=%%A"
)
set "filename=.\packages\%version%\KingdomMod.All-%BIE%-v%version%.zip"
7za a -tzip "%filename%" ".\packages\KingdomMod.BetterPayableUpgrade" ".\packages\KingdomMod.DevTools" ".\packages\KingdomMod.OverlayMap" ".\packages\KingdomMod.StaminaBar"

goto END

:ARG_NOT_EXIST
echo [ERROR] Missing argument: BIE
goto END

:END
