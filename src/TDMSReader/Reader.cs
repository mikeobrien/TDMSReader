using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            leadin.TableOfContentsMask = _reader.ReadInt32();
            leadin.Version = _reader.ReadInt32();
            leadin.NextSegmentOffset = ResetIfPastEndOfFile(
                                 _reader.BaseStream.Length, _reader.ReadInt64() + offset + LeadIn.Length);
            leadin.RawDataOffset = _reader.ReadInt64() + offset + LeadIn.Length;
            return leadin;
        }

        public IList<Metadata> ReadMetadata(long offset)
        {
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            var objectCount = _reader.ReadInt32();
            var metadatas = new List<Metadata>();
            for (var x = 0; x < objectCount; x++)
            {
                var metadata = new Metadata();
                metadata.Path = Encoding.UTF8.GetString(_reader.ReadBytes(_reader.ReadInt32()));
                metadata.RawData = new Data { Length = _reader.ReadInt32() };
                if (metadata.RawData.Length > 0)
                {
                    metadata.RawData.DataType = _reader.ReadInt32();
                    metadata.RawData.Dimension = _reader.ReadInt32();
                    metadata.RawData.Count = _reader.ReadInt64();
                    if (metadata.RawData.Length == 28) 
                        metadata.RawData.Size = _reader.ReadInt64();
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
            var data = new List<object>();
            _reader.BaseStream.Seek(offset, SeekOrigin.Begin);
            for (var x = 0; x < count; x++)
            {
                if (offset == 7340099) Debug.WriteLine("Position {0}, Index {1}, Count {2}", _reader.BaseStream.Position, x, count);
                data.Add(_valueReader.Read(dataType));
            }
            return data;
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

        private static long ResetIfPastEndOfFile(long length, long offset)
        {
            return offset >= length ? -1 : offset;
        }

        public class LeadIn
        {
            public const long Length = 28;
            public long Offset { get; set; }
            public long MetadataOffset { get; set; }
            public long RawDataOffset { get; set; }
            public long NextSegmentOffset { get; set; }
            public string Identifier { get; set; }
            public int TableOfContentsMask { get; set; }
            public int Version { get; set; }
        }

        public class Metadata
        {
            public string Path { get; set; }
            public Data RawData { get; set; }
            public IDictionary<string, object> Properties { get; set; } 
        }

        public class Data
        {
            public int Length { get; set; }
            public int DataType { get; set; }
            public int Dimension { get; set; }
            public long Count { get; set; }
            public long Size { get; set; }
        }
    }
}