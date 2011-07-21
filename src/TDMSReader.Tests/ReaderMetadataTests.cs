using System.Collections.Generic;
using NUnit.Framework;
using Should;

namespace TDMSReader.Tests
{
    [TestFixture]
    public class ReaderMetadataTests : ReaderTestsBase
    {
        private IList<Reader.Metadata> _metadatas;

        [SetUp]
        public void Setup()
        {
            base.Setup();
            _metadatas = Reader.ReadMetadata(Reader.ReadFirstSegment().MetadataOffset);
        }

        [Test]
        public void Should_Contain_Correct_Number_Of_Objects()
        {
            _metadatas.Count.ShouldEqual(8);
        }

        [Test]
        public void Should_Have_Correct_Object_Names()
        {
            _metadatas[0].Path.ShouldEqual("/'EXAMPLE'/'Time'");
            _metadatas[1].Path.ShouldEqual("/'EXAMPLE'/'Speed'");
            _metadatas[2].Path.ShouldEqual("/'EXAMPLE'/'Revs'");
            _metadatas[3].Path.ShouldEqual("/'EXAMPLE'/'Torque'");
            _metadatas[4].Path.ShouldEqual("/'EXAMPLE'");
            _metadatas[5].Path.ShouldEqual("/'Noise data'/'Noise_1'");
            _metadatas[6].Path.ShouldEqual("/'Noise data'");
            _metadatas[7].Path.ShouldEqual("/");
        }

        [Test]
        public void Should_Have_Correct_Number_Of_Properties()
        {
            _metadatas[0].Properties.Count.ShouldEqual(21);
            _metadatas[1].Properties.Count.ShouldEqual(17);
            _metadatas[2].Properties.Count.ShouldEqual(17);
            _metadatas[3].Properties.Count.ShouldEqual(17);
            _metadatas[4].Properties.Count.ShouldEqual(4);
            _metadatas[5].Properties.Count.ShouldEqual(0);
            _metadatas[6].Properties.Count.ShouldEqual(0);
            _metadatas[7].Properties.Count.ShouldEqual(0);
        }

        [Test]
        public void Should_Have_Correct_Properties()
        {
            _metadatas[0].Properties["description"].ShouldEqual("Driving time");
            _metadatas[0].Properties["minimum"].ShouldEqual((double)0);
            _metadatas[0].Properties["maximum"].ShouldEqual(59.8400999919977);
            _metadatas[0].Properties["registerint5"].ShouldEqual(0);
            _metadatas[0].Properties["registerint6"].ShouldEqual(0);
            _metadatas[0].Properties["unit_string"].ShouldEqual("s");
            _metadatas[0].Properties["datatype"].ShouldEqual("DT_DOUBLE");
            _metadatas[0].Properties["registertxt3"].ShouldEqual("");
            _metadatas[0].Properties["registerint3"].ShouldEqual(0);
            _metadatas[0].Properties["registerint4"].ShouldEqual(0);
            _metadatas[0].Properties["registerint1"].ShouldEqual(0);
            _metadatas[0].Properties["registerint2"].ShouldEqual(0);
            _metadatas[0].Properties["registertxt1"].ShouldEqual("");
            _metadatas[0].Properties["registertxt2"].ShouldEqual("");
            _metadatas[0].Properties["displaytype"].ShouldEqual("Numeric");
            _metadatas[0].Properties["monotony"].ShouldEqual("increasing");
            _metadatas[0].Properties["novaluekey"].ShouldEqual("No");
            _metadatas[0].Properties["ResultStatArithMean"].ShouldEqual(29.9200499959988);
            _metadatas[0].Properties["ResultStatMax"].ShouldEqual(59.8400999919977);
            _metadatas[0].Properties["ResultStatMin"].ShouldEqual((double)0);
            _metadatas[0].Properties["ResultStatSum"].ShouldEqual(30638.1311959028);
        }

        [Test]
        public void Should_Have_Raw_Data_Markers()
        {
            _metadatas[0].RawData.Length.ShouldEqual(20);
            _metadatas[0].RawData.Count.ShouldEqual(1024);
            _metadatas[0].RawData.DataType.ShouldEqual(10);
            _metadatas[0].RawData.Dimension.ShouldEqual(1);

            _metadatas[1].RawData.Length.ShouldEqual(20);
            _metadatas[1].RawData.Count.ShouldEqual(1024);
            _metadatas[1].RawData.DataType.ShouldEqual(10);
            _metadatas[1].RawData.Dimension.ShouldEqual(1);

            _metadatas[2].RawData.Length.ShouldEqual(20);
            _metadatas[2].RawData.Count.ShouldEqual(1024);
            _metadatas[2].RawData.DataType.ShouldEqual(10);
            _metadatas[2].RawData.Dimension.ShouldEqual(1);

            _metadatas[3].RawData.Length.ShouldEqual(20);
            _metadatas[3].RawData.Count.ShouldEqual(1024);
            _metadatas[3].RawData.DataType.ShouldEqual(10);
            _metadatas[3].RawData.Dimension.ShouldEqual(1);

            _metadatas[4].RawData.Length.ShouldEqual(-1);
            _metadatas[4].RawData.Count.ShouldEqual(0);
            _metadatas[4].RawData.DataType.ShouldEqual(0);
            _metadatas[4].RawData.Dimension.ShouldEqual(0);

            _metadatas[5].RawData.Length.ShouldEqual(20);
            _metadatas[5].RawData.Count.ShouldEqual(262144);
            _metadatas[5].RawData.DataType.ShouldEqual(10);
            _metadatas[5].RawData.Dimension.ShouldEqual(1);

            _metadatas[6].RawData.Length.ShouldEqual(-1);
            _metadatas[6].RawData.Count.ShouldEqual(0);
            _metadatas[6].RawData.DataType.ShouldEqual(0);
            _metadatas[6].RawData.Dimension.ShouldEqual(0);

            _metadatas[7].RawData.Length.ShouldEqual(-1);
            _metadatas[7].RawData.Count.ShouldEqual(0);
            _metadatas[7].RawData.DataType.ShouldEqual(0);
            _metadatas[7].RawData.Dimension.ShouldEqual(0);
        }
    }
}