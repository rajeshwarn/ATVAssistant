﻿using System;
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
            #region Load settings

            //  Get DVD settings from args and drive information
            string dvdDrive = args[0].Replace("\"", "");
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
                MovieMetaInfoManager mgr = new MovieMetaInfoManager(artworkBasePath);
                movieInfo = mgr.FindMovieInfo(di.Title);
            }

            #endregion

            #region Encode with Handbrake or rip to disk

            //  If we have media information, proceed with encode
            string handbrakeOutput = string.Empty;
            if(movieInfo != null)
            {
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
                //  Otherwise, just rip to disk
                string ripPath = Path.Combine(baseProcessingPath, dvdVolume); 

                //  Make sure the rip destination exists first:
                if(!Directory.Exists(ripPath))
                {
                    Directory.CreateDirectory(ripPath);
                }

                //  Start the clone process:
                ProcessStartInfo tcclonePInfo = new ProcessStartInfo();

                tcclonePInfo.Arguments = string.Format("--outpath \"{0}\" \"{1}\" all",
                    ripPath,
                    dvdBasePath);

                tcclonePInfo.FileName = Path.Combine(currentPath, "tcclone.exe");

                Process tcCloneProcess = Process.Start(tcclonePInfo);
                tcCloneProcess.WaitForExit(tcCloneTimeout);

                //  If it hasn't exited, but it's not responding...
                //  kill the process
                if(!tcCloneProcess.HasExited && !tcCloneProcess.Responding)
                {
                    tcCloneProcess.Kill();
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
    }
}
