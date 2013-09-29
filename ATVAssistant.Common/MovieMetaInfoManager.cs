using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ATVAssistant.Common
{
    public class MovieMetaInfoManager
    {
        public string ArtworkBasePath { get; set; }

        public MovieMetaInfoManager(string artworkBasePath)
        {
            //  If the artwork path doesn't exist already, create it:
            this.ArtworkBasePath = artworkBasePath;
            if(!Directory.Exists(artworkBasePath))
            {
                Directory.CreateDirectory(artworkBasePath);
            }
        }

        /// <summary>
        /// Finds movie information and returns it
        /// </summary>
        /// <param name="movieName"></param>
        /// <returns></returns>
        public MovieMetaInfo FindMovieInfo(string movieName)
        {
            MovieMetaInfo retval = null;

            var iTunesItem = iTunesMedia.ForMovie(movieName).FirstOrDefault();

            //  If we found a result
            if(iTunesItem != null)
            {
                try
                {
                    //  Save the artwork to the base path
                    Uri uri = new Uri(iTunesItem.LargeArtworkUrl);
                    string artworkFilename = Path.GetFileName(uri.LocalPath);

                    //  Save the artwork with the show and season number as the filename
                    string savedArtworkPath = Path.Combine(this.ArtworkBasePath,
                        string.Format("{0}{1}", Regex.Replace(movieName, @"[\W]", ""), Path.GetExtension(artworkFilename))
                        );
                    WebClient web = new WebClient();
                    web.DownloadFile(iTunesItem.LargeArtworkUrl, savedArtworkPath);

                    //  Create a new movie information object:
                    MovieMetaInfo newMovieInfo = new MovieMetaInfo()
                    {
                        Name = iTunesItem.Title,
                        ArtworkLocation = savedArtworkPath,
                        Rating = iTunesItem.Rating,
                        Genre = iTunesItem.Genre,
                        Year = iTunesItem.ReleaseDate.Year
                    };

                    retval = newMovieInfo;
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Bad things happened when trying to get movie information from iTunes: {0}", ex.Message);
                }
            }

            return retval;
        }
    }
}
