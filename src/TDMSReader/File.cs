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

        public void Open()
        {
            var reader = new Reader(_stream.Value);
            var metadata = LoadMetadata(reader).ToList();
            LoadFile(metadata);
            LoadGroups(Groups, metadata);
            LoadChannels(Groups, metadata, reader);
        }

        private void LoadFile(IEnumerable<Reader.Metadata> metadata)
        {
            var fileMetadata = metadata.Where(x => x.Path.Length == 0).Select(m => m.Properties).ToList();

            foreach (var property in fileMetadata.SelectMany(properties => properties))
            {
                this.Properties.Add(property);
            }
        }

        private static void LoadGroups(IDictionary<string, Group> groups, IEnumerable<Reader.Metadata> metadata)
        {
            var groupMetadata = metadata.Where(x => x.Path.Length == 1).
                                         GroupBy(x => x.Path[0], (k, r) => r.OrderByDescending(y => y.Properties.Count).First());
            foreach (var group in groupMetadata) 
                groups.Add(group.Path[0], new Group(group.Path[0], group.Properties));
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
            return GetSegments(reader).ToList().SelectMany(reader.ReadMetadata, (s, m) => m).ToList();
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