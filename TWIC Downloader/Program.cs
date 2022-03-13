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
        static void Main(string[] args)
        {
            foreach (string arg in args)
            {
                switch (arg.ToLower())
                {
                    case "/help":
                    case "/?":
                        PrintHelp();
                        break;
                }
            }

            // defaults
            string downloadFolder = ".";

            // file in whihc the number id of the last downloaded file is stored.
            string lastDownloadedInfoFile = "TwicLatest.txt";

            //long lastDownloadedNumber = 1414;
            long lastDownloadedNumber = 1424;

            // read command line parameters
            // ...

            try
            {
                string[] readInfo = File.ReadAllLines(downloadFolder + "\\" + lastDownloadedInfoFile);
                lastDownloadedNumber = long.Parse(readInfo[0]);
            }
            catch
            { }

            // need target directory
            // 
            // see if we have last downloaded file, if so check the id of the last downloaded file
            // keep downloading and uzipping until no newer file is found
            // 

            //            Console.WriteLine("Hello World!");

            long currFileNo = lastDownloadedNumber + 1;

            string tmpSubfolder = "_TMP_" + DateTime.Now.Ticks.ToString();
            tmpSubfolder = downloadFolder + "\\" + tmpSubfolder;
            Directory.CreateDirectory(tmpSubfolder);

            var twicFilesDownloaded = new List<long>();
            using (var client = new WebClient())
            {
                while (true)
                {
                    string twicFileName = String.Format("twic{0}g.zip", currFileNo);
                    try
                    {
                        client.DownloadFile("https://theweekinchess.com/zips/" + twicFileName, tmpSubfolder + "\\temp.zip");
                        ZipFile.ExtractToDirectory(tmpSubfolder + "\\temp.zip", tmpSubfolder + "\\Extract");
                        twicFilesDownloaded.Add(currFileNo);

                        currFileNo++;
                    }
                    catch
                    {
                        break;
                    }
                }
            }

            try
            {

                if (twicFilesDownloaded.Count > 0)
                {

                    twicFilesDownloaded.Sort();
                    string twicComboFileName = "twicMerged_" + twicFilesDownloaded[0].ToString() + "_" + twicFilesDownloaded[twicFilesDownloaded.Count - 1].ToString() + ".pgn";

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
                else
                {
                    Console.WriteLine("No new files to download.");
                    Console.WriteLine("Check the list of TWIC Downloads on https://theweekinchess.com/twic");
                    Console.WriteLine("   and adjust the list of command line arguments if necessary.");
                    Console.WriteLine("   Run: TWIC Downloader /? to see the list of command line arguments.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected error!");
                Console.WriteLine(e.Message);
            }

            Directory.Delete(tmpSubfolder, true);

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
            Console.WriteLine("NOTE: check https://theweekinchess.com/twic for available downloads.");
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
