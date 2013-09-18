using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ServiceStack.Text;

namespace ATVAssistant.Common
{
    /// <summary>
    /// Gets information for a given media item using the iTunes search API.  
    /// For more informaiton, see 
    /// http://www.apple.com/itunes/affiliates/resources/documentation/itunes-store-web-service-search-api.html 
    /// </summary>
    [DataContract]
    public class iTunesMedia
    {
        private static string _baseSearchUrl = "https://itunes.apple.com/search?{0}";

        /// <summary>
        /// The artwork url returned for the item
        /// </summary>
        [DataMember(Name = "artworkUrl100")]
        public string ArtworkUrl
        {
            get;
            set;
        }

        /// <summary>
        /// The reformatted url (to get the large version of the artwork)
        /// </summary>
        public string LargeArtworkUrl
        {
            get
            {
                return this.ArtworkUrl.Replace("100x100", "600x600");
            }
        }

        /// <summary>
        /// For TV shows, this is the show name.  For movies, this is the director
        /// </summary>
        [DataMember(Name = "artistName")]
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// For movies, this is the title of the movie
        /// </summary>
        [DataMember(Name = "trackName")]
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// For TV shows this will include show name and season information
        /// </summary>
        [DataMember(Name = "collectionName")]
        public string CollectionName
        {
            get;
            set;
        }

        /// <summary>
        /// For TV shows, this is the season number
        /// </summary>
        public int ShowSeasonNumber
        {
            get;
            set;
        }

        /// <summary>
        /// Represents Apple's advice on what this content should be rated
        /// (TV-14, TV-MA, etc)
        /// </summary>
        [DataMember(Name = "contentAdvisoryRating")]
        public string Rating
        {
            get;
            set;
        }

        /// <summary>
        /// Media genre
        /// </summary>
        [DataMember(Name = "primaryGenreName")]
        public string Genre { get; set; }

        /// <summary>
        /// Release date
        /// </summary>
        [DataMember(Name = "releaseDate")]
        public DateTime ReleaseDate { get; set; }

        /// <summary>
        /// Get media information for a TV show
        /// </summary>
        /// <param name="showName"></param>
        /// <param name="season"></param>
        /// <returns></returns>
        public static List<iTunesMedia> ForTVShow(string showName, int season)
        {
            List<iTunesMedia> retval = new List<iTunesMedia>();
            var nvc = HttpUtility.ParseQueryString(string.Empty);

            //  Include the season in the search
            nvc.Add("term", showName + " " + season);

            //  Set attributes for a TV season.  
            nvc.Add("media", "tvShow");
            nvc.Add("entity", "tvSeason");
            nvc.Add("limit", "5");

            //  Format the url
            string fullUrl = string.Format(_baseSearchUrl, nvc.ToString());

            //  Call the service and get the results:
            iTunesMediaResult serviceResult = fullUrl.GetJsonFromUrl().Trim().FromJson<iTunesMediaResult>();

            //  If we didn't get anything back, try without the season
            if(serviceResult.ResultCount < 1)
            {
                nvc["term"] = showName;

                //  Format the url
                fullUrl = string.Format(_baseSearchUrl, nvc.ToString());

                //  Call the service and get the results:
                serviceResult = fullUrl.GetJsonFromUrl().Trim().FromJson<iTunesMediaResult>();
            }

            //  Set the results:
            retval = serviceResult.Results;

            return retval;
        }

        /// <summary>
        /// Get media information for a movie
        /// </summary>
        /// <param name="movieName"></param>
        /// <returns></returns>
        public static List<iTunesMedia> ForMovie(string movieName)
        {
            List<iTunesMedia> retval = new List<iTunesMedia>();
            var nvc = HttpUtility.ParseQueryString(string.Empty);

            //  Set attributes for a movie.  
            nvc.Add("term", movieName);
            nvc.Add("media", "movie");
            nvc.Add("entity", "movie");
            nvc.Add("limit", "5");

            //  Format the url
            string fullUrl = string.Format(_baseSearchUrl, nvc.ToString());

            //  Call the service and get the results:
            iTunesMediaResult serviceResult = fullUrl.GetJsonFromUrl().Trim().FromJson<iTunesMediaResult>();

            //  Set the results:
            retval = serviceResult.Results;

            return retval;
        }
    }
}
