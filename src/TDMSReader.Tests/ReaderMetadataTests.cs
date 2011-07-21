using System.Collections.Generic;
using NUnit.Framework;
using Should;

namespace TDMSReader.Tests
{
    [TestFixture]
    public class ReaderMetadataTests : ReaderTestsBase
    {
        private IList<Reader.Metadata> _segmentOneMetadata;
        private IList<Reader.Metadata> _segmentFourMetadata;

        [SetUp]
        public void Setup()
        {
            base.Setup();
            var firstSegment = Reader.ReadFirstSegment();
            _segmentOneMetadata = Reader.ReadMetadata(firstSegment.MetadataOffset);
            _segmentFourMetadata = Reader.ReadMetadata(Reader.ReadSegment(
                                     Reader.ReadSegment(
                                     Reader.ReadSegment(
                                     firstSegment.NextSegmentOffset).NextSegmentOffset).NextSegmentOffset).MetadataOffset);
        }

        [Test]
        public void Should_Contain_Correct_Number_Of_Objects()
        {
            _segmentOneMetadata.Count.ShouldEqual(8);
        }

        [Test]
        public void Should_Have_Correct_Object_Names()
        {
            _segmentOneMetadata[0].Path.ShouldEqual("/'EXAMPLE'/'Time'");
            _segmentOneMetadata[1].Path.ShouldEqual("/'EXAMPLE'/'Speed'");
            _segmentOneMetadata[2].Path.ShouldEqual("/'EXAMPLE'/'Revs'");
            _segmentOneMetadata[3].Path.ShouldEqual("/'EXAMPLE'/'Torque'");
            _segmentOneMetadata[4].Path.ShouldEqual("/'EXAMPLE'");
            _segmentOneMetadata[5].Path.ShouldEqual("/'Noise data'/'Noise_1'");
            _segmentOneMetadata[6].Path.ShouldEqual("/'Noise data'");
            _segmentOneMetadata[7].Path.ShouldEqual("/");
        }

        [Test]
        public void Should_Have_Correct_Number_Of_Properties()
        {
            _segmentOneMetadata[0].Properties.Count.ShouldEqual(21);
            _segmentOneMetadata[1].Properties.Count.ShouldEqual(17);
            _segmentOneMetadata[2].Properties.Count.ShouldEqual(17);
            _segmentOneMetadata[3].Properties.Count.ShouldEqual(17);
            _segmentOneMetadata[4].Properties.Count.ShouldEqual(4);
            _segmentOneMetadata[5].Properties.Count.ShouldEqual(0);
            _segmentOneMetadata[6].Properties.Count.ShouldEqual(0);
            _segmentOneMetadata[7].Properties.Count.ShouldEqual(0);
        }

        [Test]
        public void Should_Have_Correct_Properties()
        {
            _segmentOneMetadata[0].Properties["description"].ShouldEqual("Driving time");
            _segmentOneMetadata[0].Properties["minimum"].ShouldEqual((double)0);
            _segmentOneMetadata[0].Properties["maximum"].ShouldEqual(59.8400999919977);
            _segmentOneMetadata[0].Properties["registerint5"].ShouldEqual(0);
            _segmentOneMetadata[0].Properties["registerint6"].ShouldEqual(0);
            _segmentOneMetadata[0].Properties["unit_string"].ShouldEqual("s");
            _segmentOneMetadata[0].Properties["datatype"].ShouldEqual("DT_DOUBLE");
            _segmentOneMetadata[0].Properties["registertxt3"].ShouldEqual("");
            _segmentOneMetadata[0].Properties["registerint3"].ShouldEqual(0);
            _segmentOneMetadata[0].Properties["registerint4"].ShouldEqual(0);
            _segmentOneMetadata[0].Properties["registerint1"].ShouldEqual(0);
            _segmentOneMetadata[0].Properties["registerint2"].ShouldEqual(0);
            _segmentOneMetadata[0].Properties["registertxt1"].ShouldEqual("");
            _segmentOneMetadata[0].Properties["registertxt2"].ShouldEqual("");
            _segmentOneMetadata[0].Properties["displaytype"].ShouldEqual("Numeric");
            _segmentOneMetadata[0].Properties["monotony"].ShouldEqual("increasing");
            _segmentOneMetadata[0].Properties["novaluekey"].ShouldEqual("No");
            _segmentOneMetadata[0].Properties["ResultStatArithMean"].ShouldEqual(29.9200499959988);
            _segmentOneMetadata[0].Properties["ResultStatMax"].ShouldEqual(59.8400999919977);
            _segmentOneMetadata[0].Properties["ResultStatMin"].ShouldEqual((double)0);
            _segmentOneMetadata[0].Properties["ResultStatSum"].ShouldEqual(30638.1311959028);
        }

        [Test]
        public void Should_Have_Raw_Data_Markers()
        {
            _segmentOneMetadata[0].RawData.Size.ShouldEqual(8192);
            _segmentOneMetadata[0].RawData.Offset.ShouldEqual(0);
            _segmentOneMetadata[0].RawData.Count.ShouldEqual(1024);
            _segmentOneMetadata[0].RawData.DataType.ShouldEqual(10);
            _segmentOneMetadata[0].RawData.Dimension.ShouldEqual(1);

            _segmentOneMetadata[1].RawData.Size.ShouldEqual(8192);
            _segmentOneMetadata[1].RawData.Offset.ShouldEqual(8192);
            _segmentOneMetadata[1].RawData.Count.ShouldEqual(1024);
            _segmentOneMetadata[1].RawData.DataType.ShouldEqual(10);
            _segmentOneMetadata[1].RawData.Dimension.ShouldEqual(1);

            _segmentOneMetadata[2].RawData.Size.ShouldEqual(8192);
            _segmentOneMetadata[2].RawData.Offset.ShouldEqual(16384);
            _segmentOneMetadata[2].RawData.Count.ShouldEqual(1024);
            _segmentOneMetadata[2].RawData.DataType.ShouldEqual(10);
            _segmentOneMetadata[2].RawData.Dimension.ShouldEqual(1);

            _segmentOneMetadata[3].RawData.Size.ShouldEqual(8192);
            _segmentOneMetadata[3].RawData.Offset.ShouldEqual(24576);
            _segmentOneMetadata[3].RawData.Count.ShouldEqual(1024);
            _segmentOneMetadata[3].RawData.DataType.ShouldEqual(10);
            _segmentOneMetadata[3].RawData.Dimension.ShouldEqual(1);

            _segmentOneMetadata[4].RawData.Size.ShouldEqual(0);
            _segmentOneMetadata[4].RawData.Offset.ShouldEqual(0);
            _segmentOneMetadata[4].RawData.Count.ShouldEqual(0);
            _segmentOneMetadata[4].RawData.DataType.ShouldEqual(0);
            _segmentOneMetadata[4].RawData.Dimension.ShouldEqual(0);

            _segmentOneMetadata[5].RawData.Size.ShouldEqual(2097152);
            _segmentOneMetadata[5].RawData.Offset.ShouldEqual(32768);
            _segmentOneMetadata[5].RawData.Count.ShouldEqual(262144);
            _segmentOneMetadata[5].RawData.DataType.ShouldEqual(10);
            _segmentOneMetadata[5].RawData.Dimension.ShouldEqual(1);

            _segmentOneMetadata[6].RawData.Size.ShouldEqual(0);
            _segmentOneMetadata[6].RawData.Offset.ShouldEqual(0);
            _segmentOneMetadata[6].RawData.Count.ShouldEqual(0);
            _segmentOneMetadata[6].RawData.DataType.ShouldEqual(0);
            _segmentOneMetadata[6].RawData.Dimension.ShouldEqual(0);

            _segmentOneMetadata[7].RawData.Size.ShouldEqual(0);
            _segmentOneMetadata[7].RawData.Offset.ShouldEqual(0);
            _segmentOneMetadata[7].RawData.Count.ShouldEqual(0);
            _segmentOneMetadata[7].RawData.DataType.ShouldEqual(0);
            _segmentOneMetadata[7].RawData.Dimension.ShouldEqual(0);

            _segmentFourMetadata[2].RawData.Size.ShouldEqual(90);
            _segmentFourMetadata[2].RawData.Offset.ShouldEqual(502848);
            _segmentFourMetadata[2].RawData.Count.ShouldEqual(5);
            _segmentFourMetadata[2].RawData.DataType.ShouldEqual(32);
            _segmentFourMetadata[2].RawData.Dimension.ShouldEqual(1);
        }
    }
}