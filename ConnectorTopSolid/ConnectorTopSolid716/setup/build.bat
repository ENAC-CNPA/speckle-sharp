@echo off&setlocal

echo "Build and zip setup file"

set folder=%~dp0
for %%i in ("%folder%\..\..\..\..") do set "folder=%%~fi"
set folder=%folder%\Speckle.TopSolid\Binaries
echo Delete all exe : %folder%
del %folder%\*.exe

echo Build Inno setup
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" setup.iss

echo Zip setup
timeout /t 3
powershell -Command "Compress-Archive -Force -Path %folder%\*.exe -DestinationPath %folder%\Speckle_ConnectorTopSolid_v7.16.zip"
