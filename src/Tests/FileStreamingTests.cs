using System;
using NUnit.Framework;
using Should;
using System.IO;
using TdmsFile = NationalInstruments.Tdms.File;

namespace Tests
{
    [TestFixture]
    public class FileStreamingTests
    {
        private TdmsFile _file;

        [SetUp]
        public void Setup()
        {
            _file = new TdmsFile(new MemoryStream(
                File.ReadAllBytes(Constants.SampleFile))).Open();
        }

        [TearDown]
        public void TearDown()
        {
            _file.Dispose();
        }

        [Test]
        public void Should_Contain_File_Information()
        {
            Console.WriteLine(new DateTime(2009, 4, 24, 8, 29, 45, DateTimeKind.Local));
            Console.WriteLine(new DateTime(2009, 4, 24, 8, 29, 45, DateTimeKind.Utc));
            _file.Properties.Count.ShouldEqual(10);
            _file.Properties["title"].ShouldEqual("DIAdem example data set");
            _file.Properties["author"].ShouldEqual("National Instruments");
            _file.Properties["registertxt3"].ShouldEqual(string.Empty);
            _file.Properties["datetime"].ShouldEqual(new DateTime(2009, 4, 24, 8, 29, 45, DateTimeKind.Utc).ToLocalTime());
            _file.Properties["registertxt1"].ShouldEqual(string.Empty);
            _file.Properties["registertxt2"].ShouldEqual(string.Empty);
            _file.Properties["name"].ShouldEqual("EXAMPLE");
            _file.Properties["description"].ShouldEqual("Data set with numeric x/y-channels, waveform channels, and text channels in several channel groups");
            _file.Properties["datestring"].ShouldEqual("24.04.2009");
            _file.Properties["timestring"].ShouldEqual("10:29:45");
        }
    }
}