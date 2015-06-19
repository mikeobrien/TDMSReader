using System.IO;
using NUnit.Framework;
using NationalInstruments.Tdms;

namespace Tests
{
    [TestFixture]
    public abstract class ReaderTestsBase
    {
        private Stream _stream;
        protected Reader Reader;

        [SetUp]
        public void Setup()
        {
            _stream = Constants.CreateStream();
            Reader = new Reader(_stream);
        }

        [TearDown]
        public void TearDown()
        {
            _stream.Dispose();
        }
    }
}