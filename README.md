AppleTV Assistant
==================

This assistant encodes and tags media (specifically TV shows) so that they can be 
imported easily into iTunes.  This is meant to be part of a larger set of utilities 
to automatically manage a media collection.

Dependencies
-------------
This project takes advantage of several other open source projects and API's, including:

* Handbrake - http://handbrake.fr/ (for video encoding)
* FFmpeg - http://www.ffmpeg.org/ (for video manipulation)
* ServiceStack.Text - https://github.com/ServiceStack/ServiceStack.Text (for JSON parsing)
* AtomicParsley - https://github.com/wez/atomicparsley (for iTunes meta data manipulation)
* iTunes search API - http://www.apple.com/itunes/affiliates/resources/documentation/itunes-store-web-service-search-api.html (for artwork and ratings information)
* Filebot - http://www.filebot.net/ (for TV show information gathering)

Optional, but suggested
------------------------
* uTorrent - http://www.utorrent.com/ (for TV show torrent downloading)
* showRSS - http://showrss.karmorra.info/ (for TV show feeds)

Setup
=====
1. First, get uTorrent and Filebot up and running and working.  For a tutorial, see the Filebot main page.
2. Next, adjust the Filebot script to run a script when it's completed processing.  The updated command in uTorrent should look something like this:

		"C:\Program Files\FileBot\filebot.exe" -script fn:amc --output "C:/filebot/outputdir" --log-file amc.log --action copy --conflict override -non-strict --def music=y exec="C:/ATVAssistant/ATVEncodeTag.exe -f ""{file}""" "ut_dir=%D" "ut_file=%F" "ut_kind=%K" "ut_title=%N" "ut_label=%L" "ut_state=%S" 

3. Make sure that ATVAssistant is in C:\ATVAssistant
4. Adjust the ATVEncodeTag.exe.config file as needed.  You can set many different options, including the default Handbrake switches and the Pushover API keys.
5. If you adjust the "AfterProcessingPath" to something useful (like your 'Automatically Add to iTunes' directory), TV shows will just appear in iTunes and your AppleTV correctly.
6. Sit back, and enjoy automated goodness.

