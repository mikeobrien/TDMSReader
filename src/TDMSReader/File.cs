using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NationalInstruments.Tdms
{
    public class File : IEnumerable<Group>, IDisposable
    {
        private readonly Lazy<Stream> _stream;

        private File()
        {
            Properties = new Dictionary<string, object>();
            Groups = new Dictionary<string, Group>();
        }

        public File(Stream stream) : this()
        {
            if (!stream.CanSeek) throw new ArgumentException("Only seekable streams supported.", "stream");
            _stream = new Lazy<Stream>(() => stream);
        }

        public File(string path) : this()
        {
            _stream = new Lazy<Stream>(() => new FileStream(path, FileMode.Open, FileAccess.Read));
        }

        public IDictionary<string, object> Properties { get; private set; }
        public IDictionary<string, Group> Groups { get; private set; } 

        public File Open()
        {
            var reader = new Reader(_stream.Value);
            var metadata = LoadMetadata(reader).ToList();
            LoadFile(metadata);
            LoadGroups(Groups, metadata);
            LoadChannels(Groups, metadata, reader);
            return this;
        }
        
        private void LoadFile(IEnumerable<Reader.Metadata> metadata)
        {
            metadata.Where(x => x.Path.Length == 0)
                .SelectMany(m => m.Properties).ToList()
                .ForEach(x => Properties.Add(x));
        }

        private static void LoadGroups(IDictionary<string, Group> groups, IEnumerable<Reader.Metadata> metadata)
        {
            var groupMetadata = metadata.Where(x => x.Path.Length == 1).
                                         GroupBy(x => x.Path[0], (k, r) => r.OrderByDescending(y => y.Properties.Count).First());
            foreach (var group in groupMetadata) 
                groups.Add(group.Path[0], new Group(group.Path[0], group.Properties));

            // add implicit groups
            foreach (var groupName in metadata.Where(x => x.Path.Length > 1 && !groups.ContainsKey(x.Path[0])).Select(x => x.Path[0]))
                groups.Add(groupName, new Group(groupName, new Dictionary<string, object>()));
        }

        private static void LoadChannels(IDictionary<string, Group> groups, IEnumerable<Reader.Metadata> metadata, Reader reader)
        {
            var channelMetadata = metadata.Where(x => x.Path.Length == 2).
                                           GroupBy(x => x.Path[1]).
                                           Join(groups, x => x.First().Path[0], x => x.Key, (c, g) => Tuple.Create(g.Value, c));
            foreach (var channel in channelMetadata)
                channel.Item1.Channels.Add(channel.Item2.First().Path[1], new Channel(channel.Item2.First().Path[1], 
                    channel.Item2.OrderByDescending(y => y.Properties.Count).First().Properties,
                    channel.Item2.Where(x => x.RawData.Count > 0).Select(x => x.RawData), reader));
        }

        private static IEnumerable<Reader.Metadata> LoadMetadata(Reader reader)
        {

            var segments = GetSegments(reader).ToList();

            var segmentMetadata = new List<Tuple<Reader.Segment, List<Reader.Metadata>>>();

            Tuple<Reader.Segment, List<Reader.Metadata>> prevSegment = null;
            foreach (var segment in segments)
            {
                var metadatas = reader.ReadMetadata(segment);
                long rawDataSize = 0;
                long nextOffset = segment.RawDataOffset;

                foreach (var m in metadatas)
                {
                    if (m.RawData.Count == 0 && prevSegment != null && m.Path.Length > 1)
                    {
                        // apply previous metadata if available
                        var prevMetaData = prevSegment.Item2.First(md => md.Path.Length > 1 && md.Path[1] == m.Path[1]);
                        m.RawData.Count = prevMetaData.RawData.Count;
                        m.RawData.DataType = prevMetaData.RawData.DataType;
                        m.RawData.ClrDataType = prevMetaData.RawData.ClrDataType;
                        m.RawData.Offset = segment.RawDataOffset + rawDataSize;
                        m.RawData.IsInterleaved = prevMetaData.RawData.IsInterleaved;
                        m.RawData.InterleaveStride = prevMetaData.RawData.InterleaveStride;
                        m.RawData.Size = prevMetaData.RawData.Size;
                        m.RawData.Dimension = prevMetaData.RawData.Dimension;
                    }
                    if (m.RawData.IsInterleaved && segment.NextSegmentOffset <= 0)
                    {
                        m.RawData.Count = segment.NextSegmentOffset > 0
                            ? (segment.NextSegmentOffset - m.RawData.Offset + m.RawData.InterleaveStride - 1)/
                              m.RawData.InterleaveStride
                            : (reader.FileSize - m.RawData.Offset + m.RawData.InterleaveStride - 1)/
                              m.RawData.InterleaveStride;

                    }
                    if (m.Path.Length > 1)
                    {
                        rawDataSize += m.RawData.Size;
                        nextOffset += m.RawData.Size;
                    }
                }

                var implicitMetadatas = new List<Reader.Metadata>();
                if (metadatas.All(m => !m.RawData.IsInterleaved && m.RawData.Size > 0))
                {
                    while (nextOffset < segment.NextSegmentOffset ||
                           (segment.NextSegmentOffset == -1 && nextOffset < reader.FileSize))
                    {
                        // Incremental Meta Data see http://www.ni.com/white-paper/5696/en/#toc1
                        foreach (var m in metadatas)
                        {
                            if (m.Path.Length > 1)
                            {
                                var implicitMetadata = new Reader.Metadata()
                                {
                                    Path = m.Path,
                                    RawData = new Reader.RawData()
                                    {
                                        Count = m.RawData.Count,
                                        DataType = m.RawData.DataType,
                                        ClrDataType = m.RawData.ClrDataType,
                                        Offset = nextOffset,
                                        IsInterleaved = m.RawData.IsInterleaved,
                                        Size = m.RawData.Size,
                                        Dimension = m.RawData.Dimension
                                    },
                                    Properties = m.Properties
                                };
                                implicitMetadatas.Add(implicitMetadata);
                                nextOffset += implicitMetadata.RawData.Size;
                            }
                        }
                    }
                }
                var metadataWIthImplicit = metadatas.Concat(implicitMetadatas).ToList();
                prevSegment = Tuple.Create(segment, metadataWIthImplicit);
                segmentMetadata.Add(prevSegment);
            }

            return segmentMetadata.SelectMany(st => st.Item2);
        }

        private static IEnumerable<Reader.Segment> GetSegments(Reader reader)
        {
            var segment = reader.ReadFirstSegment();
            while (segment != null)
            {
                yield return segment;
                segment = reader.ReadSegment(segment.NextSegmentOffset);
            }
        }

        public IEnumerator<Group> GetEnumerator() { return Groups.Values.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        public void Dispose()
        {
            _stream.Value.Dispose();
        }
    }
}