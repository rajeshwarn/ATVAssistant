using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace ATVAssistant.Common
{
    public class TVShowMetaInfoManager
    {
        /// <summary>
        /// The list of show information
        /// </summary>
        public List<TVShowMetaInfo> Shows
        {
            get;
            set;
        }

        /// <summary>
        /// The storage location for the tv show meta information file. 
        /// This is the full path, including the file name
        /// </summary>
        public string MetaInformationFile
        {
            get;
            set;
        }

        /// <summary>
        /// The base path to store show artwork in
        /// </summary>
        public string ArtworkBasePath
        {
            get;
            set;
        }

        public TVShowMetaInfoManager(string showInfoFile, string artworkBasePath)
        {
            this.Shows = new List<TVShowMetaInfo>();
            this.MetaInformationFile = showInfoFile;

            //  If the file exists...
            if(File.Exists(showInfoFile))
            {
                //  Open the file and read the text
                string fileData = File.ReadAllText(showInfoFile);

                //  Deserialize the text
                this.Shows = fileData.FromJson<List<TVShowMetaInfo>>();
            }

            //  If the artwork path doesn't exist already, create it:
            this.ArtworkBasePath = artworkBasePath;
            if(!Directory.Exists(artworkBasePath))
            {
                Directory.CreateDirectory(artworkBasePath);
            }
        }

        /// <summary>
        /// Returns the information for the specified show and season, or null if 
        /// no information was found.
        /// </summary>
        /// <param name="showName"></param>
        /// <param name="season"></param>
        /// <returns></returns>
        public TVShowMetaInfo FindShowInfo(string showName, int season)
        {
            TVShowMetaInfo retval = null;

            //  First, try to use the cached information
            retval = this.Shows.Where(s => s.Name == showName && s.Season == season).FirstOrDefault();

            if(retval == null)
            {
                //  If we can't find the show, 
                //  look for it in iTunes and get information
                var iTunesItem = iTunesMedia.ForTVShow(showName, season).FirstOrDefault();

                //  If we found a result
                if(iTunesItem != null)
                {
                    //  Save the artwork to the base path
                    Uri uri = new Uri(iTunesItem.LargeArtworkUrl);
                    string artworkFilename = Path.GetFileName(uri.LocalPath);
                    string savedArtworkPath = Path.Combine(this.ArtworkBasePath, artworkFilename);
                    WebClient web = new WebClient();
                    web.DownloadFile(iTunesItem.LargeArtworkUrl, savedArtworkPath);

                    //  Create a new show information object
                    TVShowMetaInfo newShowInfo = new TVShowMetaInfo()
                    {
                        Name = showName,
                        ArtworkLocation = savedArtworkPath,
                        Rating = iTunesItem.Rating,
                        Season = season,
                    };

                    //  Add the show, save the file
                    this.Shows.Add(newShowInfo);
                    this.SaveShowInfo();
                    retval = newShowInfo;
                }
            }
                
            return retval;
        }

        /// <summary>
        /// Saves the show information to the meta information file
        /// </summary>
        public void SaveShowInfo()
        {
            File.WriteAllText(this.MetaInformationFile, this.Shows.ToJson());
        }
    }
}
