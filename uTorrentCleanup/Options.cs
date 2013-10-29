using CommandLine;
using CommandLine.Text;

namespace uTorrentCleanup
{
    class Options
    {
        /// <summary>
        /// The new state of the torrent.  If this is finished, the torrent should be removed
        /// </summary>
        [Option('s', "state", HelpText = "The torrent state.", Required = true)]
        public int TorrentState { get; set; }

        /// <summary>
        /// The torrent hash (that identifies this torrent in uTorrent)
        /// </summary>
        [Option('h', "hash", HelpText = "The torrent info hash", Required = true)]
        public string TorrentHash { get; set; }

        [HelpOption('?', "help", HelpText = "Show this help screen")]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("uTorrent cleanup", "0.1"),
                Copyright = new CopyrightInfo("Dan Esparza", 2013),
                AddDashesToOption = true
            };

            help.AddOptions(this);

            return help;
        }
    }

    /// <summary>
    /// Torrent states
    /// </summary>
    enum TorrentState
    {
        Error = 1,
        Checked = 2,
        Paused = 3,
        SuperSeeding = 4,
        Seeding = 5,
        Downloading = 6,
        SuperSeedF = 7,
        SeedingF = 8,
        DownloadingF = 9,
        QueuedSeed = 10,
        Finished = 11,
        Queued = 12,
        Stopped = 13
    }
}
