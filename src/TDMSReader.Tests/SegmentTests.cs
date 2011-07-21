using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Should;

namespace TDMSReader.Tests
{
    [TestFixture]
    public class SegmentTests
    {
        [Test]
        public void Segment_Count_Should_Be_Four()
        {
            var reader = new File(Constants.SampleFile);
            reader.Count().ShouldEqual(4);
        }
    }
}
