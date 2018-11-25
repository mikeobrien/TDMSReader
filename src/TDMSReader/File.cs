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

        /// <summary>
        /// This will re-write the TDMS file. Mostly used for write demonstration. Although, this will also defragment the file. 
        /// </summary>
        /// <param name="path"></param>
        public void ReWrite(string path)
        {
            using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.Write))
                ReWrite(stream);
        }

        /// <summary>
        /// This will re-write the TDMS file. Mostly used for write demonstration. Although, this will also defragment the file. 
        /// </summary>
        /// <param name="stream"></param>
        public void ReWrite(Stream stream)
        {
            WriteSegment segment = new WriteSegment(stream);
            segment.Header.TableOfContents.HasRawData = Groups.SelectMany(g => g.Value.Channels.Values, (g, c) => c.HasData).Any();
            //when we re-write the file, no data shall be interleaved. (It's an all or nothing situation, with only 1 segment)
            //segment.Header.TableOfContents.RawDataIsInterleaved = Groups.SelectMany(g => g.Value.Channels.Values, (g, c) => c.RawData.First().IsInterleaved).Any();

            //Top level
            Reader.Metadata m = new Reader.Metadata();
            m.Path = new string[0];
            m.Properties = Properties;
            segment.MetaData.Add(m);

            //Groups
            foreach(KeyValuePair<string, Group> group in Groups)
            {
                m = new Reader.Metadata();
                m.Path = new string[] { group.Key };
                m.Properties = group.Value.Properties;
                segment.MetaData.Add(m);

                //Channels
                foreach (KeyValuePair<string, Channel> channel in group.Value.Channels)
                {
                    Reader.RawData[] raws = channel.Value.RawData.ToArray();

                    //Add first part
                    m = new Reader.Metadata();
                    m.Path = new string[] { group.Key, channel.Key };
                    m.Properties = channel.Value.Properties;
                    m.RawData = raws?[0];
                    segment.MetaData.Add(m);

                    //Add the other parts (if any)
                    for(int i = 1; i < raws?.Length; i++)
                    {
                        m = new Reader.Metadata();
                        m.Path = new string[] { group.Key, channel.Key };
                        m.RawData = raws[i];
                        segment.MetaData.Add(m);
                    }
                }
            }

            //Write all raw data
            Writer writer = segment.Open();
            var reader = new Reader(_stream.Value);
            foreach (KeyValuePair<string, Group> group in Groups)
                foreach (KeyValuePair<string, Channel> channel in group.Value.Channels)
                    foreach (Reader.RawData raw in channel.Value.RawData)
                    {
                        var data = reader.ReadRawData(raw);
                        raw.IsInterleaved = false;  //when we re-write the file, no data shall be interleaved
                        writer.WriteRawData(raw, data);
                    }

            //close up
            segment.Close();
        }

        private void LoadFile(IEnumerable<Reader.Metadata> metadata)
        {
            metadata.Where(x => x.Path.Length == 0)
                .SelectMany(m => m.Properties).ToList()
                .ForEach(x => Properties[x.Key] = x.Value);
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
            var channelMetadata = metadata
                .Where(x => x.Path.Length == 2)
                .GroupBy(x => Tuple.Create(x.Path[0], x.Path[1]))
                .Join(groups, x => x.First().Path[0], x => x.Key, (c, g) => Tuple.Create(g.Value, c));
            foreach (var channel in channelMetadata)
                channel.Item1.Channels.Add(channel.Item2.First().Path[1], new Channel(channel.Item2.First().Path[1],
                    channel.Item2.OrderByDescending(y => y.Properties.Count).First().Properties,
                    channel.Item2.Where(x => x.RawData.Count > 0).Select(x => x.RawData), reader));
        }

        private static IEnumerable<Reader.Metadata> LoadMetadata(Reader reader)
        {
            var segments = GetSegments(reader).ToList();

            var prevMetaDataLookup = new Dictionary<string, Dictionary<string, Reader.Metadata>>();
            foreach (var segment in segments)
            {
                if (!(segment.TableOfContents.ContainsNewObjects || 
                    segment.TableOfContents.HasDaqMxData || 
                    segment.TableOfContents.HasMetaData || 
                    segment.TableOfContents.HasRawData)) {
                    continue;
                }
                var metadatas = reader.ReadMetadata(segment);
                long rawDataSize = 0;
                long nextOffset = segment.RawDataOffset;

                foreach (var m in metadatas)
                {
                    if (m.RawData.Count == 0 && m.Path.Length > 1)
                    {
                        // apply previous metadata if available
                        if (prevMetaDataLookup.ContainsKey(m.Path[0]) && prevMetaDataLookup[m.Path[0]].ContainsKey(m.Path[1]))
                        {
                            var prevMetaData = prevMetaDataLookup[m.Path[0]][m.Path[1]];
                            if (prevMetaData != null)
                            {
                                m.RawData.Count = segment.TableOfContents.HasRawData ? prevMetaData.RawData.Count : 0;
                                m.RawData.DataType = prevMetaData.RawData.DataType;
                                m.RawData.ClrDataType = prevMetaData.RawData.ClrDataType;
                                m.RawData.Offset = segment.RawDataOffset + rawDataSize;
                                m.RawData.IsInterleaved = prevMetaData.RawData.IsInterleaved;
                                m.RawData.InterleaveStride = prevMetaData.RawData.InterleaveStride;
                                m.RawData.Size = prevMetaData.RawData.Size;
                                m.RawData.Dimension = prevMetaData.RawData.Dimension;
                            }
                        }
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
                var metadataWithImplicit = metadatas.Concat(implicitMetadatas).ToList();
                foreach (var metadata in metadataWithImplicit)
                {
                    if (metadata.Path.Length == 2)
                    {
                        if (!prevMetaDataLookup.ContainsKey(metadata.Path[0]))
                        {
                            prevMetaDataLookup[metadata.Path[0]] = new Dictionary<string, Reader.Metadata>();
                        }
                        prevMetaDataLookup[metadata.Path[0]][metadata.Path[1]] = metadata;
                    }
                    yield return metadata;
                }
            }
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