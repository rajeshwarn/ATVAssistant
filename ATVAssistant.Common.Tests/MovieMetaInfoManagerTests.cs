using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ATVAssistant.Common;

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
    }
}
