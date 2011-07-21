using System;
using System.IO;
using System.Text;

namespace TDMSReader
{
    public class ValueReader
    {
        private readonly BinaryReader _reader;

        public ValueReader(BinaryReader reader)
        {
            _reader = reader;
        }

        public string ReadString(int length)
        {
            return Encoding.UTF8.GetString(_reader.ReadBytes(length));
        }

        public string ReadString()
        {
            return Read<string>(DataType.String);
        }

        public T Read<T>(int dataType)
        {
            return (T)Read(dataType);
        }

        public object Read(int dataType)
        {
            object value;
            switch (dataType)
            {
                case DataType.Empty: value = null; break;
                case DataType.Void: value = null; _reader.ReadByte(); break;
                case DataType.Integer8: value = _reader.ReadSByte(); break;
                case DataType.Integer16: value = _reader.ReadInt16(); break;
                case DataType.Integer32: value = _reader.ReadInt32(); break;
                case DataType.Integer64: value = _reader.ReadInt64(); break;
                case DataType.UnsignedInteger8: value = _reader.ReadByte(); break;
                case DataType.UnsignedInteger16: value = _reader.ReadUInt16(); break;
                case DataType.UnsignedInteger32: value = _reader.ReadUInt32(); break;
                case DataType.UnsignedInteger64: value = _reader.ReadUInt64(); break;
                case DataType.SingleFloat:
                case DataType.SingleFloatWithUnit: value = _reader.ReadSingle(); break;
                case DataType.DoubleFloat:
                case DataType.DoubleFloatWithUnit: value = _reader.ReadDouble(); break;
                case DataType.String: value = Encoding.UTF8.GetString(_reader.ReadBytes(_reader.ReadInt32())); break;
                case DataType.Boolean: value = _reader.ReadBoolean(); break;
                case DataType.TimeStamp: _reader.ReadInt64(); value = new DateTime(1904, 1, 1).AddSeconds(_reader.ReadInt64()); break;
                default: throw new Exception("Unknown data type " + dataType);
            }
            return value;
        }
    }
}