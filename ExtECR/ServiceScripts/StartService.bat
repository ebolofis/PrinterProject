@echo off 
set ServiceName=ExtECR

echo.
echo       * * * * * * * * * * * * * * * * * * * * * * * * 
echo       *                                             *
echo       *      Starting Service '%ServiceName%'
echo       *                                             *
echo       * * * * * * * * * * * * * * * * * * * * * * * * 
echo.
sc start %ServiceName%
echo.

PAUSE