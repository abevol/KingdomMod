@echo off
cd /d %~dp0

set BIE=%1

if "%BIE%"=="" (goto ARG_NOT_EXIST)

XCOPY /Y /S "..\BetterPayableUpgrade\bin\%BIE%\" ".\packages\KingdomMod.BetterPayableUpgrade\"
set /p version=<"..\BetterPayableUpgrade\version.txt"
if not exist ".\packages\%version%" (
    MKDIR ".\packages\%version%"
)
set filename=".\packages\%version%\KingdomMod.BetterPayableUpgrade-%BIE%-v%version%.zip"
7za a -tzip "%filename%" ".\packages\KingdomMod.BetterPayableUpgrade"

XCOPY /Y /S "..\DevTools\bin\%BIE%" ".\packages\KingdomMod.DevTools\"
set /p version=<"..\DevTools\version.txt"
if not exist ".\packages\%version%" (
    MKDIR ".\packages\%version%"
)
set filename=".\packages\%version%\KingdomMod.DevTools-%BIE%-v%version%.zip"
7za a -tzip "%filename%" ".\packages\KingdomMod.DevTools"

XCOPY /Y /S "..\OverlayMap\bin\%BIE%" ".\packages\KingdomMod.OverlayMap\"
set /p version=<"..\OverlayMap\version.txt"
if not exist ".\packages\%version%" (
    MKDIR ".\packages\%version%"
)
set filename=".\packages\%version%\KingdomMod.OverlayMap-%BIE%-v%version%.zip"
7za a -tzip "%filename%" ".\packages\KingdomMod.OverlayMap"

XCOPY /Y /S "..\StaminaBar\bin\%BIE%" ".\packages\KingdomMod.StaminaBar\"
set /p version=<"..\StaminaBar\version.txt"
if not exist ".\packages\%version%" (
    MKDIR ".\packages\%version%"
)
set filename=".\packages\%version%\KingdomMod.StaminaBar-%BIE%-v%version%.zip"
7za a -tzip "%filename%" ".\packages\KingdomMod.StaminaBar"

set /p version=<"..\OverlayMap\version.txt"
set filename=".\packages\%version%\KingdomMod.All-%BIE%-v%version%.zip"
7za a -tzip "%filename%" ".\packages\KingdomMod.BetterPayableUpgrade" ".\packages\KingdomMod.DevTools" ".\packages\KingdomMod.OverlayMap" ".\packages\KingdomMod.StaminaBar" 

goto END

:ARG_NOT_EXIST
echo ARG_NOT_EXIST

:END