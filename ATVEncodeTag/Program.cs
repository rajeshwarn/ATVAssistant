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

            //  Parse commandline options
            Options options = new Options();
            if(CommandLine.Parser.Default.ParseArguments(args, options))
            {
                //  First, see if the passed filename is in our base directory:
                if(!options.FileToProcess.StartsWith(basePath))
                    Console.WriteLine("The passed file is not in the base path of {0}", basePath);

                //  Next, parse our directory structure:
                TVShowInfo showInfo = TVShowInfo.FromPathInfo(basePath, options.FileToProcess);

                #region Get show meta information (artwork, ratings, etc)
                
                //  Figure out meta information location (artwork / ratings)
                //  Get artwork and ratings for the show / season
                string showInfoFullPath = Path.Combine(currentPath, showMetaInfoFile);
                TVShowMetaInfoManager metaManager = new TVShowMetaInfoManager(showInfoFullPath, artworkBasePath);
                TVShowMetaInfo metaShowInfo = metaManager.FindShowInfo(showInfo.Name, showInfo.SeasonNumber);

                #endregion
                
                #region Encode with Handbrake

                //  Determine output path for Handbrake 
                string handbrakeOutput = Path.Combine(
                    Path.GetDirectoryName(options.FileToProcess),
                    Path.GetFileNameWithoutExtension(options.FileToProcess) + ".m4v"
                    );

                //  Process in Handbrake and wait (using timeout)
                ProcessStartInfo handbrakePInfo = new ProcessStartInfo();

                handbrakePInfo.Arguments = string.Format("-i \"{0}\" -o \"{1}\" {2}",
                    options.FileToProcess,
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
                        "\"{0}\" --genre \"TV Shows\" --stik \"TV Show\" --TVShowName \"{1}\" --TVEpisode \"{2}{3}\" --TVSeasonNum {2} --TVEpisodeNum {3} --artist \"{1}\" --title \"{4}\" --contentRating \"{5}\" --artwork \"{6}\" --overWrite",
                        handbrakeOutput,
                        showInfo.Name,
                        showInfo.SeasonNumber,
                        showInfo.EpisodeNumber,
                        showInfo.EpisodeTitle,
                        metaShowInfo.Rating,
                        metaShowInfo.ArtworkLocation
                        );
                }
                else
                {
                    //  Otherwise, use simpler arguments
                    apPInfo.Arguments = string.Format(
                        "\"{0}\" --genre \"TV Shows\" --stik \"TV Show\" --TVShowName \"{1}\" --TVEpisode \"{2}{3}\" --TVSeasonNum {2} --TVEpisodeNum {3} --artist \"{1}\" --title \"{4}\" --overWrite",
                        handbrakeOutput,
                        showInfo.Name,
                        showInfo.SeasonNumber,
                        showInfo.EpisodeNumber,
                        showInfo.EpisodeTitle
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

                //  Remove original file (it shouldn't be needed anymore)
                if(File.Exists(options.FileToProcess))
                {
                    File.Delete(options.FileToProcess);
                } 

                #endregion

                #region Send notification message
                
                if(!string.IsNullOrEmpty(pushoverAppKey) && !string.IsNullOrEmpty(pushoverUserKey))
                {
                    //  Create the push client
                    Pushover pushClient = new Pushover(pushoverAppKey, pushoverUserKey);

                    //  Format the message
                    string message = string.Format(
                        "Season {0} Episode {1} (\"{2}\") is ready to watch",
                        showInfo.SeasonNumber,
                        showInfo.EpisodeNumber,
                        showInfo.EpisodeTitle);

                    pushClient.Push(showInfo.Name, message);
                } 

                #endregion
            }
        }
    }
}
