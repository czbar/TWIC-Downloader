# TWIC-Downloader

## Purpose
Semi-automates downloads of PGN files from the web site of The Week in Chess.

This is to help users updating their chess databases e.g. Chessbase.

The utility checks for the latest downloaded file (as per the info in 
TwicLatest.txt) and starts downloading from the next one, 
or from the file of the version specified on the command line. 

All new files are downloaded and merged into a single PGN file that can
can be conveniently used as input for updating the user's database.

## Command  line arguments
   /help or /? - print this info   
   /dir        - working folder   
   /from nnnn  - number of the earliest twic pgn zip archive to download   
   /to nnnn    - number of the latest twic pgn zip archive to download

Example: TWIC Downloader /dir . /from 1420 /to 1425")

NOTE: you can view available archives at https://theweekinchess.com/twic.

If the /from argument is not specified,
the downloader will look for the number following the number
stored in TwicLatest.txt in the working folder.

If the /to argument is not specified,
the downloader will look for ALL files with numbers greater than
the one stored in TwicLatest.txt in the working folder or
specified by the /from argument
