using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TDMSReader
{
    public class Reader : IDisposable
    {
        private readonly ValueReader _valueReader;
        private readonly BinaryReader _reader;

        public Reader(Stream file)
        {
            _reader = new BinaryReader(file);
            _valueReader = new ValueReader(_reader);
        }

        public LeadIn ReadFirstSegment()
        {
            return ReadSegment(0);
        }

        public LeadIn ReadSegment(long offset)
        {
            if (offset < 0 || offset >= _reader.BaseStream.Length) return null;
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            var leadin = new LeadIn { Offset = offset, MetadataOffset = offset + LeadIn.Length };
            leadin.Identifier = Encoding.ASCII.GetString(_reader.ReadBytes(4));
            var tableOfContentsMask = _reader.ReadInt32();
            leadin.TableOfContents = new TableOfContents
                {
                    ContainsNewObjects = ((tableOfContentsMask >> 2) & 1) == 1,
                    HasDaqMxData = ((tableOfContentsMask >> 7) & 1) == 1,
                    HasMetaData = ((tableOfContentsMask >> 1) & 1) == 1,
                    HasRawData = ((tableOfContentsMask >> 3) & 1) == 1,
                    NumbersAreBigEndian = ((tableOfContentsMask >> 6) & 1) == 1,
                    RawDataIsInterleaved = ((tableOfContentsMask >> 5) & 1) == 1
                };
            leadin.Version = _reader.ReadInt32();
            Func<long, long> resetWhenEol = x => x < _reader.BaseStream.Length ? x : -1;
            leadin.NextSegmentOffset = resetWhenEol(_reader.ReadInt64() + offset + LeadIn.Length);
            leadin.RawDataOffset = _reader.ReadInt64() + offset + LeadIn.Length;
            return leadin;
        }

        public IList<Metadata> ReadMetadata(long offset)
        {
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            var objectCount = _reader.ReadInt32();
            var metadatas = new List<Metadata>();
            long rawDataOffset = 0;
            for (var x = 0; x < objectCount; x++)
            {
                var metadata = new Metadata();
                metadata.Path = Encoding.UTF8.GetString(_reader.ReadBytes(_reader.ReadInt32()));
                metadata.RawData = new RawData();
                var rawDataIndexLength = _reader.ReadInt32();
                if (rawDataIndexLength > 0)
                {
                    metadata.RawData.Offset = rawDataOffset;
                    metadata.RawData.DataType = _reader.ReadInt32();
                    metadata.RawData.Dimension = _reader.ReadInt32();
                    metadata.RawData.Count = _reader.ReadInt64();
                    metadata.RawData.Size = rawDataIndexLength == 28 ? _reader.ReadInt64() :
                                                DataType.GetLength(metadata.RawData.DataType) * metadata.RawData.Count;
                    rawDataOffset += metadata.RawData.Size;
                }
                var propertyCount = _reader.ReadInt32();
                metadata.Properties = new Dictionary<string, object>();
                for (var y = 0; y < propertyCount; y++)
                {
                    var key =_valueReader.ReadString();
                    var value =_valueReader.Read(_reader.ReadInt32());
                    metadata.Properties.Add(key, value);
                }
                metadatas.Add(metadata);
            }
            return metadatas;
        }

        public IEnumerable<object> ReadRawData(long offset, long count, int dataType)
        {
            return dataType == DataType.String ? ReadRawStrings(offset, count) : ReadRawFixed(offset, count, dataType);
        }

        private IEnumerable<object> ReadRawFixed(long offset, long count, int dataType)
        {
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            for (var x = 0; x < count; x++) yield return _valueReader.Read(dataType);
        }

        private IEnumerable<object> ReadRawStrings(long offset, long count)
        {
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            var dataOffset = offset + (count * 4);
            var indexPosition = _reader.BaseStream.Position;
            var dataPosition = dataOffset;
            for (var x = 0; x < count; x++)
            {
                var endOfString = _reader.ReadInt32();
                indexPosition = _reader.BaseStream.Position;

                _reader.BaseStream.Seek(dataPosition, SeekOrigin.Begin);
                yield return _valueReader.ReadString((int)((dataOffset + endOfString) - dataPosition));

                dataPosition = dataOffset + endOfString;
                _reader.BaseStream.Seek(indexPosition, SeekOrigin.Begin);
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        public class LeadIn
        {
            public const long Length = 28;
            public long Offset { get; set; }
            public long MetadataOffset { get; set; }
            public long RawDataOffset { get; set; }
            public long NextSegmentOffset { get; set; }
            public string Identifier { get; set; }
            public TableOfContents TableOfContents { get; set; }
            public int Version { get; set; }
        }

        public class TableOfContents
        {
            public bool HasMetaData { get; set; }
            public bool HasRawData { get; set; }
            public bool HasDaqMxData { get; set; }
            public bool RawDataIsInterleaved { get; set; }
            public bool NumbersAreBigEndian { get; set; }
            public bool ContainsNewObjects { get; set; }
        }

        public class Metadata
        {
            public string Path { get; set; }
            public RawData RawData { get; set; }
            public IDictionary<string, object> Properties { get; set; } 
        }

        public class RawData
        {
            public long Offset { get; set; }
            public int DataType { get; set; }
            public int Dimension { get; set; }
            public long Count { get; set; }
            public long Size { get; set; }
        }
    }
}