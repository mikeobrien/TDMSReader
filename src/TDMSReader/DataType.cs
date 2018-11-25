using System;

namespace NationalInstruments.Tdms
{
    public class DataType
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

        public static long GetArrayLength(int dataType, long size)
        {
            return GetLength(dataType) * size;
        }

        public static int GetLength(int dataType)
        {
            switch (dataType)
            {
                case Empty: return 0;
                case Void: return 1;
                case Integer8: return 1;
                case Integer16: return 2;
                case Integer32: return 4;
                case Integer64: return 8;
                case UnsignedInteger8: return 1;
                case UnsignedInteger16: return 2;
                case UnsignedInteger32: return 4;
                case UnsignedInteger64: return 8;
                case SingleFloat:
                case SingleFloatWithUnit: return 4;
                case DoubleFloat:
                case DoubleFloatWithUnit: return 8;
                case Boolean: return 1;
                case TimeStamp: return 16;
                default: throw new ArgumentException("Cannot determine size of data type " + dataType, "dataType");
            }
        }

        public static Type GetClrType(int dataType)
        {
            switch (dataType)
            {
                case Empty: return typeof(object);
                case Void: return typeof(object);
                case Integer8: return typeof(sbyte);
                case Integer16: return typeof(short);
                case Integer32: return typeof(int);
                case Integer64: return typeof(long);
                case UnsignedInteger8: return typeof(byte);
                case UnsignedInteger16: return typeof(ushort);
                case UnsignedInteger32: return typeof(uint);
                case UnsignedInteger64: return typeof(ulong);
                case SingleFloat:
                case SingleFloatWithUnit: return typeof(float);
                case DoubleFloat:
                case DoubleFloatWithUnit: return typeof(double);
                case Boolean: return typeof(bool);
                case String: return typeof(string);
                case TimeStamp: return typeof(DateTime);
                default: throw new Exception("Unknown data type " + dataType);
            }
        }

        public static int GetDataType(object value)
        {
            if (value == null) return Void;
            else if (value.GetType() == typeof(bool)) return Boolean;
            else if (value.GetType() == typeof(sbyte)) return Integer8;
            else if (value.GetType() == typeof(Int16)) return Integer16;
            else if (value.GetType() == typeof(Int32)) return Integer32;
            else if (value.GetType() == typeof(Int64)) return Integer64;
            else if (value.GetType() == typeof(byte)) return UnsignedInteger8;
            else if (value.GetType() == typeof(UInt16)) return UnsignedInteger16;
            else if (value.GetType() == typeof(UInt32)) return UnsignedInteger32;
            else if (value.GetType() == typeof(UInt64)) return UnsignedInteger64;
            else if (value.GetType() == typeof(float)) return SingleFloat;
            else if (value.GetType() == typeof(double)) return DoubleFloat;
            else if (value.GetType() == typeof(string)) return String;
            else if (value.GetType() == typeof(DateTime)) return TimeStamp;
            else throw new Exception("Unknown data type " + value.GetType());
        }
    }
}