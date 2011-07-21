using System.Linq;
using NUnit.Framework;
using Should;

namespace TDMSReader.Tests
{
    [TestFixture]
    public class ReaderRawDataTests : ReaderTestsBase
    {
        [Test]
        public void Should_Read_Raw_Data()
        {
            var segment = Reader.ReadFirstSegment();
            var metadata = Reader.ReadMetadata(segment.MetadataOffset).First();
            var rawData = Reader.ReadRawData(segment.RawDataOffset, metadata.RawData.Count, metadata.RawData.DataType).Cast<double>().ToList();
            rawData.Count.ShouldEqual(1024);
            rawData.Where(x => x == 0.0).Count().ShouldEqual(1);
            rawData.Where(x => x != 0.0).Count().ShouldEqual(1023);
        }
    }
}