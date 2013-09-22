using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ATVAssistant.Common;
using PushoverClient;

namespace RipRobot
{
    /// <summary>
    /// This robot is meant to be called as an AutoPlay action in Windows.
    /// It automatically determines the movie inserted, rips / encodes / tags
    /// imports into iTunes (or at least stores for later).
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            #region DVD ready check
            
            //  Get DVD settings from args and drive information
            string dvdDrive = string.Empty;
            if(args.Any())
            {
                dvdDrive = args[0].Replace("\"", "");
            }
            else
            {
                dvdDrive = ConfigurationManager.AppSettings["DvdDrive"];
            }

            //  Wait for the drive to become available:
            int tries = 0;
            while(tries < 4 && !DriveReady(dvdDrive))
            {
                //  Increment the number of tries
                tries++;

                Console.WriteLine("Waiting for {0} to become ready.  Attempt {1}", dvdDrive, tries);

                //  Sleep for 30 seconds:
                Thread.Sleep(TimeSpan.FromSeconds(30));
            }

            //  If the drive still isn't ready, print a message and get out:
            if(!DriveReady(dvdDrive))
            {
                Console.WriteLine("I don't think {0} is going to be ready anytime soon.  Exiting.", dvdDrive);
                return;
            } 

            #endregion

            #region Load settings

            string dvdBasePath = Path.Combine(dvdDrive + Path.DirectorySeparatorChar, "VIDEO_TS");
            string dvdVolume = (from drive in DriveInfo.GetDrives()
                                where drive.Name == dvdDrive + @"\"
                                select drive.VolumeLabel).FirstOrDefault();

            //  Get the base path for source files:
            string baseProcessingPath = ConfigurationManager.AppSettings["BaseProcessingPath"];
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //  Get the Handbrake & tcclone settings:
            string handbrakeSwitches = ConfigurationManager.AppSettings["HandbrakeSwitches"];
            int handbrakeTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["HandbrakeTimeout"]);
            int tcCloneTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["tcCloneTimeout"]);

            //  Get the AtomicParsley settings:
            int atomicParsleyTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["AtomicParsleyTimeout"]);

            //  Get after processing path:
            string afterProcessingPath = ConfigurationManager.AppSettings["AfterProcessingPath"];

            //  Get artwork Path
            string artworkBasePath = ConfigurationManager.AppSettings["ArtworkBasePath"];

            //  Pushover settings
            string pushoverAppKey = ConfigurationManager.AppSettings["PushoverAppKey"];
            string pushoverUserKey = ConfigurationManager.AppSettings["PushoverUserKey"];

            #endregion

            #region Get DVD information

            //  Get DVD information
            DVDInfo di = DVDInfo.ForDVD();

            //  If we have basic information, get media information:
            MovieMetaInfo movieInfo = null;
            if(di != null)
            {
                Console.WriteLine("Found disc information for: {0} -- looking for movie information", di.Title);
                MovieMetaInfoManager mgr = new MovieMetaInfoManager(artworkBasePath);
                movieInfo = mgr.FindMovieInfo(di.Title);
            }
            else
            {
                Console.WriteLine("Disc information not found!");
            }

            #endregion

            #region Encode with Handbrake or rip to disk

            //  If we have media information, proceed with encode
            string handbrakeOutput = string.Empty;
            if(movieInfo != null)
            {
                Console.WriteLine("Found movie information for {0} (made in {1}).  Attempting to encode", movieInfo.Name, movieInfo.Year);

                //  Determine output path for Handbrake 
                handbrakeOutput = Path.Combine(
                    baseProcessingPath,
                    di.Title + ".m4v"
                    );

                //  Process in Handbrake and wait (using timeout)
                ProcessStartInfo handbrakePInfo = new ProcessStartInfo();

                handbrakePInfo.Arguments = string.Format("-i \"{0}\" -o \"{1}\" {2}",
                    dvdBasePath,
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
            }
            else
            {
                Console.WriteLine("We don't have movie information for disc volume {0}. Attempting to rip", dvdVolume);

                //  Otherwise, just rip to disk
                string ripPath = Path.Combine(baseProcessingPath, dvdVolume, "VIDEO_TS"); 

                //  Make sure the rip destination exists first:
                if(!Directory.Exists(ripPath))
                {
                    Directory.CreateDirectory(ripPath);
                }

                //  Copy files from the DVD to the rip path:
                foreach(string sourceFileName in Directory.EnumerateFiles(dvdBasePath, "*.*"))
                {
                    string destFileName = Path.Combine(ripPath, Path.GetFileName(sourceFileName));
                    File.Copy(sourceFileName, destFileName, true);
                }
            }

            #endregion

            #region If we encoded, add meta data and add to iTunes

            //  If we have movie information and our encoded file exists...
            if(movieInfo != null && File.Exists(handbrakeOutput))
            { 
                //  Add meta information using Atomicparsley
                ProcessStartInfo apPInfo = new ProcessStartInfo();

                apPInfo.Arguments = string.Format(
                    "\"{0}\" --title \"{1}\" --year \"{2}\" --genre \"{3}\" --stik \"Short Film\" --description \"Library\" --artwork \"{4}\" --contentRating \"{5}\" --overWrite",
                    handbrakeOutput,
                    movieInfo.Name,
                    movieInfo.Year,
                    movieInfo.Genre,
                    movieInfo.ArtworkLocation,
                    movieInfo.Rating
                    );

                apPInfo.FileName = Path.Combine(currentPath, "AtomicParsley.exe");

                Process apProcess = Process.Start(apPInfo);
                apProcess.WaitForExit(atomicParsleyTimeout);

                //  If it hasn't exited, but it's not responding...
                //  kill the process
                if(!apProcess.HasExited && !apProcess.Responding)
                {
                    apProcess.Kill();
                }

                //  Move file to post processing area (most likely adding to iTunes)
                string afterProcessingFullPath = Path.Combine(afterProcessingPath, Path.GetFileName(handbrakeOutput));
                File.Move(handbrakeOutput, afterProcessingFullPath);
            }

            #endregion

            #region Send push notification

            //  If we have media information, we just encoded the file
            if(movieInfo != null)
            {
                //  Send a push notification about the specific movie just added
                if(!string.IsNullOrEmpty(pushoverAppKey) && !string.IsNullOrEmpty(pushoverUserKey))
                {
                    //  Create the push client
                    Pushover pushClient = new Pushover(pushoverAppKey);

                    //  Format the message
                    string message = "Movie is ready to watch";

                    pushClient.Push(movieInfo.Name, message, pushoverUserKey);
                }

            }
            else
            { 
                //  Send a push notification about the rip 
                if(!string.IsNullOrEmpty(pushoverAppKey) && !string.IsNullOrEmpty(pushoverUserKey))
                {
                    //  Create the push client
                    Pushover pushClient = new Pushover(pushoverAppKey);

                    //  Format the message
                    string message = "Disc has been ripped";

                    pushClient.Push(dvdVolume, message, pushoverUserKey);
                }
            }

            #endregion
        }

        /// <summary>
        /// Check to see if a drive is ready
        /// </summary>
        /// <param name="driveName">A drive name.  Example: D:\</param>
        /// <returns></returns>
        static bool DriveReady(string driveName)
        { 
            bool retval = false;

            retval = (from drive in DriveInfo.GetDrives()
                      where drive.Name == driveName + @"\"
                      && drive.IsReady == true
                      select drive).Any();

            return retval;
        }
    }
}
