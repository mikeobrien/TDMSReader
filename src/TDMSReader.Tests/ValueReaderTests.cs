using System;
using System.IO;
using NUnit.Framework;
using Should;

namespace TDMSReader.Tests
{
    [TestFixture]
    public class ValueReaderTests
    {
        [Test]
        public void Should_Read_Void_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 0 }));
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.Void);
            value.ShouldBeNull();
        }

        [Test]
        public void Should_Raise_Exception_For_Unsupported_Data_Type()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 0 }));
            var valueReader = new ValueReader(reader);
            Assert.Throws<Exception>(() => valueReader.Read(9999));
        }

        [Test]
        public void Should_Read_String_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 6, 0, 0, 0, 111, 104, 32, 104, 97, 105 }));
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.String);
            value.ShouldBeType<string>();
            ((string)value).ShouldEqual("oh hai");
        }

        [Test]
        public void Should_Read_Boolean_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] {1, 0}));
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.Boolean);
            value.ShouldBeType<bool>();
            ((bool)value).ShouldBeTrue();

            valueReader.Read<bool>(ValueReader.Boolean).ShouldBeFalse();
        }

        [Test]
        public void Should_Read_Signed_Byte_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 128, 192, 0, 63, 127 }));
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.Integer8);
            value.ShouldBeType<sbyte>();
            ((sbyte)value).ShouldEqual((sbyte)-128);

            valueReader.Read<sbyte>(ValueReader.Integer8).ShouldEqual((sbyte)-64);
            valueReader.Read<sbyte>(ValueReader.Integer8).ShouldEqual((sbyte)0);
            valueReader.Read<sbyte>(ValueReader.Integer8).ShouldEqual((sbyte)63);
            valueReader.Read<sbyte>(ValueReader.Integer8).ShouldEqual((sbyte)127);
        }

        [Test]
        public void Should_Read_Unsigned_Byte_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 0, 1, 62, 127, 255 }));
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.UnsignedInteger8);
            value.ShouldBeType<byte>();
            ((byte)value).ShouldEqual((byte)0);

            valueReader.Read<byte>(ValueReader.UnsignedInteger8).ShouldEqual((byte)1);
            valueReader.Read<byte>(ValueReader.UnsignedInteger8).ShouldEqual((byte)62);
            valueReader.Read<byte>(ValueReader.UnsignedInteger8).ShouldEqual((byte)127);
            valueReader.Read<byte>(ValueReader.UnsignedInteger8).ShouldEqual((byte)255);
        }

        [Test]
        public void Should_Read_Signed_Short_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 0, 128, 0, 192, 0, 0, 255, 63, 255, 127 }));
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.Integer16);
            value.ShouldBeType<short>();
            ((short)value).ShouldEqual((short)-32768);

            valueReader.Read<short>(ValueReader.Integer16).ShouldEqual((short)-16384);
            valueReader.Read<short>(ValueReader.Integer16).ShouldEqual((short)0);
            valueReader.Read<short>(ValueReader.Integer16).ShouldEqual((short)16383);
            valueReader.Read<short>(ValueReader.Integer16).ShouldEqual((short)32767);
        }

        [Test]
        public void Should_Read_Unsigned_Short_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 0, 0, 1, 0, 255, 63, 255, 127, 255, 255 }));
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.UnsignedInteger16);
            value.ShouldBeType<ushort>();
            ((ushort)value).ShouldEqual((ushort)0);

            valueReader.Read<ushort>(ValueReader.UnsignedInteger16).ShouldEqual((ushort)1);
            valueReader.Read<ushort>(ValueReader.UnsignedInteger16).ShouldEqual((ushort)16383);
            valueReader.Read<ushort>(ValueReader.UnsignedInteger16).ShouldEqual((ushort)32767);
            valueReader.Read<ushort>(ValueReader.UnsignedInteger16).ShouldEqual((ushort)65535);
        }

        [Test]
        public void Should_Read_Signed_Integer_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 0, 0, 0, 128, 0, 0, 0, 192, 0, 0, 0, 0, 
                                                                        255, 255, 255, 63, 255, 255, 255, 127 }));
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.Integer32);
            value.ShouldBeType<int>();
            ((int)value).ShouldEqual(-2147483648);

            valueReader.Read<int>(ValueReader.Integer32).ShouldEqual(-1073741824);
            valueReader.Read<int>(ValueReader.Integer32).ShouldEqual(0);
            valueReader.Read<int>(ValueReader.Integer32).ShouldEqual(1073741823);
            valueReader.Read<int>(ValueReader.Integer32).ShouldEqual(2147483647);
        }

        [Test]
        public void Should_Read_Unsigned_Integer_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 0, 0, 0, 0, 1, 0, 0, 0, 255, 255, 255, 63, 
                                                                        255, 255, 255, 127, 255, 255, 255, 255 }));
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.UnsignedInteger32);
            value.ShouldBeType<uint>();
            ((uint)value).ShouldEqual((uint)0);

            valueReader.Read<uint>(ValueReader.UnsignedInteger32).ShouldEqual((uint)1);
            valueReader.Read<uint>(ValueReader.UnsignedInteger32).ShouldEqual((uint)1073741823);
            valueReader.Read<uint>(ValueReader.UnsignedInteger32).ShouldEqual((uint)2147483647);
            valueReader.Read<uint>(ValueReader.UnsignedInteger32).ShouldEqual(4294967295);
        }

        [Test]
        public void Should_Read_Signed_Long_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 0, 0, 0, 0, 0, 0, 0, 128, 0, 0, 0, 0, 0,  
                                                                        0, 0, 192, 0, 0, 0, 0, 0, 0, 0, 0, 
                                                                        255, 255, 255, 255, 255, 255, 255, 63, 
                                                                        255, 255, 255, 255, 255, 255, 255, 127 }));
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.Integer64);
            value.ShouldBeType<long>();
            ((long)value).ShouldEqual(-9223372036854775808);

            valueReader.Read<long>(ValueReader.Integer64).ShouldEqual(-4611686018427387904);
            valueReader.Read<long>(ValueReader.Integer64).ShouldEqual(0);
            valueReader.Read<long>(ValueReader.Integer64).ShouldEqual(4611686018427387903);
            valueReader.Read<long>(ValueReader.Integer64).ShouldEqual(9223372036854775807);
        }

        [Test]
        public void Should_Read_Unsigned_Long_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0,  
                                                                        0, 0, 0, 255, 255, 255, 255, 0, 0, 0, 0, 
                                                                        255, 255, 255, 255, 255, 255, 255, 127,  
                                                                        255, 255, 255, 255, 255, 255, 255, 255 }));
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.UnsignedInteger64);
            value.ShouldBeType<ulong>();
            ((ulong)value).ShouldEqual((ulong)0);

            valueReader.Read<ulong>(ValueReader.UnsignedInteger64).ShouldEqual((ulong)1);
            valueReader.Read<ulong>(ValueReader.UnsignedInteger64).ShouldEqual(4294967295);
            valueReader.Read<ulong>(ValueReader.UnsignedInteger64).ShouldEqual((ulong)9223372036854775807);
            valueReader.Read<ulong>(ValueReader.UnsignedInteger64).ShouldEqual(18446744073709551615);
        }

        [Test]
        public void Should_Read_Timestamp_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,  
                                                                        0, 0, 0, 0, 0, 0, 0, 0, 161, 147, 111, 196, 0, 0, 0, 0, 
                                                                        0, 0, 0, 0, 0, 0, 0, 0, 97, 165, 147, 237, 255, 255, 255, 255}));
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.TimeStamp);
            value.ShouldBeType<DateTime>();
            ((DateTime)value).ShouldEqual(DateTime.Parse("1904-01-01 00:00:00"));

            valueReader.Read<DateTime>(ValueReader.TimeStamp).ShouldEqual(DateTime.Parse("2008-06-07 01:23:45"));
            valueReader.Read<DateTime>(ValueReader.TimeStamp).ShouldEqual(DateTime.Parse("1894-03-15 13:23:45"));
        }

        [Test]
        public void Should_Read_Float_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 174, 71, 1, 192, 174, 71, 129, 191, 0, 0, 0, 0,  
                                                                        174, 71, 129, 63, 174, 71, 1, 64 }));
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.SingleFloat);
            value.ShouldBeType<float>();
            ((float)value).ShouldEqual(-2.02f);

            valueReader.Read<float>(ValueReader.SingleFloat).ShouldEqual(-1.01f);
            valueReader.Read<float>(ValueReader.SingleFloat).ShouldEqual(0.00f);
            valueReader.Read<float>(ValueReader.SingleFloat).ShouldEqual(1.01f);
            valueReader.Read<float>(ValueReader.SingleFloat).ShouldEqual(2.02f);
        }

        [Test]
        public void Should_Read_Float_With_Unit_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 174, 71, 1, 192, 174, 71, 129, 191, 0, 0, 0, 0,  
                                                                        174, 71, 129, 63, 174, 71, 1, 64 }));
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.SingleFloatWithUnit);
            value.ShouldBeType<float>();
            ((float)value).ShouldEqual(-2.02f);

            valueReader.Read<float>(ValueReader.SingleFloatWithUnit).ShouldEqual(-1.01f);
            valueReader.Read<float>(ValueReader.SingleFloatWithUnit).ShouldEqual(0.00f);
            valueReader.Read<float>(ValueReader.SingleFloatWithUnit).ShouldEqual(1.01f);
            valueReader.Read<float>(ValueReader.SingleFloatWithUnit).ShouldEqual(2.02f);
        }

        [Test]
        public void Should_Read_Double_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 41, 92, 143, 194, 245, 40, 0, 192, 41, 92, 143, 194, 
                                                                        245, 40, 240, 191, 0, 0, 0, 0, 0, 0, 0, 0, 41, 92,
                                                                        143, 194, 245, 40, 240, 63, 41, 92, 143, 194, 245, 
                                                                        40, 0, 64 })); 
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.DoubleFloat);
            value.ShouldBeType<double>();
            ((double)value).ShouldEqual(-2.02);

            valueReader.Read<double>(ValueReader.DoubleFloat).ShouldEqual(-1.01);
            valueReader.Read<double>(ValueReader.DoubleFloat).ShouldEqual(0);
            valueReader.Read<double>(ValueReader.DoubleFloat).ShouldEqual(1.01);
            valueReader.Read<double>(ValueReader.DoubleFloat).ShouldEqual(2.02);
        }

        [Test]
        public void Should_Read_Double_With_Unit_Value()
        {
            var reader = new BinaryReader(new MemoryStream(new byte[] { 41, 92, 143, 194, 245, 40, 0, 192, 41, 92, 143, 194, 
                                                                        245, 40, 240, 191, 0, 0, 0, 0, 0, 0, 0, 0, 41, 92,
                                                                        143, 194, 245, 40, 240, 63, 41, 92, 143, 194, 245, 
                                                                        40, 0, 64 }));
            var valueReader = new ValueReader(reader);
            var value = valueReader.Read(ValueReader.DoubleFloatWithUnit);
            value.ShouldBeType<double>();
            ((double)value).ShouldEqual(-2.02);

            valueReader.Read<double>(ValueReader.DoubleFloatWithUnit).ShouldEqual(-1.01);
            valueReader.Read<double>(ValueReader.DoubleFloatWithUnit).ShouldEqual(0);
            valueReader.Read<double>(ValueReader.DoubleFloatWithUnit).ShouldEqual(1.01);
            valueReader.Read<double>(ValueReader.DoubleFloatWithUnit).ShouldEqual(2.02);
        }
    }
}
