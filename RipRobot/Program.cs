using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ATVAssistant.Common;

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
            
            //  Get the base path for source files:
            string basePath = ConfigurationManager.AppSettings["BasePath"];
            string currentPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            //  Get the Handbrake settings:
            string handbrakeSwitches = ConfigurationManager.AppSettings["HandbrakeSwitches"];
            int handbrakeTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["HandbrakeTimeout"]);

            //  Get the AtomicParsley settings:
            int atomicParsleyTimeout = Convert.ToInt32(ConfigurationManager.AppSettings["AtomicParsleyTimeout"]);

            //  Get after processing path:
            string afterProcessingPath = ConfigurationManager.AppSettings["AfterProcessingPath"];

            //  Get artwork Path
            string artworkBasePath = ConfigurationManager.AppSettings["ArtworkBasePath"]; 

            #endregion

            #region Get DVD information

            //  Get DVD information
            List<iTunesMedia> mediaInfo = new List<iTunesMedia>();
            DVDInfo di = DVDInfo.ForDVD();

            //  If we have basic information, get media information:
            if(di != null)
                mediaInfo = iTunesMedia.ForMovie(di.Title);

            #endregion

            #region Encode with Handbrake or rip to disk

            //  If we have media information, proceed with encode
            if(mediaInfo.Any())
            {

            }
            else
            { 
                //  Otherwise, just rip to disk

            }

            #endregion

            #region If we encoded, add meta data and add to iTunes

            //  If we have media information and our encoded file exists...
            if(mediaInfo.Any() && File.Exists(""))
            { 
                //  Add meta information using Atomicparsley

                //  Move file to post processing area (most likely adding to iTunes)

            }

            #endregion

            #region Send push notification

            //  If we have media information, we just encoded the file
            if(mediaInfo.Any())
            {
                //  Send a push notification about the specific movie just added

            }
            else
            { 
                //  Send a push notification about the rip 

            }

            #endregion
        }
    }
}
