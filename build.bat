@ECHO OFF
CLS
SETLOCAL

CALL "./FAKE\Fake.exe" "build.fsx" %*

PAUSE