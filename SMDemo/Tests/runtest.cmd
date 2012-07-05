@echo off
rem
rem Call with 2 arguments Or no arguments
rem Default sets host to be ws://localhost:8080, compression flag to empty string
rem    runtest.cmd
rem Explicit arguments examples
rem  runtest.cmd c ws://smserver.cloudapp.net:8080  -> static compression, no encryption, cloud service
rem  runtest.cmd n wss://smserver.cloudapp.net:8081  -> no compression, encryption, cloud service  
rem
@setlocal


set TEST_DOWNLOAD_EXISTING_TEST_FILE=1
set TEST_DOWNLOAD_NON_EXISTING_TEST_FILE=1
set TEST_DOWNLOAD_EXISTING_WEBSITE=1
set TEST_DOWNLOAD_NON_EXISTING_WEBSITE=1
set TEST_HTTP11_DOWNLOAD_EXISTING_TEST_FILE=1
set TEST_HTTP11_DOWNLOAD_NON_EXISTING_TEST_FILE=1
set TEST_HTTP11_DOWNLOAD_EXISTING_WEBSITE=1
set TEST_HTTP11_DOWNLOAD_NON_EXISTING_WEBSITE=1
set TEST_SM_LOG=1
set TEST10_ON=1


set argcActual=0
for %%i in (%*) do set /A argcActual+=1
if %argcActual% NEQ 2 (

   if %argcActual% NEQ 0 (
      echo ERROR: call with no args or 2 args!!!
      goto END
   )

   set COMPRESSION= 
   set SERVERNAME=ws://localhost:8080

) else (

   if /I "%~1" == "n" (
     set COMPRESSION= 
   ) else (
     set COMPRESSION=%~1
   )

   set SERVERNAME=%~2

)

echo starting with compression=%COMPRESSION% and SERVERNAME=%SERVERNAME%



:TEST1
if %TEST_DOWNLOAD_EXISTING_TEST_FILE% == 0 goto TEST2

rmdir /s /q files >NUL 2>&1
client connect %SERVERNAME% get /files/test.txt close >NUL 2>&1
IF %ERRORLEVEL% NEQ 0 (
   echo Failed the smoke TEST_DOWNLOAD_EXISTING_TEST_FILE  Error: Client terminated with error code
) else (
   if not exist .\files\test.txt ( 
      echo Failed the smoke TEST_DOWNLOAD_EXISTING_TEST_FILE   Error: Did not load TEST.TXT
   ) else (
       dir files\test.txt > res.txt
       findstr /C:"13 test.txt" res.txt >NUL 2>&1
       IF %ERRORLEVEL% NEQ 0 (
          echo Failed the smoke TEST_DOWNLOAD_EXISTING_TEST_FILE   Error: TEST.TXT is not 13 bytes
       ) else (
           echo TEST_DOWNLOAD_EXISTING_TEST_FILE Passed
       )
   )
)


:TEST2
if %TEST_DOWNLOAD_NON_EXISTING_TEST_FILE% == 0 goto TEST3

rmdir /s /q files >NUL 2>&1
client connect %SERVERNAME% get /files/test123.txt close >NUL 2>&1
IF %ERRORLEVEL% NEQ 0 (
   echo Failed the smoke TEST_DOWNLOAD_NON_EXISTING_TEST_FILE Error: Client terminated with error code
) else (
   if exist .\files\test123.txt ( 
      echo Failed the smoke TEST_DOWNLOAD_NON_EXISTING_TEST_FILE  Error: Loaded non-existing TEST123.TXT
   ) else (
      echo TEST_DOWNLOAD_NON_EXISTING_TEST_FILE Passed
   )
)

:TEST3
if %TEST_DOWNLOAD_EXISTING_WEBSITE% == 0 goto TEST4

rmdir /s /q website >NUL 2>&1
client connect %SERVERNAME% get /website/test.html close >NUL 2>&1
IF %ERRORLEVEL% NEQ 0 (
   echo Failed the smoke TEST_DOWNLOAD_EXISTING_WEBSITE Error: Client terminated with error code
) else (
  dir website > res.txt
  findstr /C:"28 File(s)      1,214,303 bytes" res.txt >NUL 2>&1
  IF %ERRORLEVEL% NEQ 0 ( 
     echo Failed the smoke TEST_DOWNLOAD_EXISTING_WEBSITE  Error: Number of files or size do not match
  ) else (
     echo TEST_DOWNLOAD_EXISTING_WEBSITE Passed
  )
)


:TEST4
if %TEST_DOWNLOAD_NON_EXISTING_WEBSITE% == 0 goto TEST5

rmdir /s /q website >NUL 2>&1
client connect %SERVERNAME% get /website/test2.html close >NUL 2>&1
IF %ERRORLEVEL% NEQ 0 (
   echo Failed the smoke TEST_DOWNLOAD_NON_EXISTING_WEBSITE Error: Client terminated with error code
) else (
  if exist .\website\test2.html ( 
     echo Failed the smoke TEST_DOWNLOAD_NON_EXISTING_WEBSITE Error: Loaded non-existing test2.html
  ) else (
     echo TEST_DOWNLOAD_NON_EXISTING_WEBSITE Passed
  )
)


:TEST5
if %TEST_HTTP11_DOWNLOAD_EXISTING_TEST_FILE% == 0 goto TEST6

rmdir /s /q files >NUL 2>&1
client connect %SERVERNAME% httpget /files/test.txt close >NUL 2>&1
IF %ERRORLEVEL% NEQ 0 (
   echo Failed the smoke TEST_HTTP11_DOWNLOAD_EXISTING_TEST_FILE Error: Client terminated with error code
) else (
   if not exist .\files\test.txt ( 
      echo Failed the smoke TEST_HTTP11_DOWNLOAD_EXISTING_TEST_FILE  Error: .\files\test.txt  does not exist
   ) else (
       dir files\test.txt > res.txt
       findstr /C:"13 test.txt" res.txt >NUL 2>&1
       IF %ERRORLEVEL% NEQ 0 (
          echo Failed the smoke TEST_HTTP11_DOWNLOAD_EXISTING_TEST_FILE  Error: TEST.TXT is not 13 bytes
       ) else (
           echo TEST_HTTP11_DOWNLOAD_EXISTING_TEST_FILE Passed
       )
   )
)


:TEST6
if %TEST_HTTP11_DOWNLOAD_NON_EXISTING_TEST_FILE% == 0 goto TEST7

rmdir /s /q files >NUL 2>&1
client connect %SERVERNAME% httpget /files/test123.txt close >NUL 2>&1
IF %ERRORLEVEL% NEQ 0 (
   echo Failed the smoke TEST_HTTP11_DOWNLOAD_NON_EXISTING_TEST_FILE Error: Client terminated with error code
) else (
   if exist .\files\test123.txt ( 
      echo Failed the smoke TEST_HTTP11_DOWNLOAD_NON_EXISTING_TEST_FILE Error: Loaded non-existing TEST123.TXT
   ) else (
      echo TEST_HTTP11_DOWNLOAD_NON_EXISTING_TEST_FILE Passed
   )
)


:TEST7
if %TEST_HTTP11_DOWNLOAD_EXISTING_WEBSITE% == 0 goto TEST8

del /s /q website\* >NUL 2>&1
client connect %SERVERNAME% httpget /website/test.html close >NUL 2>&1
IF %ERRORLEVEL% NEQ 0 (
   echo Failed the smoke TEST_HTTP11_DOWNLOAD_EXISTING_WEBSITE Error: Client terminated with error code
) else (
  IF not exist .\website\test.html (
     echo Failed the smoke TEST_HTTP11_DOWNLOAD_EXISTING_WEBSITE  Error: .\website\test.html does not exist
  ) else (
     dir website > res.txt 
     findstr /C:"28 File(s)      1,214,303 bytes" res.txt >NUL 2>&1
     IF %ERRORLEVEL% NEQ 0 ( 
       echo Failed the smoke TEST_HTTP11_DOWNLOAD_EXISTING_WEBSITE  Error: Number of files or size do not match
     ) else (
       echo TEST_HTTP11_DOWNLOAD_EXISTING_WEBSITE Passed
     )
  )
)


:TEST8
if %TEST_HTTP11_DOWNLOAD_NON_EXISTING_WEBSITE% == 0 goto TEST9

rmdir /s /q website >NUL 2>&1
client connect %SERVERNAME% httpget /website/test2.html close >NUL 2>&1
IF %ERRORLEVEL% NEQ 0 (
   echo Failed the smoke TEST_HTTP11_DOWNLOAD_NON_EXISTING_WEBSITE Error: Client terminated with error code
) else (
  if exist .\website\test2.html ( 
     echo Failed the smoke TEST_HTTP11_DOWNLOAD_NON_EXISTING_WEBSITE  Error: Loaded non-existing test2.html
  ) else (
     echo TEST_HTTP11_DOWNLOAD_NON_EXISTING_WEBSITE Passed
  )
)


:TEST9

if %TEST_SM_LOG% == 0 goto TEST10

rmdir /s /q website >NUL 2>&1
del /q res.txt >NUL 2>&1
client connect %SERVERNAME% get /website/test.html dump-stats close >res.txt 2>&1
IF %ERRORLEVEL% NEQ 0 (
   echo Failed the smoke TEST_SM_LOG Error: Client terminated with error code
) else (
  findstr /C:"Size of data exchanged:            1218826" res.txt >NUL 2>&1
  IF %ERRORLEVEL% NEQ 0 ( 
     echo Failed the smoke TEST_SM_LOG Error: S+M log does not match
  ) else (
     echo TEST_SM_LOG Passed
  )
)


:TEST10


:END


