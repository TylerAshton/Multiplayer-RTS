@echo off
setlocal ENABLEDELAYEDEXPANSION

REM === CONFIGURATION ===
set MERGETOOL_PATH="X:\Program Files (x86)\6000.0.30f1\Editor\Data\Tools\UnityYAMLMerge.exe"
set SCENE_PATH=Assets/Scenes/MainWorld.unity
REM ^ Adjust to your Unity version and scene location

echo === Unity Scene Auto-Merge with Direct LFS Fetch ===

REM === GET BRANCH NAMES ===
set /p BRANCH1="Enter your branch name (e.g. main): "
set /p BRANCH2="Enter their branch name (e.g. feature/xyz): "

echo.
echo 🔍 Finding common ancestor between %BRANCH1% and %BRANCH2%...

REM === FIND MERGE BASE COMMIT ===
for /f %%i in ('git merge-base %BRANCH1% %BRANCH2%') do set BASE_HASH=%%i

if not defined BASE_HASH (
    echo ❌ Failed to find common ancestor. Are the branches valid?
    pause
    exit /b 1
)

echo 🧠 Found base commit: %BASE_HASH%

REM === FUNCTION TO FETCH AND SAVE PURE FILE ===
REM Usage: call :fetchPureFile <commit> <path> <outputfile>

:fetchPureFile
setlocal
set commit=%1
set filepath=%2
set outfile=%3

REM Get blob SHA of the file at commit
for /f %%b in ('git ls-tree -r %commit% -- "%filepath%" ^| awk "{print $3}"') do set blob_sha=%%b

REM Fallback if awk is not available (Windows CMD alternative)
if not defined blob_sha (
    for /f "tokens=3" %%b in ('git ls-tree -r %commit% -- "%filepath%"') do set blob_sha=%%b
)

if not defined blob_sha (
    echo ❌ Failed to get blob SHA for %filepath% at %commit%
    endlocal & exit /b 1
)

REM Download blob content (pure file)
git cat-file -p %blob_sha% > "%outfile%"

if errorlevel 1 (
    echo ❌ Failed to extract pure file %outfile%
    endlocal & exit /b 1
) else (
    echo ✅ Extracted pure file %outfile%
)

endlocal & exit /b 0

REM ===========================

echo.
echo 📥 Fetching and extracting pure files from commits...

call :fetchPureFile %BASE_HASH% %SCENE_PATH% Base.unity
call :fetchPureFile %BRANCH1% %SCENE_PATH% Mine.unity
call :fetchPureFile %BRANCH2% %SCENE_PATH% Theirs.unity

echo.
echo 🧪 Merging scenes...
%MERGETOOL_PATH% merge -p Base.unity Theirs.unity Mine.unity Merged.unity

if %ERRORLEVEL% EQU 0 (
    echo ✅ Merge complete: Merged.unity
) else (
    echo ❌ Merge failed. Inspect Base.unity, Theirs.unity, Mine.unity manually.
)

pause
exit /b