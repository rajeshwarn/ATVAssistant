using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ATVAssistant.Common;
using System.Text.RegularExpressions;

namespace ATVAssistant.Common.Tests
{
    [TestClass]
    public class MovieMetaInfoManagerTests
    {
        [TestMethod]
        public void MovieInformation_ForMovie_Successful()
        {
            //  Arrange
            string artworkBasePath = @"c:\temp\artwork";
            string movieName = "Star Trek";

            //  Act
            MovieMetaInfoManager mgr = new MovieMetaInfoManager(artworkBasePath);
            MovieMetaInfo movieInfo = mgr.FindMovieInfo(movieName);

            //  Assert
            Assert.IsNotNull(movieInfo);
            Assert.IsTrue(!string.IsNullOrEmpty(movieInfo.ArtworkLocation));
        }

        [TestMethod]
        public void MediaInfo_ForMovie_CleanUpName()
        {
            //  Arrange
            string artworkBasePath = @"c:\temp\artwork";
            string movieName = "Percy Jackson Sea of Monsters";

            //  Act
            MovieMetaInfoManager mgr = new MovieMetaInfoManager(artworkBasePath);
            MovieMetaInfo movieInfo = mgr.FindMovieInfo(movieName);

            //  Clean up the movie name:
            string newName = Regex.Replace(movieInfo.Name, @"[^a-zA-Z\d\s]", "");

            //  Assert
            Assert.IsFalse(string.IsNullOrEmpty(newName));
        }
    }
}
