@echo off
set solution=%1
set solution=%solution:"=%

copy "%solution%\Extensions\SampleAnnouncement\bin\Debug\*.dll" "%solution%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\"
copy "%solution%\Extensions\SampleAnnouncement\bin\Debug\*.pdb" "%solution%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\"
copy "%solution%\Extensions\SampleSprocket\bin\Debug\*.dll" "%solution%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\"
copy "%solution%\Extensions\SampleSprocket\bin\Debug\*.pdb" "%solution%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\"
copy "%solution%\Extensions\TwitterAnnouncer\bin\Debug\*.dll" "%solution%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\"
copy "%solution%\Extensions\TwitterAnnouncer\bin\Debug\*.pdb" "%solution%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\"
copy "%solution%\Extensions\IPityTheFoolSprocket\bin\Debug\*.dll" "%solution%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\"
copy "%solution%\Extensions\IPityTheFoolSprocket\bin\Debug\*.pdb" "%solution%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\"