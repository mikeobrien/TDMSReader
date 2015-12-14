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
    }
}
