using System.Collections;
using System.Collections.Generic;

namespace TDMSReader
{
    public class Group : IEnumerable<Channel>
    {
        public Group(string name, IDictionary<string, object> properties)
        {
            Name = name;
            Properties = properties;
            Channels = new Dictionary<string, Channel>();
        }

        public string Name { get; private set; }
        public IDictionary<string, object> Properties { get; private set; } 
        public IDictionary<string, Channel> Channels { get; private set; }

        public IEnumerator<Channel> GetEnumerator() { return Channels.Values.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}