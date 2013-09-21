using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATVAssistant.Common
{
    public class MovieMetaInfo
    {
        /// <summary>
        /// The movie name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The local artwork location
        /// </summary>
        public string ArtworkLocation { get; set; }

        /// <summary>
        /// The MPAA rating for the movie
        /// </summary>
        public string Rating { get; set; }

        /// <summary>
        /// The year the movie was made
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// The suggested genre for the movie
        /// </summary>
        public string Genre { get; set; }
    }
}
