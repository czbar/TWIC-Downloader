using System;
using System.Diagnostics;
using System.Net;
using System.IO.Compression;
using System.IO;
using System.Text;
using System.Collections.Generic;

/// <summary>
/// Checks for the latest zip downloads available on the TWIC web site.
/// If there is anything new, download, unzip and merge.
/// 
/// </summary>
namespace TWIC_Downloader
{
    class Program
    {

        //
        // DEFAULTS
        //

        //
        // These are constant and cannot be changed from the command line.
        //

        // Url of the page with TWIC downloads
        static string TwicArchiveUrl = "https://theweekinchess.com/zips/";

        // file in whihc the number id of the last downloaded file is stored.
        static string lastDownloadedInfoFile = "TwicLatest.txt";

        // if /from is not specified and there is no TwicLatest.txt we start from
        // this number (first archive for 2022).
        static long lastDownloadedNumber = 1417;
        static bool useDefaultlastDownloadedNumber = false;

        // Working folder.
        // By deafult this is the current directory
        static string workingFolder = ".";

        // The earliest TWIC archive to download.
        // By default this is null. The downloads start from
        // the number following the lastDownloadedNumber + 1
        // where lastDownloadedNumber is saved in "TwicLatest.txt"
        static long? downloadFromNo = null;

        // The latest TWIC archive to download.
        // By default this is null. The downloads will stop
        // when the archive for a given number (as they get incremented
        // during the download operation) is not found.
        static long? downloadToNo = null;

        static string tmpSubfolder;

        static List<long> twicFilesDownloaded = new List<long>();

        static void Main(string[] args)
        {
            // Print info how to get help unless this is a request
            // to display help.
            if (args.Length == 0 || (args[0] != "/?" && args[0] != "/help"))
            {
                Console.WriteLine("Starting.");
                Console.WriteLine("To display help, run:");
                Console.WriteLine("     TWIC Downloader /?");
                Console.WriteLine("");
            }

            if (!ProcessCommandLine(args))
                return;

            if (!downloadFromNo.HasValue)
            {
                // if the /from argument was not specified, read it from the lastDownloadedInfoFile 
                try
                {
                    string[] readInfo = File.ReadAllLines(Path.Combine(workingFolder, lastDownloadedInfoFile));
                    lastDownloadedNumber = long.Parse(readInfo[0]);
                }
                catch
                {
                    // the file does not exist or the content was invalid so we will use the default
                    useDefaultlastDownloadedNumber = true;
                }
                downloadFromNo = lastDownloadedNumber + 1;
            }

            //
            // Report all arguments
            //
            Console.WriteLine("Looking for TWIC archives on " + TwicArchiveUrl);
            Console.WriteLine("Earliest archive to look for: " + BuildTwicArchiveName(downloadFromNo.Value));
            if (useDefaultlastDownloadedNumber)
            {
                Console.WriteLine("     NOTE: the above is the default becase file TwicLatest.txt was not found");
                Console.WriteLine("           and parameter /from was not specified.");
            }

            if (downloadToNo.HasValue)
            {
                Console.WriteLine("Working directory: " + workingFolder);
            }

            Console.WriteLine("");


            tmpSubfolder = "_TMP_" + DateTime.Now.Ticks.ToString();
            tmpSubfolder = Path.Combine(workingFolder, tmpSubfolder);

            Console.WriteLine("Creating temporary subfolder " + tmpSubfolder);
            Console.WriteLine("It will be deleted upon exit.");
            Console.WriteLine("");

            try
            {
                Directory.CreateDirectory(tmpSubfolder);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to create temporaty folder.");
                Console.WriteLine(e.Message);
                Console.WriteLine("Exiting.");
                return;
            }

            DownloadTwicArchives();

            if (twicFilesDownloaded.Count == 0)
            {
                Console.WriteLine("Failed to download any new archives.");
                Console.WriteLine("If this is unexpected, check the parameters and Internet access to " + TwicArchiveUrl);
            }
            else
            {
                MergeDownloadedArchives();
                // update the content of lastDownloadedInfoFile
                File.WriteAllText(Path.Combine(workingFolder, lastDownloadedInfoFile), twicFilesDownloaded[twicFilesDownloaded.Count - 1].ToString());
            }


            Directory.Delete(tmpSubfolder, true);

        }

        static string BuildTwicArchiveName(long archiveNo)
        {
            return String.Format("twic{0}g.zip", archiveNo);
        }

        static bool ProcessCommandLine(string[] args)
        {
            // read command line parameters
            int n = 0;
            while (n < args.Length)
            {
                switch (args[n].ToLower())
                {
                    case "/help":
                    case "/?":
                        // we only honor the request to show help if this
                        // is the first argument.
                        if (n == 0)
                        {
                            PrintHelp();
                            return false;
                        }
                        break;
                    case "/from":
                        n++;
                        long fromVal;
                        if (long.TryParse(args[n], out fromVal))
                        {
                            downloadFromNo = fromVal;
                        }
                        else
                        {
                            Console.WriteLine("INVALID parameter /from. Must be a number.");
                            Console.WriteLine("Correct the parameter\'s value and run the program again.");
                            return false;
                        }
                        break;
                    case "/to":
                        n++;
                        long toVal;
                        if (long.TryParse(args[n], out toVal))
                        {
                            downloadToNo = toVal;
                        }
                        else
                        {
                            Console.WriteLine("INVALID parameter /to. Must be a number.");
                            Console.WriteLine("Correct the parameter\'s value and run the program again.");
                            return false;
                        }
                        break;
                    case "/dir":
                        n++;
                        if (Directory.Exists(args[n]))
                        {
                            workingFolder = args[n];
                        }
                        else
                        {
                            Console.WriteLine("INVALID parameter /dir. Must be an existing directory.");
                            Console.WriteLine("Correct the parameter\'s value or create the specified directory and run the program again.");
                            return false;
                        }
                        break;
                }
                n++;
            }

            return true;
        }

        static void DownloadTwicArchives()
        {
            Console.WriteLine("Downloading TWIC archives...");
            long currFileNo = downloadFromNo.Value;
            using (var client = new WebClient())
            {
                while (true)
                {
                    string twicFileName = BuildTwicArchiveName(currFileNo);
                    try
                    {
                        client.DownloadFile(TwicArchiveUrl + twicFileName, tmpSubfolder + "\\temp.zip");
                        try
                        {
                            ZipFile.ExtractToDirectory(tmpSubfolder + "\\temp.zip", tmpSubfolder + "\\Extract");
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("ERROR failed to unzip " + twicFileName);
                            Console.WriteLine(e.Message);
                            break;
                        }

                        twicFilesDownloaded.Add(currFileNo);
                        Console.WriteLine("   " + twicFileName);

                        currFileNo++;
                    }
                    catch
                    {
                        break;
                    }
                }
            }
        }


        static void MergeDownloadedArchives()
        {
            string twicComboFileName = "";

            try
            {

                twicFilesDownloaded.Sort();
                twicComboFileName = Path.Combine(workingFolder, 
                    "twicMerged_" + twicFilesDownloaded[0].ToString() + "_" + twicFilesDownloaded[twicFilesDownloaded.Count - 1].ToString() + ".pgn");

                string[] pgnFiles;
                pgnFiles = Directory.GetFiles(tmpSubfolder + "\\Extract", "*.pgn");
                using (StreamWriter writer = new StreamWriter(twicComboFileName))
                {
                    for (int i = 0; i < pgnFiles.Length; i++)
                    {
                        using (StreamReader reader = File.OpenText(pgnFiles[i]))
                        {
                            writer.Write(reader.ReadToEnd());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected merging downloads!");
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("");
            Console.WriteLine("Downloaded archives successfully merged.");
            Console.WriteLine("Your output is in " + twicComboFileName);
        }

        static void PrintHelp()
        {
            Console.WriteLine("TWIC Downloader command  line arguments: ");
            Console.WriteLine("   /help or /? - print this info");
            Console.WriteLine("   /dir        - working folder");
            Console.WriteLine("   /from nnnn  - number of the earliest twic pgn zip archive to download ");
            Console.WriteLine("   /to nnnn    - number of the latest twic pgn zip archive to download ");
            Console.WriteLine("");
            Console.WriteLine("Example: TWIC Downloader /dir . /from 1420 /to 1425");
            Console.WriteLine("NOTE: you can view available archives at https://theweekinchess.com/twic.");
            Console.WriteLine("");
            Console.WriteLine("If the /from argument is not specified,");
            Console.WriteLine("         the downloader will look for the number following the number");
            Console.WriteLine("         stored in TwicLatest.txt in the working folder.");
            Console.WriteLine("");
            Console.WriteLine("If the /to argument is not specified,");
            Console.WriteLine("         the downloader will look for ALL files with numbers greater than ");
            Console.WriteLine("         the one stored in TwicLatest.txt in the working folder or");
            Console.WriteLine("         specified by the /from argument");
        }
    }
}
