using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            //  Get the base path:
            string basePath = ConfigurationManager.AppSettings["BasePath"];

            //  Parse commandline options
            Options options = new Options();
            if(CommandLine.Parser.Default.ParseArguments(args, options))
            {
                //  First, see if the passed filename is in our base directory:
                if(!options.FileToProcess.StartsWith(basePath))
                    Console.WriteLine("The passed file is not in the base path of {0}", basePath);

                //  Next, parse our directory structure:
                MediaInfo mInfo = MediaInfo.FromPathInfo(basePath, options.FileToProcess);

                //  Print out information we know now:
                Console.WriteLine("TV show: {0}, Season: {1}", mInfo.TvShow, mInfo.TvShowSeasonNumber);
            }
        }
    }
}
