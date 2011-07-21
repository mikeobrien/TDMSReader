using System;
using System.IO;
using System.Text;

namespace TDMSReader
{
    public static class BinaryReaderExtensions
    {
        public static string ReadString(this BinaryReader reader, int length)
        {
            return Encoding.UTF8.GetString(reader.ReadBytes(length));
        }

        public static string ReadLengthPrefixedString(this BinaryReader reader)
        {
            return Read<string>(reader, DataType.String);
        }

        public static T Read<T>(this BinaryReader reader, int dataType)
        {
            return (T)Read(reader, dataType);
        }

        public static object Read(this BinaryReader reader, int dataType)
        {
            object value;
            switch (dataType)
            {
                case DataType.Empty: value = null; break;
                case DataType.Void: value = null; reader.ReadByte(); break;
                case DataType.Integer8: value = reader.ReadSByte(); break;
                case DataType.Integer16: value = reader.ReadInt16(); break;
                case DataType.Integer32: value = reader.ReadInt32(); break;
                case DataType.Integer64: value = reader.ReadInt64(); break;
                case DataType.UnsignedInteger8: value = reader.ReadByte(); break;
                case DataType.UnsignedInteger16: value = reader.ReadUInt16(); break;
                case DataType.UnsignedInteger32: value = reader.ReadUInt32(); break;
                case DataType.UnsignedInteger64: value = reader.ReadUInt64(); break;
                case DataType.SingleFloat:
                case DataType.SingleFloatWithUnit: value = reader.ReadSingle(); break;
                case DataType.DoubleFloat:
                case DataType.DoubleFloatWithUnit: value = reader.ReadDouble(); break;
                case DataType.String: value = Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32())); break;
                case DataType.Boolean: value = reader.ReadBoolean(); break;
                case DataType.TimeStamp: reader.ReadInt64(); value = new DateTime(1904, 1, 1).AddSeconds(reader.ReadInt64()); break;
                default: throw new ArgumentException("Unknown data type " + dataType, "dataType");
            }
            return value;
        }
    }
}