@echo off
set solution=%1

cp %solution%/Extensions/SampleAnnouncement/bin/Debug/*.dll %solution%/Jabbot.ConsoleBotHost/bin/Debug/Sprockets/
cp %solution%/Extensions/SampleSprocket/bin/Debug/*.dll %solution%/Jabbot.ConsoleBotHost/bin/Debug/Sprockets/