using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using DirectShowLib;
using DirectShowLib.Dvd;
using ServiceStack.Text;

namespace ATVAssistant.Common
{
    /// <summary>
    /// Gets information for a DVD currently inserted in the drive
    /// </summary>
    [DataContract]
    public class DVDInfo
    {
        /// <summary>
        /// Base API url
        /// </summary>
        private static string _baseSearchUrl = "http://www.api.dvdid.info/0dbbac014a8fb4bebab783cc55135c63e1d1f300/getInfo/{0}/json";

        /// <summary>
        /// The movie database id
        /// </summary>
        [DataMember(Name = "TMDbId")]
        public string TMDBId { get; set; }

        /// <summary>
        /// IMDB id
        /// </summary>
        [DataMember(Name = "IMDbId")]
        public string IMDBId { get; set; }

        /// <summary>
        /// DVD title
        /// </summary>
        [DataMember(Name = "dvdTitle")]
        public string Title { get; set; }

        /// <summary>
        /// Fetches the DVDId for a given DVD volume
        /// </summary>
        /// <param name="dvdVolume"></param>
        /// <returns></returns>
        public string GetDVDId(string dvdVolume = null)
        {
            string retval = string.Empty;

            try
            {
                IDvdGraphBuilder dvdGraphBuilder = (IDvdGraphBuilder)new DvdGraphBuilder();
                int hResult;

                // Build the DVD Graph
                AMDvdRenderStatus amDvdRenderStatus;
                hResult = dvdGraphBuilder.RenderDvdVideoVolume(dvdVolume,
                AMDvdGraphFlags.None, out amDvdRenderStatus);
                DsError.ThrowExceptionForHR(hResult);

                // Get the IDvDInfo2 interface
                object comObject;
                hResult = dvdGraphBuilder.GetDvdInterface(typeof(IDvdInfo2).GUID, out comObject);
                DsError.ThrowExceptionForHR(hResult);
                IDvdInfo2 dvdInfo2 = (IDvdInfo2)comObject;
                comObject = null;

                // Get the DVD ID.
                long discId;
                dvdInfo2.GetDiscID(dvdVolume, out discId);

                //  Print out the disc id if it's not 0:
                if(discId != 0)
                    retval = string.Format("{0,8:x}|{1,8:x}", (int)(discId >> 32), (int)(discId & 0xFFFFFFFF));
            }
            catch(Exception)
            {
                //  Silently eat this for now
            }

            return retval;
        }

        /// <summary>
        /// Gets the DVD information for a given DVD volume
        /// </summary>
        /// <param name="dvdVolume"></param>
        /// <returns></returns>
        public static DVDInfo ForDVD(string dvdVolume = null)
        {
            DVDInfo retval = null;
            string dvdId = new DVDInfo().GetDVDId(dvdVolume);

            //  Format the url
            string fullUrl = string.Format(_baseSearchUrl, dvdId);

            try
            {
                //  Call the service and get the results:
                DVDInfo serviceResult = fullUrl.GetJsonFromUrl().Trim().FromJson<DVDInfo>();

                //  Set the results:
                retval = serviceResult;
            }
            catch(Exception)
            { /* Fail quietly */ }

            return retval;
        }
    }
}
