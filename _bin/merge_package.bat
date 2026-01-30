@echo off
cd /d %~dp0

set BIE=%1
if "%BIE%"=="" (goto ARG_NOT_EXIST)

REM 读取版本号
for /f "tokens=3 delims=><" %%A in ('findstr "<Version>" "..\ProjectSettings.shared.props"') do (
    set "version=%%A"
)
if "%version%"=="" (
    echo [ERROR] Version not found in ProjectSettings.shared.props
    goto END
)

set "pkgRoot=.\packages"
set "pkgDir=%pkgRoot%\%version%"

REM 根据 BIE 参数选择 BepInEx 包
if /I "%BIE%"=="BIE6_IL2CPP" (
    set "bepinexZip=%pkgRoot%\BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.753+0d275a4.zip"
) else if /I "%BIE%"=="BIE6_Mono" (
    set "bepinexZip=%pkgRoot%\BepInEx-Unity.Mono-win-x64-6.0.0-be.753+0d275a4.zip"
) else (
    echo [ERROR] Unknown BIE: %BIE%
    goto END
)

set "cpp2ilZip=%pkgRoot%\Cpp2IL.Patch.zip"
set "il2cppInteropZip=%pkgRoot%\Il2CppInterop.Patch.zip"
set "modZip=%pkgDir%\KingdomMod.All-%BIE%-v%version%.zip"
set "outZip=%pkgDir%\KingdomMod.All-%BIE%-v%version%-with-BepInEx.zip"

if not exist "%bepinexZip%" (echo [ERROR] Missing file: %bepinexZip% & goto END)
if /I "%BIE%"=="BIE6_IL2CPP" (
    if not exist "%cpp2ilZip%" (echo [ERROR] Missing file: %cpp2ilZip% & goto END)
    if not exist "%il2cppInteropZip%" (echo [ERROR] Missing file: %il2cppInteropZip% & goto END)
)
if not exist "%modZip%" (echo [ERROR] Missing file: %modZip% & goto END)

set "tmpDir=%pkgDir%\_merge_tmp"
if exist "%tmpDir%" rmdir /s /q "%tmpDir%"
mkdir "%tmpDir%"

if exist "%outZip%" del /f /q "%outZip%"

REM 解压压缩包到同一临时目录以实现合并
7za x -y -o"%tmpDir%" "%bepinexZip%"
if /I "%BIE%"=="BIE6_IL2CPP" (
    7za x -y -o"%tmpDir%" "%cpp2ilZip%"
    7za x -y -o"%tmpDir%" "%il2cppInteropZip%"
)
7za x -y -o"%tmpDir%\BepInEx\plugins" "%modZip%"

REM 重新打包为最终输出
7za a -tzip "%outZip%" "%tmpDir%\*"

REM 清理临时目录
rmdir /s /q "%tmpDir%"

goto END

:ARG_NOT_EXIST
echo [ERROR] Missing argument: BIE
goto END

:END


