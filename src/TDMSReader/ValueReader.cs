using System;
using System.IO;
using System.Text;

namespace TDMSReader
{
    public class ValueReader
    {
        public const int Empty = 0x0000000F;
        public const int Void = 0x00000000;
        public const int Integer8 = 0x00000001;
        public const int Integer16 = 0x00000002;
        public const int Integer32 = 0x00000003;
        public const int Integer64 = 0x00000004;
        public const int UnsignedInteger8 = 0x00000005;
        public const int UnsignedInteger16 = 0x00000006;
        public const int UnsignedInteger32 = 0x00000007;
        public const int UnsignedInteger64 = 0x00000008;
        public const int SingleFloat = 0x00000009;
        public const int DoubleFloat = 0x0000000A;
        public const int ExtendedFloat = 0x0000000B;
        public const int SingleFloatWithUnit = 0x00000019;
        public const int DoubleFloatWithUnit = 0x0000001A;
        public const int ExtendedFloatWithUnit = 0x0000001B;
        public const int String = 0x00000020;
        public const int Boolean = 0x00000021;
        public const int TimeStamp = 0x00000044;

        private readonly BinaryReader _reader;

        public ValueReader(BinaryReader reader)
        {
            _reader = reader;
        }

        public string ReadString()
        {
            return Read<string>(String);
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
                case Empty: value = null; break;
                case Void: value = null; _reader.ReadByte(); break;
                case Integer8: value = _reader.ReadSByte(); break;
                case Integer16: value = _reader.ReadInt16(); break;
                case Integer32: value = _reader.ReadInt32(); break;
                case Integer64: value = _reader.ReadInt64(); break;
                case UnsignedInteger8: value = _reader.ReadByte(); break;
                case UnsignedInteger16: value = _reader.ReadUInt16(); break;
                case UnsignedInteger32: value = _reader.ReadUInt32(); break;
                case UnsignedInteger64: value = _reader.ReadUInt64(); break;
                case SingleFloat:
                case SingleFloatWithUnit: value = _reader.ReadSingle(); break;
                case DoubleFloat:
                case DoubleFloatWithUnit: value = _reader.ReadDouble(); break;
                case String: value = Encoding.UTF8.GetString(_reader.ReadBytes(_reader.ReadInt32())); break;
                case Boolean: value = _reader.ReadBoolean(); break;
                case TimeStamp: _reader.ReadInt64(); value = new DateTime(1904, 1, 1).AddSeconds(_reader.ReadInt64()); break;
                default: throw new Exception("Unknown data type " + dataType);
            }
            return value;
        }
    }
}