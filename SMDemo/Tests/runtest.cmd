@echo off
rem
rem Call with 5 arguments Or no arguments
rem Default sets host to be ws://localhost:8080, compression flag to empty string, credit update quantum to zero
rem    runtest.cmd
rem Explicit arguments examples
rem  runtest.cmd c 64000 ws smserver.cloudapp.net 8080  -> static compression, no encryption, CU=64000, cloud service
rem  runtest.cmd n 128000 wss smserver.cloudapp.net 8081  -> no compression, encryption, CU=128000, cloud service  
rem  runtest.cmd n 0 wss smserver.cloudapp.net 8081   -> no compression, encryption, no CU, cloud service  
rem
@setlocal


set TEST_CONNECT_OPTIONS=1
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
if %argcActual% NEQ 5 (

   if %argcActual% NEQ 0 (
      echo ERROR: call with no args or 5 args!!!
      goto END
   )

   set COMPRESSION= 
   set CREDITUPDATE= 
   set SCHEME=ws
   set URL=localhost
   set PORT=8080
   set SERVERNAME=ws://localhost:8080

) else (

   if /I "%~1" == "n" (
     set COMPRESSION= 
   ) else (
     set COMPRESSION=%~1
   )

   if /I "%~2" == "0" (
     set CREDITUPDATE=
   ) else (
     set CREDITUPDATE=%~2
   )

   set SCHEME=%~3
   set URL=%~4
   set PORT==%~5
   set SERVERNAME=%~3://%~4:%~5

)

echo starting with compression=%COMPRESSION%, creditUpdate=%CREDITUPDATE%, SERVERNAME=%SERVERNAME%


:TEST0
if %TEST_CONNECT_OPTIONS% == 0 goto TEST1

rem Invalid scheme
rem
client verbose 3 connect wsss://%URL% close >res.txt 2>&1
IF ERRORLEVEL 1 (
   findstr /C:"ERROR:Unrecognized URL scheme. Specify 'ws/wss' URL scheme." res.txt >NUL 2>&1
   IF ERRORLEVEL 1 (
      type res.txt
      echo Failed 'CONNECT INVALID SCHEME' smoke  Error: Unexpected error string.
      goto ENDTEST0
   )
) else (
   type res.txt
   echo Failed 'CONNECT INVALID SCHEME' smoke  Error: Client did not terminate with error code
   goto ENDTEST0
)

rem Empty scheme
rem
client verbose 3 connect %URL%:%PORT% close >res.txt 2>&1
IF ERRORLEVEL 1 (
   findstr /C:"ERROR:Unrecognized URL scheme. Specify 'ws/wss' URL scheme." res.txt >NUL 2>&1
   IF ERRORLEVEL 1 (
      type res.txt
      echo Failed 'CONNECT EMPTY SCHEME' smoke  Error: Unexpected error string.
      goto ENDTEST0
   )
) else (
   type res.txt
   echo Failed 'CONNECT EMPTY SCHEME' smoke  Error: Client did not terminate with error code
   goto ENDTEST0
)


rem Invalid port
rem
client verbose 3 connect %SCHEME%://%URL%:9000 close >res.txt 2>&1
IF ERRORLEVEL 1 (
   findstr /C:"ERROR:Session error: No connection could be made because the target machine actively refused it" res.txt >NUL 2>&1
   IF ERRORLEVEL 1 (
      type res.txt
      echo Failed 'CONNECT INVALID PORT' smoke  Error: Unexpected error string.
      goto ENDTEST0
   )
) else (
   type res.txt
   echo Failed 'CONNECT INVALID PORT' smoke  Error: Client did not terminate with error code
   goto ENDTEST0
)

rem Empty URL
rem
client verbose 3 connect %SCHEME%://:%PORT% close >res.txt 2>&1
IF ERRORLEVEL 1 (
   findstr /C:"ERROR: [%SCHEME%://:%PORT%] Uri is not in correct format." res.txt >NUL 2>&1
   IF ERRORLEVEL 1 (
      type res.txt
      echo Failed 'CONNECT EMPTY URL' smoke  Error: Unexpected error string.
      goto ENDTEST0
   )
) else (
   type res.txt
   echo Failed 'CONNECT EMPTY URL' smoke  Error: Client did not terminate with error code
   goto ENDTEST0
)


client verbose 3 connect %SERVERNAME% close >res.txt 2>&1
IF ERRORLEVEL 1 (
   type res.txt
   echo Failed 'CONNECT URL' smoke  Error: Client terminated with error code
   goto ENDTEST0
) else (
   findstr /C:"Session open URI=%SERVERNAME%/ State=Opened IsFlowControlEnabled=False" res.txt >NUL 2>&1
   IF ERRORLEVEL 1 (
      echo Failed 'CONNECT URL' smoke  Error: Unexpected debug string
      goto ENDTEST0
   )
)

client verbose 3 connect c %SERVERNAME% close >res.txt 2>&1
IF ERRORLEVEL 1 (
   type res.txt
   echo Failed 'CONNECT C URL' smoke  Error: Client terminated with error code
   goto ENDTEST0
) else (
   findstr /C:"Session open URI=%SERVERNAME%/ State=Opened IsFlowControlEnabled=False" res.txt >NUL 2>&1
   IF ERRORLEVEL 1 (
      echo Failed 'CONNECT C URL' smoke  Error: Unexpected debug string
      goto ENDTEST0
   )
)

client verbose 3 connect S %SERVERNAME% close >res.txt 2>&1
IF ERRORLEVEL 1 (
   type res.txt
   echo Failed 'CONNECT S URL' smoke  Error: Client terminated with error code
   goto ENDTEST0
) else (
   findstr /C:"Session open URI=%SERVERNAME%/ State=Opened IsFlowControlEnabled=False" res.txt >NUL 2>&1
   IF ERRORLEVEL 1 (
      echo Failed 'CONNECT C URL' smoke  Error: Unexpected debug string
      goto ENDTEST0
   )
)


client verbose 3 connect 64000 %SERVERNAME% close >res.txt 2>&1
IF ERRORLEVEL 1 (
   type res.txt
   echo Failed 'CONNECT CU URL' smoke  Error: Client terminated with error code
   goto ENDTEST0
) else (
   findstr /C:"Session open URI=%SERVERNAME%/ State=Opened IsFlowControlEnabled=True" res.txt >NUL 2>&1
   IF ERRORLEVEL 1 (
      echo Failed 'CONNECT CU URL' smoke  Error: Unexpected debug string
      goto ENDTEST0
   )
)


client verbose 3 connect c 64000 %SERVERNAME% close >res.txt 2>&1
IF ERRORLEVEL 1 (
   type res.txt
   echo Failed 'CONNECT C CU URL' smoke  Error: Client terminated with error code
   goto ENDTEST0
) else (
   findstr /C:"Session open URI=%SERVERNAME%/ State=Opened IsFlowControlEnabled=True" res.txt >NUL 2>&1
   IF ERRORLEVEL 1 (
      echo Failed 'CONNECT C CU URL' smoke  Error: Unexpected debug string
      goto ENDTEST0
   )
)

client verbose 3 connect s 64000 %SERVERNAME% close >res.txt 2>&1
IF ERRORLEVEL 1 (
   type res.txt
   echo Failed 'CONNECT C CU URL' smoke  Error: Client terminated with error code
   goto ENDTEST0
) else (
   findstr /C:"Session open URI=%SERVERNAME%/ State=Opened IsFlowControlEnabled=True" res.txt >NUL 2>&1
   IF ERRORLEVEL 1 (
      echo Failed 'CONNECT C CU URL' smoke  Error: Unexpected debug string
      goto ENDTEST0
   )
)

rem Invalid order of arguments to CONNECT
rem
client verbose 3 connect 64000 s %SERVERNAME% close >res.txt 2>&1
IF ERRORLEVEL 1 (
   findstr /C:"ERROR:64000: Compression option should be 'c' or 's'" res.txt >NUL 2>&1
   IF ERRORLEVEL 1 (
      echo Failed 'CONNECT CU S URL' smoke  Error: Unexpected debug string
      goto ENDTEST0
   )
) else (
   type res.txt
   echo Failed 'CONNECT CU S URL' smoke  Error: Client did not terminate with error code
   goto ENDTEST0
)

rem Invalid order of arguments to CONNECT
rem
client verbose 3 connect 64000 c %SERVERNAME% close >res.txt 2>&1
IF ERRORLEVEL 1 (
   findstr /C:"ERROR:64000: Compression option should be 'c' or 's'" res.txt >NUL 2>&1
   IF ERRORLEVEL 1 (
      echo Failed 'CONNECT CU C URL' smoke  Error: Unexpected debug string
      goto ENDTEST0
   )
) else (
   type res.txt
   echo Failed 'CONNECT CU C URL' smoke  Error: Client did not terminate with error code
)

echo TEST_CONNECT_OPTIONS Passed

:ENDTEST0


:TEST1
if %TEST_DOWNLOAD_EXISTING_TEST_FILE% == 0 goto TEST2

rmdir /s /q files >NUL 2>&1
client connect %SERVERNAME% get /files/test.txt close >res.txt 2>&1
IF ERRORLEVEL 1 (
   type res.txt
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
client connect %SERVERNAME% get /files/test123.txt close >res.txt 2>&1
IF ERRORLEVEL 1 (
   type res.txt
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
client connect %SERVERNAME% get /website/test.html close >res.txt 2>&1
IF ERRORLEVEL 1 (
   type res.txt
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
client connect %SERVERNAME% get /website/test2.html close >res.txt 2>&1
IF ERRORLEVEL 1 (
   type res.txt
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
client connect %SERVERNAME% httpget /files/test.txt close >res.txt 2>&1
IF ERRORLEVEL 1 (
   type res.txt
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
client connect %SERVERNAME% httpget /files/test123.txt close >res.txt 2>&1
IF ERRORLEVEL 1 (
   type res.txt
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
client connect %SERVERNAME% httpget /website/test.html close >res.txt 2>&1
IF ERRORLEVEL 1 (
   type res.txt
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
client connect %SERVERNAME% httpget /website/test2.html close >res.txt 2>&1
IF ERRORLEVEL 1 (
   type res.txt
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
IF ERRORLEVEL 1 (
   type res.txt
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


