@echo off
REM Until new major version of C# is released the build number in the path will keep counting.
REM CCK *requires* .NET compiler version 4.0 or higher. Don't get confused with KSP run-time requirement of 3.5.
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild ..\SOURCE\CCK\CCK\CCK.csproj /t:Rebuild /p:Configuration=Release
