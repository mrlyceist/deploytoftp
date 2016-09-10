using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DeployToFtp;

namespace DeployToFtpTests
{
    [TestClass]
    public class DirectoryListParserTests
    {
        private string _responce =
            "drwxr-xr-x    2 0        0            4096 Sep 07 19:33 software\n-rw-r--r--    1 106      114         17765 Sep 07 19:29 temp.xlsx\n";

        private string _nixFile = "-rw-r--r--    1 106      114         17765 Sep 07 19:29 temp.xlsx";
        private string _nixDirectory = "drwxr-xr-x    2 0        0            4096 Sep 07 19:33 software";

        [TestMethod]
        public void DifrctoryListParserReturnsTwoUnixFiles()
        {
            DirectoryListParser parser = new DirectoryListParser(_responce);
            Assert.AreEqual(2, parser.FullListing.Length);
        }

        [TestMethod]
        public void ParserDefinesFileStyleCorrectly()
        {
            DirectoryListParser parser = new DirectoryListParser(_responce);
            Assert.AreEqual(FileListStyle.UnixStyle, parser.GuessFileListStyle(_responce.Split('\n')));
        }
        
    }
}
