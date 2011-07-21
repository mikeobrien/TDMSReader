using System.Linq;
using NUnit.Framework;
using Should;

namespace TDMSReader.Tests
{
    [TestFixture]
    public class ReaderRawDataTests : ReaderTestsBase
    {
        [Test]
        public void Should_Read_Raw_Fixed_Width_Data()
        {
            var segment = Reader.ReadFirstSegment();
            var metadata = Reader.ReadMetadata(segment.MetadataOffset).First();
            var rawData = Reader.ReadRawData(segment.RawDataOffset + metadata.RawData.Offset, metadata.RawData.Count, metadata.RawData.DataType).Cast<double>().ToList();
            segment.RawDataOffset.ShouldEqual(2303);
            metadata.RawData.Offset.ShouldEqual(0);
            rawData.Count.ShouldEqual(1024);
            rawData.Where(x => x == 0.0).Count().ShouldEqual(1);
            rawData.Where(x => x != 0.0).Count().ShouldEqual(1023);
        }

        [Test]
        public void Should_Read_Raw_String_Data()
        {
            var segment = Reader.ReadSegment(
                                     Reader.ReadSegment(
                                     Reader.ReadSegment(
                                     Reader.ReadFirstSegment().NextSegmentOffset).NextSegmentOffset).NextSegmentOffset);
            var metadata = Reader.ReadMetadata(segment.MetadataOffset).Skip(2).First();
            var rawData = Reader.ReadRawData(segment.RawDataOffset + metadata.RawData.Offset, metadata.RawData.Count, metadata.RawData.DataType).Cast<string>().ToList();
            segment.RawDataOffset.ShouldEqual(7340099);
            metadata.RawData.Offset.ShouldEqual(502848);
            rawData.Count.ShouldEqual(5);
            rawData[0].ShouldEqual("Sum of measurement values");
            rawData[1].ShouldEqual("Minimum");
            rawData[2].ShouldEqual("Maximum");
            rawData[3].ShouldEqual("Arithmetic mean");
            rawData[4].ShouldEqual("Root mean square");
        }
    }
}