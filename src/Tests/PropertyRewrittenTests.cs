using NationalInstruments.Tdms;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class PropertyRewrittenTests
    {
        private File _file;

        [SetUp]
        public void Setup()
        {
            _file = new File(Constants.PropertyRewrittenFile).Open();
        }

        [Test]
        public void Open_FileWithSamePropertyWrittenSeveralTimes_LastWrittenValueShouldBeRead()
        {
            _file.Properties["Drive Unit"].ShouldEqual("Midrange");
        }
    }
}