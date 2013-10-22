using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ATVAssistant.Common;
using PushoverClient;
using ShowInfo;
using ShowInfoProvider;

namespace ATVEncodeTag
{
    /// <summary>
    /// This assistant encodes and tags media (specifically TV shows) so that they
    /// can be imported easily into iTunes.  This is meant to be part of a larger set
    /// of utilities to automatically manage a media collection.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            //  Parse commandline options
            Options options = new Options();
            if(CommandLine.Parser.Default.ParseArguments(args, options))
            {
                switch(options.ProcessType)
                {
                    case "single":

                        //  Process the single file
                        ProcessFile(Path.Combine(options.DirectoryToProcess, options.FileToProcess));

                        break;
                    case "multi":

                        //  Find out what extensions to look for
                        string searchPatternsString = ConfigurationManager.AppSettings["MediaFileSearchPatterns"].ToString();
                        string[] seperators = { "|" };
                        var searchPatterns = searchPatternsString.Split(seperators, StringSplitOptions.RemoveEmptyEntries);

                        //  Find all the relevant files in the passed directory
                        IEnumerable<string> allFilesToProcess = DirectoryHelper.GetFiles(options.DirectoryToProcess, searchPatterns, SearchOption.AllDirectories);

                        //  Call 'ProcessFile for each file:
                        foreach(string fileToProcess in allFilesToProcess)
                        {
                            ProcessFile(fileToProcess);
                        }

                        break;
                    case "check":

                        //  Check the single file:
                        CheckFile(Path.Combine(options.DirectoryToProcess, options.FileToProcess));

                        break;
                }
            }
        }

        /// <summary>
        /// Process a single file
        /// </summary>
        /// <param name="fileName">The file to process</param>
        private static void ProcessFile(string fileName)
        {
            #region Load settings

            //  Get the base path for source files:
            string basePath = ConfigurationManager.AppSettings["BasePath"];
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //  Get the Handbrake settings:
            string handbrakeSwitches = ConfigurationManager.AppSettings["HandbrakeSwitches"];
            int handbrakeTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["HandbrakeTimeout"]);

            //  Get the AtomicParsley settings:
            int atomicParsleyTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["AtomicParsleyTimeout"]);

            //  Get after processing path:
            string afterProcessingPath = ConfigurationManager.AppSettings["AfterProcessingPath"];

            //  Get artwork Path
            string artworkBasePath = ConfigurationManager.AppSettings["ArtworkBasePath"];

            //  Get show info filename
            string showMetaInfoFile = ConfigurationManager.AppSettings["ShowMetaInfoFile"];

            //  Pushover settings
            string pushoverAppKey = ConfigurationManager.AppSettings["PushoverAppKey"];
            string pushoverUserKey = ConfigurationManager.AppSettings["PushoverUserKey"];

            #endregion

            #region Get show & episode information

            ShowInformationManager mgr = new ShowInformationManager();
            TVEpisodeInfo episodeInfo = mgr.GetEpisodeInfoForFilename(fileName);

            #endregion

            #region Get show meta information (artwork, ratings, etc)

            //  Figure out meta information location (artwork / ratings)
            //  Get artwork and ratings for the show / season
            string showInfoFullPath = Path.Combine(currentPath, showMetaInfoFile);
            TVShowMetaInfoManager metaManager = new TVShowMetaInfoManager(showInfoFullPath, artworkBasePath);
            TVShowMetaInfo metaShowInfo = null;
            
            //  If we found show & episode information, look for meta information
            if(episodeInfo != null)
                metaShowInfo = metaManager.FindShowInfo(episodeInfo.ShowName, episodeInfo.SeasonNumber);

            #endregion

            #region Encode with Handbrake

            //  Determine output path for Handbrake 
            string handbrakeOutput = Path.Combine(
                Path.GetDirectoryName(fileName),
                Path.GetFileNameWithoutExtension(fileName) + ".m4v"
                );

            //  Process in Handbrake and wait (using timeout)
            ProcessStartInfo handbrakePInfo = new ProcessStartInfo();

            handbrakePInfo.Arguments = string.Format("-i \"{0}\" -o \"{1}\" {2}",
                fileName,
                handbrakeOutput,
                handbrakeSwitches);

            handbrakePInfo.FileName = Path.Combine(currentPath, "HandBrakeCLI.exe");

            Process handbrakeProcess = Process.Start(handbrakePInfo);
            handbrakeProcess.WaitForExit(handbrakeTimeout);

            //  If it hasn't exited, but it's not responding...
            //  kill the process
            if(!handbrakeProcess.HasExited && !handbrakeProcess.Responding)
            {
                handbrakeProcess.Kill();
            }

            #endregion

            #region Process with AtomicParsley

            //  Process in AtomicParsley and wait
            ProcessStartInfo apPInfo = new ProcessStartInfo();

            //  If we have ratings information & artwork
            if(metaShowInfo != null)
            {
                //  Use the ratings and artwork information
                apPInfo.Arguments = string.Format(
                    "\"{0}\" --genre \"TV Shows\" --stik \"TV Show\" --TVShowName \"{1}\" --TVEpisode \"{2}{3}\" --TVSeasonNum {2} --TVEpisodeNum {3} --artist \"{1}\" --title \"{4}\" --description \"{7}\" --contentRating \"{5}\" --artwork \"{6}\" --overWrite",
                    handbrakeOutput,
                    episodeInfo.ShowName,
                    episodeInfo.SeasonNumber,
                    episodeInfo.EpisodeNumber,
                    episodeInfo.EpisodeTitle,
                    metaShowInfo.Rating,
                    metaShowInfo.ArtworkLocation,
                    GetEpisodeSummary(episodeInfo.EpisodeSummary)
                    );
            }
            else if(episodeInfo != null)
            {
                //  Otherwise, use simpler arguments
                apPInfo.Arguments = string.Format(
                    "\"{0}\" --genre \"TV Shows\" --stik \"TV Show\" --TVShowName \"{1}\" --TVEpisode \"{2}{3}\" --TVSeasonNum {2} --TVEpisodeNum {3} --artist \"{1}\" --title \"{4}\" --description \"{5}\" --overWrite",
                    handbrakeOutput,
                    episodeInfo.ShowName,
                    episodeInfo.SeasonNumber,
                    episodeInfo.EpisodeNumber,
                    episodeInfo.EpisodeTitle,
                    GetEpisodeSummary(episodeInfo.EpisodeSummary)
                    );
            }

            apPInfo.FileName = Path.Combine(currentPath, "AtomicParsley.exe");

            Process apProcess = Process.Start(apPInfo);
            apProcess.WaitForExit(atomicParsleyTimeout);

            //  If it hasn't exited, but it's not responding...
            //  kill the process
            if(!apProcess.HasExited && !apProcess.Responding)
            {
                apProcess.Kill();
            }

            #endregion

            #region After processing

            //  If our output file exists, move to after processing path
            if(File.Exists(handbrakeOutput))
            {
                string afterProcessingFullPath = Path.Combine(afterProcessingPath, Path.GetFileName(handbrakeOutput));
                File.Move(handbrakeOutput, afterProcessingFullPath);
            }

            #endregion

            #region Send notification message

            if(!string.IsNullOrEmpty(pushoverAppKey) && !string.IsNullOrEmpty(pushoverUserKey))
            {
                //  Create the push client
                Pushover pushClient = new Pushover(pushoverAppKey);

                //  Format the message
                if(episodeInfo != null)
                {
                    string message = string.Format(
                        "Season {0} Episode {1} (\"{2}\") is ready to watch",
                        episodeInfo.SeasonNumber,
                        episodeInfo.EpisodeNumber,
                        episodeInfo.EpisodeTitle);

                    pushClient.Push(episodeInfo.ShowName, message, pushoverUserKey);
                }
                else
                {
                    pushClient.Push(fileName, "File added to iTunes", pushoverUserKey);
                }
                
            }

            #endregion
        }

        /// <summary>
        /// Checks to see what episode information is found for the given filename
        /// </summary>
        /// <param name="fileName"></param>
        private static void CheckFile(string fileName)
        {
            Console.WriteLine("Looking for episode information for {0}...", fileName);

            ShowInformationManager mgr = new ShowInformationManager();
            TVEpisodeInfo episodeInfo = null;

            try
            {
                episodeInfo = mgr.GetEpisodeInfoForFilename(fileName);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Looks like there was a problem getting episode information: {0}", ex.Message);
            }

            if(episodeInfo == null)
                Console.WriteLine("Couldn't find episode information for {0}", fileName);
            else
                Console.WriteLine("Found episode information for: {0}\nShow:{1}\nSeason {2} Episode {3}: {4}\nSummary:{5}\n", fileName, episodeInfo.ShowName, episodeInfo.SeasonNumber, episodeInfo.EpisodeNumber, episodeInfo.EpisodeTitle, GetEpisodeSummary(episodeInfo.EpisodeSummary));

        }

        /// <summary>
        /// Preps the episode summary to be used by AtomicParsley
        /// </summary>
        /// <param name="summary"></param>
        /// <returns></returns>
        private static string GetEpisodeSummary(string summary)
        { 
            string retval = summary.Trim();
            
            //  If we actually have a summary
            if(!string.IsNullOrWhiteSpace(retval))
            {
                //  If it's too long ... 
                if(retval.Length > 250)
                {
                    //  Shorten it and trim any leading / trailing whitespace
                    retval = retval.Substring(0, 250).Trim();
                }
            }

            return retval;
        }
    }
}
