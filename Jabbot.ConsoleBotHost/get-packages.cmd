@echo off
set solution=%1
set solution=%solution:"=%

copy "%solution%\Extensions\SampleAnnouncement\bin\Debug\*.dll" "%solution%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\"
copy "%solution%\Extensions\SampleSprocket\bin\Debug\*.dll" "%solution%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\"
copy "%solution%\Extensions\TwitterSprocket\bin\Debug\*.dll" "%solution%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\"