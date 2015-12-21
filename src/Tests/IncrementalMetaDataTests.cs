using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Should;
using NationalInstruments.Tdms;

namespace Tests
{
    [TestFixture]
    public class IncrementalMetaDataTests
    {
        private File _file;

        [SetUp]
        public void Setup()
        {
            _file = new File(Constants.IncrementalMetaInformation).Open();
        }

        [TearDown]
        public void TearDown()
        {
            _file.Dispose();
        }

        [Test]
        public void Should_Read_Incremetal_Raw_Data()
        {
            var rawData = _file.Groups.First().Value.Channels.Select(c => c.Value.GetData<int>().ToArray()).ToList();

            rawData[0].ShouldEqual(new[] { 1, 2, 3, 1, 2, 3 });
            rawData[1].ShouldEqual(new[] { 4, 5, 6, 4, 5, 6 });
        }

        [Test]
        public void Should_Read_Interleaved_File()
        {
            _file = new File(Constants.IncrementalMetaInformationInterleavedData).Open();
            var rawData = _file.Groups.First().Value.Channels.Select(c => c.Value.GetData<double>().ToArray()).ToList();

            rawData.Count.ShouldEqual(11);
            Assert.That(rawData[0], Is.EqualTo(new[] { 124300.04, 124301 }).Within(.0001));
            Assert.That(rawData[1], Is.EqualTo(new[] { 22.75, 23 }).Within(.0001));
            Assert.That(rawData[2], Is.EqualTo(new[] { 10.22481728, 10.2258358 }).Within(.0001));
            Assert.That(rawData[3], Is.EqualTo(new[] { 9.857121468, 9.852658272 }).Within(.0001));
            Assert.That(rawData[4], Is.EqualTo(new[] { 10.42528534, 10.42677307 }).Within(.0001));
            Assert.That(rawData[5], Is.EqualTo(new[] { 10.38357735, 10.38459492 }).Within(.0001));
            Assert.That(rawData[6], Is.EqualTo(new[] { 9.864326477, 9.865403175 }).Within(.0001));
            Assert.That(rawData[7], Is.EqualTo(new[] { 10.23942375, 10.24104881 }).Within(.0001));
            Assert.That(rawData[8], Is.EqualTo(new[] { 11.24080276, 11.23340607}).Within(.0001));
            Assert.That(rawData[9], Is.EqualTo(new[] { 11.35889435, 11.36006546 }).Within(.0001));
            Assert.That(rawData[10], Is.EqualTo(new[] { 11.25831127, 11.26730537 }).Within(.0001));
        }
    }
}
