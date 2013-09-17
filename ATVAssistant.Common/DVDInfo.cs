using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DirectShowLib;
using DirectShowLib.Dvd;

namespace ATVAssistant.Common
{
    /// <summary>
    /// Gets information for a DVD currently inserted in the drive
    /// </summary>
    public class DVDInfo
    {
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
    }
}
