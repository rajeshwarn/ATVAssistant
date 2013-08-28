using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATVAssistant.Common.Tests
{
    [TestClass]
    public class TVShowMetaInfoManagerTests
    {
        [TestMethod]
        public void ShowInformation_ForShowNotCached_Successful()
        {
            //  Arrange
            string showInfoFile = @"c:\temp\showinfo.json";
            string artworkBasePath = @"c:\temp\artwork";
            string showName = "Alias";
            int season = 3;

            //  Act
            TVShowMetaInfoManager mgr = new TVShowMetaInfoManager(showInfoFile, artworkBasePath);
            TVShowMetaInfo showInfo = mgr.FindShowInfo(showName, season);

            //  Assert
            Assert.IsNotNull(showInfo);
            Assert.IsTrue(!string.IsNullOrEmpty(showInfo.ArtworkLocation));
        }
    }
}
