using System;
using System.Collections.Generic;
using System.Linq;

namespace TDMSReader
{
    public class Channel
    {
        private readonly IEnumerable<Reader.RawData> _rawData;
        private readonly string _path;

        public Channel(string name, IDictionary<string, object> properties, IEnumerable<Reader.RawData> rawData, string path)
        {
            _rawData = rawData;
            _path = path;
            Name = name;
            Properties = properties;
        }

        public string Name { get; private set; }
        public bool HasData { get { return _rawData.Any(); } }
        public long DataCount { get { return _rawData.Sum(x => x.Count); } }
        public Type DataType { get { return _rawData.Select(x => x.ClrDataType).FirstOrDefault(); } }
        public IDictionary<string, object> Properties { get; private set; }

        public IEnumerable<T> GetData<T>()
        {
            if (_rawData.Any(x => x.IsInterleaved))
                throw new NotSupportedException("This library does not support the reading of interleaved data.");
            using (var reader = new Reader(_path))
                foreach (var value in _rawData.SelectMany(reader.ReadRawData)) yield return (T)value;
        }
    }
}