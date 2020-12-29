@echo off 
set ServiceName=ExtECR

echo.
echo       * * * * * * * * * * * * * * * * * * * * * * * * 
echo       *                                             *
echo       *      Deleting Service '%ServiceName%'
echo       *                                             *
echo       * * * * * * * * * * * * * * * * * * * * * * * * 
echo.
sc delete  %ServiceName%
echo.

PAUSE