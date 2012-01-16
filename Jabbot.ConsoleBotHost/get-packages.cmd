set solution_dir=%1
set output_dir=%solution_dir%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\

del %solution_dir%\Jabbot.ConsoleBotHost\bin\Debug\Sprockets\*.dll

for /D %%I in ( %solution_dir%\Extensions\* ) do (
xcopy %%I\bin\Debug\*.dll %output_dir% /C /Y
)