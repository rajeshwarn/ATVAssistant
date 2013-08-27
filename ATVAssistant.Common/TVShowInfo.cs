using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ATVAssistant.Common
{
    /// <summary>
    /// Information about a media item
    /// </summary>
    public class TVShowInfo
    {
        /// <summary>
        /// The TV show name
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The TV show season number
        /// </summary>
        public int SeasonNumber
        {
            get;
            set;
        }

        /// <summary>
        /// The TV show Episode number within the season
        /// </summary>
        public int EpisodeNumber
        {
            get;
            set;
        }

        /// <summary>
        /// The TV show episode title
        /// </summary>
        public string EpisodeTitle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets media information about a given file
        /// </summary>
        /// <param name="basePath">The base path for all files processed with filebot</param>
        /// <param name="pathToMedia">The full path for the file to parse</param>
        /// <returns></returns>
        public static TVShowInfo FromPathInfo(string basePath, string pathToMedia)
        {
            TVShowInfo retval = new TVShowInfo();

            //  Parse the path information from the base path onward:
            string[] pathInfo = pathToMedia.Substring(basePath.Length).Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

            //  Parse the filename:
            string[] fileInfo = Path.GetFileNameWithoutExtension(pathToMedia).Split('-');
            string parsedEpisode = fileInfo[1].Trim().Split('E')[1].Trim();
            string episodeTitle = fileInfo[2].Trim();

            //  Set TV show:
            retval.Name = pathInfo[0];

            //  Set season:
            string parsedSeason = Regex.Replace(pathInfo[1], "[^0-9.]", "");
            int parsedSeasonNumber = 0;

            if(int.TryParse(parsedSeason, out parsedSeasonNumber))
            {
                retval.SeasonNumber = parsedSeasonNumber;
            }

            //  Set Episode number
            int parsedEpisodeNumber = 0;

            if(int.TryParse(parsedEpisode, out parsedEpisodeNumber))
            {
                retval.EpisodeNumber = parsedEpisodeNumber;
            }

            //  Set Episode title:
            retval.EpisodeTitle = episodeTitle;

            //  Return our MediaInfo object:
            return retval;
        }
    }
}
