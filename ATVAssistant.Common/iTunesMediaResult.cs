using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ATVAssistant.Common
{
    /// <summary>
    /// Represents result from iTunes search API
    /// </summary>
    [DataContract]
    public class iTunesMediaResult
    {
        /// <summary>
        /// The number of results returned
        /// </summary>
        [DataMember(Name="resultCount")]
        public int ResultCount
        {
            get;
            set;
        }

        /// <summary>
        /// The actual results
        /// </summary>
        [DataMember(Name = "results")]
        public List<iTunesMedia> Results
        {
            get;
            set;
        }
    }
}
