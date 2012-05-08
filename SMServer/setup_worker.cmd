@echo off

echo Granting permissions for Network Service to the deployment directory...
icacls . /grant "Users":(OI)(CI)F
netsh advfirewall firewall add rule name="Allow Input Service Traffic on 8080" protocol=TCP dir=in action=allow enable=yes localport=8080 profile=any
if %ERRORLEVEL% neq 0 goto error
echo OK

echo SUCCESS
exit /b 0

:error

echo FAILED
exit /b -1