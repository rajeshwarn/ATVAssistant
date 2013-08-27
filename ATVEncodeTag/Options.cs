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
        [Option('f', "file", HelpText="The media file to process")]
        public string FileToProcess
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates the file should be encoded with Handbrake
        /// </summary>
        [Option('e', "encode", HelpText = "Indicates the file should be encoded with Handbrake")]
        public bool Encode
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates the file should be tagged using AtomicParsley
        /// </summary>
        [Option('t', "tag", HelpText = "Indicates the file should be tagged using AtomicParsley")]
        public bool Tag
        {
            get;
            set;
        }

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
