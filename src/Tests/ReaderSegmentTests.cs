using NUnit.Framework;
using Should;
using TDMSReader;

namespace Tests
{
    [TestFixture]
    public class ReaderSegmentTests : ReaderTestsBase
    {
        private const string Identifier = "TDSm";
        private const int Version = 4712;

        [Test]
        public void Should_Get_First_Segment()
        {
            var leadin = Reader.ReadFirstSegment();
            leadin.ShouldNotBeNull();
            leadin.Identifier.ShouldEqual(Identifier);
            leadin.TableOfContents.ContainsNewObjects.ShouldBeTrue();
            leadin.TableOfContents.HasDaqMxData.ShouldBeFalse();
            leadin.TableOfContents.HasMetaData.ShouldBeTrue();
            leadin.TableOfContents.HasRawData.ShouldBeTrue();
            leadin.TableOfContents.NumbersAreBigEndian.ShouldBeFalse();
            leadin.TableOfContents.RawDataIsInterleaved.ShouldBeFalse();
            leadin.Version.ShouldEqual(Version);
            leadin.Offset.ShouldEqual(0);
            leadin.NextSegmentOffset.ShouldEqual(2132223);
            leadin.RawDataOffset.ShouldEqual(2303);
            leadin.MetadataOffset.ShouldEqual(Reader.Segment.Length);
        }

        [Test]
        public void Should_Get_Second_Segment()
        {
            var leadin = Reader.ReadSegment(
                         Reader.ReadFirstSegment().NextSegmentOffset);
            leadin.ShouldNotBeNull();
            leadin.Identifier.ShouldEqual(Identifier);
            leadin.TableOfContents.ContainsNewObjects.ShouldBeTrue();
            leadin.TableOfContents.HasDaqMxData.ShouldBeFalse();
            leadin.TableOfContents.HasMetaData.ShouldBeTrue();
            leadin.TableOfContents.HasRawData.ShouldBeTrue();
            leadin.TableOfContents.NumbersAreBigEndian.ShouldBeFalse();
            leadin.TableOfContents.RawDataIsInterleaved.ShouldBeFalse();
            leadin.Version.ShouldEqual(Version);
            leadin.Offset.ShouldEqual(2132223);
            leadin.NextSegmentOffset.ShouldEqual(4733159);
            leadin.RawDataOffset.ShouldEqual(2133159);
            leadin.MetadataOffset.ShouldEqual(leadin.Offset + Reader.Segment.Length);
        }

        [Test]
        public void Should_Get_Third_Segment()
        {
            var leadin = Reader.ReadSegment(
                         Reader.ReadSegment(
                         Reader.ReadFirstSegment().NextSegmentOffset).NextSegmentOffset);
            leadin.ShouldNotBeNull();
            leadin.Identifier.ShouldEqual(Identifier);
            leadin.TableOfContents.ContainsNewObjects.ShouldBeTrue();
            leadin.TableOfContents.HasDaqMxData.ShouldBeFalse();
            leadin.TableOfContents.HasMetaData.ShouldBeTrue();
            leadin.TableOfContents.HasRawData.ShouldBeTrue();
            leadin.TableOfContents.NumbersAreBigEndian.ShouldBeFalse();
            leadin.TableOfContents.RawDataIsInterleaved.ShouldBeFalse();
            leadin.Version.ShouldEqual(Version);
            leadin.Offset.ShouldEqual(4733159);
            leadin.NextSegmentOffset.ShouldEqual(7334099);
            leadin.RawDataOffset.ShouldEqual(4734099);
            leadin.MetadataOffset.ShouldEqual(leadin.Offset + Reader.Segment.Length);
        }

        [Test]
        public void Should_Get_Fourth_Segment()
        {
            var leadin = Reader.ReadSegment(
                         Reader.ReadSegment(
                         Reader.ReadSegment(
                         Reader.ReadFirstSegment().NextSegmentOffset).NextSegmentOffset).NextSegmentOffset);
            leadin.ShouldNotBeNull();
            leadin.Identifier.ShouldEqual(Identifier);
            leadin.TableOfContents.ContainsNewObjects.ShouldBeTrue();
            leadin.TableOfContents.HasDaqMxData.ShouldBeFalse();
            leadin.TableOfContents.HasMetaData.ShouldBeTrue();
            leadin.TableOfContents.HasRawData.ShouldBeTrue();
            leadin.TableOfContents.NumbersAreBigEndian.ShouldBeFalse();
            leadin.TableOfContents.RawDataIsInterleaved.ShouldBeFalse();
            leadin.Version.ShouldEqual(Version);
            leadin.Offset.ShouldEqual(7334099);
            leadin.NextSegmentOffset.ShouldEqual(-1);
            leadin.RawDataOffset.ShouldEqual(7340099);
            leadin.MetadataOffset.ShouldEqual(leadin.Offset + Reader.Segment.Length);
        }

        [Test]
        public void Should_Not_Get_Fifth_Segment()
        {
            var leadin = Reader.ReadSegment(
                         Reader.ReadSegment(
                         Reader.ReadSegment(
                         Reader.ReadSegment(
                         Reader.ReadFirstSegment().NextSegmentOffset).NextSegmentOffset).NextSegmentOffset).NextSegmentOffset);
            leadin.ShouldBeNull();
        }
    }
}