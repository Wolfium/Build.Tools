@ECHO OFF
CLS
SETLOCAL

CALL "./FAKE\Fake.exe" "build/build.fsx" %*

PAUSE