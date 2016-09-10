using System;
using System.Linq;
using DeployToFtp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeployToFtpTests
{
    [TestClass]
    public class ResponseParserTests
    {
        private string _nixResponse =
            "drwxr-xr-x    2 0        0            4096 Sep 07 19:33 software\n-rw-r--r--    1 106      114         17765 Sep 07 19:29 temp.xlsx\n";

        private string _winResponcse =
            "08-14-16  09:59PM                43378 License.rtf\n08-16-16  10:29PM       <DIR>          RegexTester\n";

        private string _nixFile = "-rw-r--r--    1 106      114         17765 Sep 07 19:29 temp.xlsx";
        private string _nixDir  = "drwxr-xr-x    2 0        0            4096 Sep 07 19:33 software";
        private string _winFile = "08-14-16  09:59PM                43378 License.rtf";
        private string _winDir  = "08-16-16  10:29PM       <DIR>          RegexTester";

        private readonly DateTime _nixFileDate = new DateTime(2016, 9, 7, 19, 29, 0);
        private readonly DateTime _winFileDate = new DateTime(2016, 8, 14, 21, 59, 0);

        [TestMethod]
        public void ResponseParserGetsRightFileStyle()
        {
            Assert.AreEqual(FileSystem.UnixStyle, ResponseParser.SwitchFileSystem(_nixFile));
            Assert.AreEqual(FileSystem.WindowsStyle, ResponseParser.SwitchFileSystem(_winFile));
        }

        [TestMethod]
        public void GetFlagsGetsFlagsFromNixResponse()
        {
            Assert.AreEqual("rw-r--r--", ResponseParser.GetFlags(_nixFile));
        }

        [TestMethod]
        public void GetCreateTimeGetsTimeStringFromNixResponse()
        {
            var oldFile = "-rw-r--r--    1 106      114         17765 Nov 25 2013 temp.xlsx";
            Assert.AreEqual(_nixFileDate, ResponseParser.GetCreateTime(_nixFile));
            Assert.AreEqual(new DateTime(2013, 11, 25, 0, 0, 0), ResponseParser.GetCreateTime(oldFile));
        }

        [TestMethod]
        public void GetCreateTimeGetsRightTimeFromWinResponce()
        {
            Assert.AreEqual(_winFileDate, ResponseParser.GetCreateTime(_winFile));
        }

        [TestMethod]
        public void GetNameGetsRightName()
        {
            Assert.AreEqual("temp.xlsx", ResponseParser.GetName(_nixFile));
            Assert.AreEqual("License.rtf", ResponseParser.GetName(_winFile));
        }

        [TestMethod]
        public void NixFileIsParsedCorrectly()
        {
            FileStruct file = ResponseParser.ParseFile(_nixFile);

            Assert.AreEqual("temp.xlsx", file.Name);
            Assert.AreEqual(false, file.IsDirectory);
            Assert.AreEqual(_nixFileDate, file.CreateTime);
            Assert.AreEqual("rw-r--r--", file.Flags);
        }

        [TestMethod]
        public void WinFileIsParsedCorrectly()
        {
            FileStruct file = ResponseParser.ParseFile(_winFile);

            Assert.AreEqual("License.rtf", file.Name);
            Assert.AreEqual(false, file.IsDirectory);
            Assert.AreEqual(_winFileDate, file.CreateTime);
            Assert.AreEqual("notdeterm", file.Flags);
        }

        [TestMethod]
        public void IsItDirectoryFindsOutIfFileIsDirectory()
        {
            Assert.IsFalse(ResponseParser.IsItDirectory(_nixFile));
            Assert.IsFalse(ResponseParser.IsItDirectory(_winFile));
            Assert.IsTrue(ResponseParser.IsItDirectory(_nixDir));
            Assert.IsTrue(ResponseParser.IsItDirectory(_winDir));
        }

        [TestMethod]
        public void ResponseParserParsesResponse()
        {
            var nixParser = new ResponseParser(_nixResponse);
            var winParser = new ResponseParser(_winResponcse);
            Assert.AreEqual(2, nixParser.Files.Count());
            Assert.AreEqual(2, winParser.Files.Count());
            Assert.AreEqual("temp.xlsx", nixParser.Files.Where(file => !file.IsDirectory).ToArray()[0].Name);
            Assert.AreEqual("RegexTester", winParser.Files.Where(file => file.IsDirectory).ToArray()[0].Name);
        }
    }
}