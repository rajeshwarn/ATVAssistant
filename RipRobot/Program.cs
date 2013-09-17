using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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

        }
    }
}
