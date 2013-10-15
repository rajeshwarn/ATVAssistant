using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ShowInfo;
using ShowInfoProvider;

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
            //  string showName = "Undercover Boss (US)";
            //  string showName = "Homeland";
            //  string showName = "The Colbert Report";
            string showName = "Castle (2009)";
            int season = 3;

            //  Act
            TVShowMetaInfoManager mgr = new TVShowMetaInfoManager(showInfoFile, artworkBasePath);
            TVShowMetaInfo showInfo = mgr.FindShowInfo(showName, season);

            //  Assert
            Assert.IsNotNull(showInfo);
            Assert.IsTrue(!string.IsNullOrEmpty(showInfo.ArtworkLocation));
        }

        [TestMethod]
        public void ShowInformation_FromFilename_Successful()
        { 
            //  Arrange
            string showfilename = "Person.of.Interest.S03E03.HDTV.x264-LOL.mp4";
            string showInfoFile = @"c:\temp\showinfo.json";
            string artworkBasePath = @"c:\temp\artwork";

            //  Act
            ShowInformationManager mgr = new ShowInformationManager();
            TVEpisodeInfo episodeInfo = mgr.GetEpisodeInfoForFilename(showfilename);
            TVShowMetaInfoManager metaMgr = new TVShowMetaInfoManager(showInfoFile, artworkBasePath);
            TVShowMetaInfo metaInfo = metaMgr.FindShowInfo(episodeInfo.ShowName, episodeInfo.SeasonNumber);

            //  Assert
            Assert.IsNotNull(metaInfo);
            Assert.IsTrue(!string.IsNullOrEmpty(metaInfo.ArtworkLocation));
        }
    }
}
