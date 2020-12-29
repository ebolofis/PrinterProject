@echo off 
set ServiceName=ExtECR

echo.
echo       * * * * * * * * * * * * * * * * * * * * * * * * 
echo       *                                             *
echo       *      Stoping Service '%ServiceName%'
echo       *                                             *
echo       * * * * * * * * * * * * * * * * * * * * * * * * 
echo.
sc stop %ServiceName%
echo.

PAUSE