using System;
using System.IO;
using System.Text;

namespace NationalInstruments.Tdms
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
            switch (dataType)
            {
                case DataType.Empty: return null;
                case DataType.Void: reader.ReadByte(); return null;
                case DataType.Integer8: return reader.ReadSByte();
                case DataType.Integer16: return reader.ReadInt16();
                case DataType.Integer32: return reader.ReadInt32();
                case DataType.Integer64: return reader.ReadInt64();
                case DataType.UnsignedInteger8: return reader.ReadByte();
                case DataType.UnsignedInteger16: return reader.ReadUInt16();
                case DataType.UnsignedInteger32: return reader.ReadUInt32();
                case DataType.UnsignedInteger64: return reader.ReadUInt64();
                case DataType.SingleFloat:
                case DataType.SingleFloatWithUnit: return reader.ReadSingle();
                case DataType.DoubleFloat:
                case DataType.DoubleFloatWithUnit: return reader.ReadDouble();
                case DataType.String: return Encoding.UTF8.GetString(reader.ReadBytes(reader.ReadInt32()));
                case DataType.Boolean: return reader.ReadBoolean();
                case DataType.TimeStamp: 
                    reader.ReadInt64(); 
                    return new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(reader.ReadInt64()); 
                default: throw new ArgumentException("Unknown data type " + dataType, "dataType");
            }
        }
    }
}