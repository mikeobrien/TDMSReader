using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace TDMSReader
{
    public class File : IEnumerable<Group>
    {
        private readonly string _path;

        public File(string path)
        {
            _path = path;
            Properties = new Dictionary<string, object>();
            Groups = new Dictionary<string, Group>();
        }

        public IDictionary<string, object> Properties { get; private set; }
        public IDictionary<string, Group> Groups { get; private set; } 

        public void Open()
        {
            using (var reader = new Reader(_path))
            {
                var metadata = LoadMetadata(reader).ToList();
                LoadFile(metadata);
                LoadGroups(Groups, metadata);
                LoadChannels(Groups, metadata, _path);
            }
        }

        private void LoadFile(IEnumerable<Reader.Metadata> metadata)
        {
            var fileMetadata = metadata.Where(x => x.Path.Length == 0).
                                        OrderByDescending(x => x.Properties.Count).FirstOrDefault();
            if (fileMetadata != null) Properties = fileMetadata.Properties;
        }

        private static void LoadGroups(IDictionary<string, Group> groups, IEnumerable<Reader.Metadata> metadata)
        {
            var groupMetadata = metadata.Where(x => x.Path.Length == 1).
                                         GroupBy(x => x.Path[0], (k, r) => r.OrderByDescending(y => y.Properties.Count).First());
            foreach (var group in groupMetadata) 
                groups.Add(group.Path[0], new Group(group.Path[0], group.Properties));
        }

        private static void LoadChannels(IDictionary<string, Group> groups, IEnumerable<Reader.Metadata> metadata, string path)
        {
            var channelMetadata = metadata.Where(x => x.Path.Length == 2).
                                           GroupBy(x => x.Path[1]).
                                           Join(groups, x => x.First().Path[0], x => x.Key, (c, g) => Tuple.Create(g.Value, c));
            foreach (var channel in channelMetadata)
                channel.Item1.Channels.Add(channel.Item2.First().Path[1], new Channel(channel.Item2.First().Path[1], 
                    channel.Item2.OrderByDescending(y => y.Properties.Count).First().Properties,
                    channel.Item2.Where(x => x.RawData.Count > 0).Select(x => x.RawData), path));
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
    }
}