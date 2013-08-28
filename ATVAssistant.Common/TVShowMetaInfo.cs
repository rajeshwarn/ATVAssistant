using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace ATVAssistant.Common
{
    [DataContract]
    public class TVShowMetaInfo
    {
        [DataMember(Name="showname")]
        public string Name
        {
            get;
            set;
        }

        [DataMember(Name = "season")]
        public int Season
        {
            get;
            set;
        }

        [DataMember(Name = "artworklocation")]
        public string ArtworkLocation
        {
            get;
            set;
        }

        [DataMember(Name = "rating")]
        public string Rating
        {
            get;
            set;
        }
        
    }
}
