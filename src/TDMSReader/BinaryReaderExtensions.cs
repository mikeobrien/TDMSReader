﻿using System;
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
                    return new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                        .AddSeconds(reader.ReadUInt64() / (double)ulong.MaxValue)       //fixed truncate error in old code 
                        .AddSeconds(reader.ReadInt64())
                        .ToLocalTime(); 
                default: throw new ArgumentException("Unknown data type " + dataType, "dataType");
            }
        }
    }

    public static class BinaryWriterExtensions
    {
        public static void Write(this BinaryWriter writer, int dataType, object data)
        {
            switch (dataType)
            {
                case DataType.Empty: break;
                case DataType.Void: writer.Write((byte)0); break;
                case DataType.Integer8: writer.Write((sbyte)data); break;
                case DataType.Integer16: writer.Write((Int16)data); break;
                case DataType.Integer32: writer.Write((Int32)data); break;
                case DataType.Integer64: writer.Write((Int64)data); break;
                case DataType.UnsignedInteger8: writer.Write((byte)data); break;
                case DataType.UnsignedInteger16: writer.Write((UInt16)data); break;
                case DataType.UnsignedInteger32: writer.Write((UInt32)data); break;
                case DataType.UnsignedInteger64: writer.Write((UInt16)data); break;
                case DataType.SingleFloat:
                case DataType.SingleFloatWithUnit: writer.Write((float)data); break;
                case DataType.DoubleFloat:
                case DataType.DoubleFloatWithUnit: writer.Write((double)data); break;
                case DataType.String:
                    byte[] bytes = Encoding.UTF8.GetBytes((string)data);
                    writer.Write((Int32)bytes.Length);
                    writer.Write(bytes);
                    break;
                case DataType.Boolean: writer.Write((bool)data); break;
                case DataType.TimeStamp:
                    var t = (((DateTime)data).ToUniversalTime() - new DateTime(1904, 1, 1, 0, 0, 0, DateTimeKind.Utc));
                    writer.Write((UInt64)((t.TotalSeconds % 1) * ulong.MaxValue));
                    writer.Write((Int64)t.TotalSeconds);
                    break;
                default: throw new ArgumentException("Unknown data type " + dataType, "dataType");
            }
        }
    }
}