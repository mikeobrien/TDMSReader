using NUnit.Framework;
using Should;
using NationalInstruments.Tdms;

namespace Tests
{
    [TestFixture]
    public class GroupTests
    {
        private File _file;

        [SetUp]
        public void Setup()
        {
            _file = new File(Constants.SampleFile).Open();
        }

        [TearDown]
        public void TearDown()
        {
            _file.Dispose();
        }

        [Test]
        public void Should_Contain_Group_Information()
        {
            _file.Groups.Count.ShouldEqual(4);

            _file.Groups.ContainsKey("EXAMPLE").ShouldBeTrue();
            var group = _file.Groups["EXAMPLE"];
            group.Properties.Count.ShouldEqual(4);
            group.Properties["description"].ShouldEqual("DIAdem standard example channels");
            group.Properties["registertxt1"].ShouldEqual(string.Empty);
            group.Properties["registertxt2"].ShouldEqual(string.Empty);
            group.Properties["registertxt3"].ShouldEqual(string.Empty);

            _file.Groups.ContainsKey("Noise data").ShouldBeTrue();
            group = _file.Groups["Noise data"];
            group.Properties.Count.ShouldEqual(4);
            group.Properties["description"].ShouldEqual("Channel group with numeric channels, x/y-channels, waveform channels and text channels");
            group.Properties["registertxt1"].ShouldEqual(string.Empty);
            group.Properties["registertxt2"].ShouldEqual(string.Empty);
            group.Properties["registertxt3"].ShouldEqual(string.Empty);

            _file.Groups.ContainsKey("Noise data results").ShouldBeTrue();
            group = _file.Groups["Noise data results"];
            group.Properties.Count.ShouldEqual(4);
            group.Properties["description"].ShouldEqual(string.Empty);
            group.Properties["registertxt1"].ShouldEqual(string.Empty);
            group.Properties["registertxt2"].ShouldEqual(string.Empty);
            group.Properties["registertxt3"].ShouldEqual(string.Empty);

            _file.Groups.ContainsKey("Room temperatures").ShouldBeTrue();
            group = _file.Groups["Room temperatures"];
            group.Properties.Count.ShouldEqual(4);
            group.Properties["description"].ShouldEqual("Temperatures in three different areas");
            group.Properties["registertxt1"].ShouldEqual(string.Empty);
            group.Properties["registertxt2"].ShouldEqual(string.Empty);
            group.Properties["registertxt3"].ShouldEqual(string.Empty);
        }
    }
}