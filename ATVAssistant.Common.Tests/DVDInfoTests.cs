using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ATVAssistant.Common.Tests
{
    [TestClass]
    public class DVDInfoTests
    {
        /// <summary>
        /// Requires a DVD movie in the drive to pass
        /// </summary>
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
    }
}
