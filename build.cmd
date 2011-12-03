@echo Off
set config=%1
if "%config%" == "" (
   set config=debug
)

md artifacts
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild Jabbot.sln /p:Configuration="%config%";BuildPackage=true;PackageOutputDir=../artifacts /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false