using NUnit.Framework;
using TDMSReader;

namespace Tests
{
    [TestFixture]
    public abstract class ReaderTestsBase
    {
        protected Reader Reader;

        [SetUp]
        public void Setup()
        {
            Reader = new Reader(Constants.CreateStream());
        }

        [TearDown]
        public void TearDown()
        {
            Reader.Dispose();
        }
    }
}