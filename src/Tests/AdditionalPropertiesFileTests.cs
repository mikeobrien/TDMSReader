using System;
using NationalInstruments.Tdms;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class AdditionalPropertiesFileTests 
    {
        private File _file;

        [SetUp]
        public void Setup()
        {
            _file = new File(Constants.AdditionalPropertiesFile).Open();
        }

        [TearDown]
        public void TearDown()
        {
            _file.Dispose();
        }

        [Test]
        public void Should_Have_Correct_Number_of_Properties()
        {
            _file.Properties.Count.ShouldEqual(4);
        }

        [Test]
        public void Should_Have_Product_Id()
        {
            _file.Properties["Product ID"].ShouldEqual("1335819");
        }

        [Test]
        public void Should_Have_Drive_Unit()
        {
            _file.Properties["Drive Unit"].ShouldEqual("Midrange");
        }

        [Test]
        public void Should_Have_Shunt_Resistor_Value()
        {
            _file.Properties["Rr"].ShouldEqual(0.446);
        }

        [Test]
        public void Should_Have_Timestamp()
        {
            var expectedDate = new DateTime(2013, 09, 04, 11, 25, 32, 621, DateTimeKind.Utc).ToLocalTime();
            _file.Properties["Timestamp"].ShouldEqual(expectedDate);
        }
    }
}