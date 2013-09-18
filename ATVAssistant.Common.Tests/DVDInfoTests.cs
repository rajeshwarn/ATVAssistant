using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATVAssistant.Common.Tests
{
    /// <summary>
    /// These require a DVD movie in the drive to pass
    /// </summary>
    [TestClass]
    public class DVDInfoTests
    {
        [TestMethod]
        public void GetDVDId_Successful()
        {
            //  Arrange
            DVDInfo di = new DVDInfo();
            string retval = string.Empty;

            //  Act
            retval = di.GetDVDId();

            //  Assert
            Assert.AreNotEqual<string>(string.Empty, retval);
        }

        [TestMethod]
        public void GetDVDInfo_Successful()
        {
            //  Arrange
            DVDInfo di = null;

            //  Act
            di = DVDInfo.ForDVD();

            //  Assert
            Assert.IsNotNull(di);
        }

        [TestMethod]
        public void GetITunesInfoForDVD_Successful()
        {
            //  Arrange
            DVDInfo di = null;

            //  Act
            di = DVDInfo.ForDVD();
            List<iTunesMedia> mediaInfo = iTunesMedia.ForMovie(di.Title);

            //  Assert
            Assert.IsNotNull(mediaInfo);
            Assert.IsTrue(mediaInfo.Count > 0);
        }
    }
}
