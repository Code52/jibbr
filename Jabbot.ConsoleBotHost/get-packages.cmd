@echo off

set solution_dir=%1
set output_dir=%solution_dir%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\

del %solution_dir%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\*.dll
xcopy %solution_dir%\Extensions\SampleAnnouncement\bin\Debug\*.dll %output_dir% /C /Y
xcopy %solution_dir%\Extensions\SampleSprocket\bin\Debug\*.dll %output_dir% /C /Y
xcopy %solution_dir%\Extensions\HelpSprocket\bin\Debug\*.dll %output_dir% /C /Y