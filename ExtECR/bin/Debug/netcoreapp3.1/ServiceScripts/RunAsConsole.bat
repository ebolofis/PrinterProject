@echo off 
set ServiceLocation=%CD%\..\ExtECR.exe
echo.
echo.
echo   Running as Console App: %ServiceLocation%
echo. 
echo.  
echo. 
%ServiceLocation% --console
echo.  

PAUSE