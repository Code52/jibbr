set solution=%1

copy "%solution%\Extensions\SampleAnnouncement\bin\Debug\*.dll" "%solution%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\"
copy "%solution%\Extensions\SampleSprocket\bin\Debug\*.dll" "%solution%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\"