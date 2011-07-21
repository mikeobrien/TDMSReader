using System.IO;
using System.Linq;
using NUnit.Framework;
using Should;

namespace TDMSReader.Tests
{
    [TestFixture]
    public class ReadeExportTextTests : ReaderTestsBase
    {
        [Test, Ignore]
        public void Should_Read_Raw_Data()
        {
            var file = new StreamWriter(System.IO.File.Create(@"D:\temp\tdms.sample.txt"));

            var segment = Reader.ReadFirstSegment();
            while (segment != null)
            {
                var metadatas = Reader.ReadMetadata(segment.MetadataOffset);
                file.WriteLine("********************** Segment **********************");
                foreach (var metadata in metadatas)
                {
                    file.WriteLine("------ Metadata -----");
                    file.WriteLine("Path: {0}", metadata.Path);
                    file.WriteLine("Properties: {0}", metadata.Properties.Count);
                    foreach (var property in metadata.Properties)
                    {
                        file.WriteLine("\t{0} = {1}", property.Key, property);
                    }
                    file.WriteLine("Raw data of {0}: {1}", metadata.RawData.DataType, metadata.RawData.Count);
                    var rawData = Reader.ReadRawData(segment.RawDataOffset, metadata.RawData.Count, metadata.RawData.DataType);
                    foreach (var value in rawData)
                    {
                        file.WriteLine("\t{0}", value);
                    }
                    file.WriteLine("-------------------");
                }
                file.WriteLine("********************************************");
                segment = Reader.ReadSegment(segment.NextSegmentOffset);
            }

            file.Close();
        }
    }
}