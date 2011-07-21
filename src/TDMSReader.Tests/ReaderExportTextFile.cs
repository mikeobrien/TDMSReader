using System.IO;
using System.Linq;
using NUnit.Framework;

namespace TDMSReader.Tests
{
    [TestFixture]
    public class ReadeExportTextFile : ReaderTestsBase
    {
        [Test, Ignore]
        public void Should_Read_Raw_Data()
        {
            var file = new StreamWriter(File.Create(@"D:\temp\tdms.sample.txt"));

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
                        file.WriteLine("\t{0} = {1}", property.Key, property.Value);
                    }
                    file.WriteLine("Raw data of {0}: {1}", metadata.RawData.DataType, metadata.RawData.Count);
                    var rawData = Reader.ReadRawData(segment.RawDataOffset + metadata.RawData.Offset, metadata.RawData.Count, metadata.RawData.DataType).ToList();
                    foreach (var value in rawData.Take(10))
                    {
                        file.WriteLine("\t{0}", value);
                    }
                    if (rawData.Count > 10) file.WriteLine("\t...");
                }
                segment = Reader.ReadSegment(segment.NextSegmentOffset);
            }

            file.Close();
        }
    }
}