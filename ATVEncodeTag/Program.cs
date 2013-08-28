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
            //  Get the base path for source files:
            string basePath = ConfigurationManager.AppSettings["BasePath"];
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //  Get the Handbrake settings:
            string handbrakeSwitches = ConfigurationManager.AppSettings["HandbrakeSwitches"];
            int handbrakeTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["HandbrakeTimeout"]);

            //  Parse commandline options
            Options options = new Options();
            if(CommandLine.Parser.Default.ParseArguments(args, options))
            {
                //  First, see if the passed filename is in our base directory:
                if(!options.FileToProcess.StartsWith(basePath))
                    Console.WriteLine("The passed file is not in the base path of {0}", basePath);

                //  Next, parse our directory structure:
                TVShowInfo showInfo = TVShowInfo.FromPathInfo(basePath, options.FileToProcess);

                //  Figure out meta information location (artwork / ratings)
                //  Get artwork and ratings for the show / season

                //  Determine output path for Handbrake 
                string handbrakeOutput = Path.Combine(
                    Path.GetDirectoryName(options.FileToProcess),
                    Path.GetFileNameWithoutExtension(options.FileToProcess) + ".m4v"
                    );

                //  Process in Handbrake and wait (using timeout)
                ProcessStartInfo pInfo = new ProcessStartInfo();
                
                pInfo.Arguments = string.Format("-i \"{0}\" -o \"{1}\" {2}", 
                    options.FileToProcess, 
                    handbrakeOutput, 
                    handbrakeSwitches);

                pInfo.FileName = Path.Combine(currentPath, "HandBrakeCLI.exe");
                
                Process process = Process.Start(pInfo);
                process.WaitForExit(handbrakeTimeout);

                //  If it hasn't exited, but it's not responding...
                //  kill the process
                if(!process.HasExited && !process.Responding)
                {
                    process.Kill();
                }

                //  Process in AtomicParsley and wait

                //  Move to after processing path

                //  Remove original file (it shouldn't be needed anymore)

                //  Send notification using Pushover

            }
        }
    }
}
