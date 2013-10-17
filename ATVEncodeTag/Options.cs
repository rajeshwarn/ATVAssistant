using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace ATVEncodeTag
{
    public class Options
    {
        /// <summary>
        /// The file to process
        /// </summary>
        [Option('f', "file", HelpText="The media file to process", Required=false)]
        public string FileToProcess { get; set; }

        /// <summary>
        /// The directory containing the media files to process
        /// </summary>
        [Option('d', "directory", HelpText = "The directory containing media files to process", Required=false)]
        public string DirectoryToProcess { get; set; }

        /// <summary>
        /// Processing single file or multiple files, or check episode information.  Values:  single/multi/check
        /// </summary>
        [Option('t', "type", HelpText = "Processing type: single/multi/check", Required = false)]
        public string ProcessType { get; set; }

        [HelpOption('?', "help", HelpText="Show this help screen")]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("ATV Assistant", "0.1"),
                Copyright = new CopyrightInfo("Dan Esparza", 2013),
                AddDashesToOption = true
            };

            help.AddOptions(this);

            return help;
        }
    }
}
