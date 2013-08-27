using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATVAssistant.Common.Tests
{
    [TestClass]
    public class iTunesMediaTests
    {
        [TestMethod]
        public void GetArtworkForShowWithSeason_Successful()
        {
            //  Arrange
            string show = "Breaking Bad";
            int season = 5;

            //  Act
            List<iTunesMedia> mediaInfo = iTunesMedia.ForTVShow(show, season);

            //  Assert
            Assert.IsNotNull(mediaInfo);
            Assert.IsTrue(mediaInfo.Count > 0);
        }

        [TestMethod]
        public void GetArtworkForShow_Successful()
        {
            //  Arrange
            string show = "Breaking Bad";

            //  Act
            List<iTunesMedia> mediaInfo = iTunesMedia.ForTVShow(show);

            //  Assert
            Assert.IsNotNull(mediaInfo);
            Assert.IsTrue(mediaInfo.Count > 0);
        }

        [TestMethod]
        public void GetArtworkForMovie_Successful()
        {
            //  Arrange
            string movie = "Jurassic Park";

            //  Act
            List<iTunesMedia> mediaInfo = iTunesMedia.ForMovie(movie);

            //  Assert
            Assert.IsNotNull(mediaInfo);
            Assert.IsTrue(mediaInfo.Count > 0);
        }
    }
}
