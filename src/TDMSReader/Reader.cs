using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NationalInstruments.Tdms
{
    public class Reader
    {
        private readonly BinaryReader _reader;

        public Reader(Stream stream)
        {
            _reader = new BinaryReader(stream);
        }

        public Segment ReadFirstSegment()
        {
            return ReadSegment(0);
        }

        public Segment ReadSegment(long offset)
        {
            if (offset < 0 || offset >= _reader.BaseStream.Length) return null;
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            var leadin = new Segment { Offset = offset, MetadataOffset = offset + Segment.Length };
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
            leadin.NextSegmentOffset = resetWhenEol(_reader.ReadInt64() + offset + Segment.Length);
            leadin.RawDataOffset = _reader.ReadInt64() + offset + Segment.Length;
            return leadin;
        }

        public IList<Metadata> ReadMetadata(Segment segment)
        {
            _reader.BaseStream.Seek(segment.MetadataOffset, SeekOrigin.Begin);
            var objectCount = _reader.ReadInt32();
            var metadatas = new List<Metadata>();
            var rawDataOffset = segment.RawDataOffset;
            bool isInterleaved = segment.TableOfContents.RawDataIsInterleaved;
            int interleaveStride = 0;
            for (var x = 0; x < objectCount; x++)
            {
                var metadata = new Metadata();
                metadata.Path = Regex.Matches(Encoding.UTF8.GetString(_reader.ReadBytes(_reader.ReadInt32())), "'(.*?)'").Cast<Match>().
                                SelectMany(y => y.Groups.Cast<System.Text.RegularExpressions.Group>().Skip(1), (m, g) => g.Value).ToArray();
                metadata.RawData = new RawData();
                var rawDataIndexLength = _reader.ReadInt32();
                if (rawDataIndexLength > 0)
                {
                    metadata.RawData.Offset = rawDataOffset;
                    metadata.RawData.IsInterleaved = segment.TableOfContents.RawDataIsInterleaved;
                    metadata.RawData.DataType = _reader.ReadInt32();
                    metadata.RawData.ClrDataType = DataType.GetClrType(metadata.RawData.DataType);
                    metadata.RawData.Dimension = _reader.ReadInt32();
                    metadata.RawData.Count = _reader.ReadInt64();
                    metadata.RawData.Size = rawDataIndexLength == 28 ? _reader.ReadInt64() :
                                                DataType.GetArrayLength(metadata.RawData.DataType, metadata.RawData.Count);
                    if (isInterleaved)
                    {
                        //fixed error. The interleave stride is the sum of all channel (type) dataSizes
                        rawDataOffset += DataType.GetLength(metadata.RawData.DataType);
                        interleaveStride += DataType.GetLength(metadata.RawData.DataType);
                    }
                    else
                        rawDataOffset += metadata.RawData.Size;
                }
                var propertyCount = _reader.ReadInt32();
                metadata.Properties = new Dictionary<string, object>();
                for (var y = 0; y < propertyCount; y++)
                {
                    var key = _reader.ReadLengthPrefixedString();
                    var value = _reader.Read(_reader.ReadInt32());
                    metadata.Properties[key] = value;
                }
                metadatas.Add(metadata);
            }
            if (isInterleaved)
            {
                foreach (var metadata in metadatas)
                {
                    if (metadata.RawData.ClrDataType != null)
                    {
                        metadata.RawData.InterleaveStride = interleaveStride;

                        metadata.RawData.Count = segment.NextSegmentOffset > 0
                            ? (segment.NextSegmentOffset - metadata.RawData.Offset + interleaveStride - 1) / interleaveStride
                            : (_reader.BaseStream.Length - metadata.RawData.Offset + interleaveStride - 1) / interleaveStride;
                    }
                }
            }
            return metadatas;
        }

        public IEnumerable<object> ReadRawData(RawData rawData)
        {
            if (rawData.IsInterleaved)
                return ReadRawInterleaved(rawData.Offset, rawData.Count, rawData.DataType, rawData.InterleaveStride - DataType.GetLength(rawData.DataType));    //fixed error
            return rawData.DataType == DataType.String ? ReadRawStrings(rawData.Offset, rawData.Count) :
                                                         ReadRawFixed(rawData.Offset, rawData.Count, rawData.DataType);
        }

        private IEnumerable<object> ReadRawFixed(long offset, long count, int dataType)
        {
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            for (var x = 0; x < count; x++) yield return _reader.Read(dataType);
        }

        private IEnumerable<object> ReadRawInterleaved(long offset, long count, int dataType, int interleaveSkip)
        {
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            for (var x = 0; x < count; x++)
            {
                var value = _reader.Read(dataType);
                _reader.BaseStream.Seek(interleaveSkip, SeekOrigin.Current);
                yield return value;
            }
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
                yield return _reader.ReadString((int)((dataOffset + endOfString) - dataPosition));

                dataPosition = dataOffset + endOfString;
                _reader.BaseStream.Seek(indexPosition, SeekOrigin.Begin);
            }
        }

        public long FileSize
        {
            get { return _reader.BaseStream.Length; }
        }

        public class Segment
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
            public string[] Path { get; set; }
            public RawData RawData { get; set; }
            public IDictionary<string, object> Properties { get; set; }
        }

        public class RawData
        {
            public long Offset { get; set; }
            public int DataType { get; set; }
            public Type ClrDataType { get; set; }
            public int Dimension { get; set; }
            public long Count { get; set; }
            public long Size { get; set; }
            public bool IsInterleaved { get; set; }
            public int InterleaveStride { get; set; }
        }
    }
}