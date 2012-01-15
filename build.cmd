@echo Off
set config=%1
if "%config%" == "" (
   set config=debug
)

%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Jabbot.sln /p:Configuration="%config%"

.\tools\xunit\xunit.console.clr4.exe D:\Code\github\code52\jibbr\Tests\ExtensionTests\bin\Debug\ExtensionTests.dll