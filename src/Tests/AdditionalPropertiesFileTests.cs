using System;
using NUnit.Framework;
using Should;

namespace Tests
{
    using File = NationalInstruments.Tdms.File;

    [TestFixture]
    public class AdditionalPropertiesFileTests 
    {
        protected File File;

        [SetUp]
        public void Setup()
        {
            File = new File(Constants.AdditionalPropertiesFile);
            File.Open();
        }

        [Test]
        public void Should_Have_Correct_Number_of_Properties()
        {
            File.Properties.Count.ShouldEqual(4);
        }

        [Test]
        public void Should_Have_Product_Id()
        {
            File.Properties["Product ID"].ShouldEqual("1335819");
        }

        [Test]
        public void Should_Have_Drive_Unit()
        {
            File.Properties["Drive Unit"].ShouldEqual("Midrange");
        }

        [Test]
        public void Should_Have_Shunt_Resistor_Value()
        {
            File.Properties["Rr"].ShouldEqual(0.446);
        }

        [Test]
        public void Should_Have_Timestamp()
        {
            var expectedDate = new DateTime(2013, 09, 04, 12, 25, 32, 621, DateTimeKind.Local);
            File.Properties["Timestamp"].ShouldEqual(expectedDate);
        }
    }
}