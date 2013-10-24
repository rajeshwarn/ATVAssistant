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
* TVshow-info - https://github.com/danesparza/tvshow-info (for TV Episode information)

Optional, but suggested
------------------------
* uTorrent - http://www.utorrent.com/ (for TV show torrent downloading)
* showRSS - http://www.showrss.info/ (for TV show feeds)

Setup
=====
1. First, get uTorrent up and running and working.  For a tutorial, see the uTorrent main page.
2. Next, set uTorrent to run a script when it's completed processing.  The updated command in uTorrent should look something like this:

		"C:\ATVAssistant\ATVEncodeTag.exe" -f "%F" -d "%D" -t "%K" 

3. Make sure that ATVAssistant is in C:\ATVAssistant
4. Adjust the ATVEncodeTag.exe.config file as needed.  You can set many different options, including the default Handbrake switches and the Pushover API keys.
5. If you adjust the "AfterProcessingPath" in the ATVEncodeTag.config to something useful (like your 'Automatically Add to iTunes' directory), TV shows will just appear in iTunes and your AppleTV correctly.
6. Sit back, and enjoy automated goodness.

