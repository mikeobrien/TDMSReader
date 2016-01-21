using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NationalInstruments.Tdms
{
    public class Channel
    {
        private readonly IEnumerable<Reader.RawData> _rawData;
        private readonly Reader _reader;

        public Channel(string name, IDictionary<string, object> properties, IEnumerable<Reader.RawData> rawData, Reader reader)
        {
            _rawData = rawData;
            _reader = reader;
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
            return _rawData.SelectMany(_reader.ReadRawData).Select(value => (T)value);
        }
    }
}