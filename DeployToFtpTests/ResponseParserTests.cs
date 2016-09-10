using System;
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

        private DateTime _nixFileDate = new DateTime(2016, 9, 7, 19, 29, 0);
        private DateTime _winFileDate = new DateTime(2016, 8, 14, 21, 59, 0);

        [TestMethod]
        public void ResponseParserGetsRightFileStyle()
        {
            Assert.AreEqual(FileListStyle.UnixStyle, ResponseParser.SwitchFileSystem(_nixResponse.Split('\n')));
            Assert.AreEqual(FileListStyle.WindowsStyle, ResponseParser.SwitchFileSystem(_winResponcse.Split('\n')));
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
            Assert.AreEqual("160907-19:29", ResponseParser.GetCreateTime(_nixFile));
            Assert.AreEqual("131125-00:00", ResponseParser.GetCreateTime(oldFile));
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
            Assert.AreEqual("160907-19:29", file.CreateTime);
            Assert.AreEqual("rw-r--r--", file.Flags);
        }

        //[TestMethod]
        //public void WinFileIsParsedCorrectly()
        //{
        //    FileStruct file = ResponseParser.ParseFile(_winFile);

        //    Assert.AreEqual("License.rtf", file.Name);
        //    Assert.AreEqual(false, file.IsDirectory);
        //    Assert.AreEqual("160814-21:59", file.CreateTime);
        //    Assert.AreEqual("notdeterm", file.Flags);
        //}
    }
}