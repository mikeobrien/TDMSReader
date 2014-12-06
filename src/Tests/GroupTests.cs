using NUnit.Framework;
using Should;
using NationalInstruments.Tdms;

namespace Tests
{
    [TestFixture]
    public class GroupTests
    {
        protected File File;

        [SetUp]
        public void Setup()
        {
            File = new File(Constants.SampleFile);
            File.Open();
        }

        [Test]
        public void Should_Contain_Group_Information()
        {
            File.Groups.Count.ShouldEqual(4);

            File.Groups.ContainsKey("EXAMPLE").ShouldBeTrue();
            var group = File.Groups["EXAMPLE"];
            group.Properties.Count.ShouldEqual(4);
            group.Properties["description"].ShouldEqual("DIAdem standard example channels");
            group.Properties["registertxt1"].ShouldEqual(string.Empty);
            group.Properties["registertxt2"].ShouldEqual(string.Empty);
            group.Properties["registertxt3"].ShouldEqual(string.Empty);

            File.Groups.ContainsKey("Noise data").ShouldBeTrue();
            group = File.Groups["Noise data"];
            group.Properties.Count.ShouldEqual(4);
            group.Properties["description"].ShouldEqual("Channel group with numeric channels, x/y-channels, waveform channels and text channels");
            group.Properties["registertxt1"].ShouldEqual(string.Empty);
            group.Properties["registertxt2"].ShouldEqual(string.Empty);
            group.Properties["registertxt3"].ShouldEqual(string.Empty);

            File.Groups.ContainsKey("Noise data results").ShouldBeTrue();
            group = File.Groups["Noise data results"];
            group.Properties.Count.ShouldEqual(4);
            group.Properties["description"].ShouldEqual(string.Empty);
            group.Properties["registertxt1"].ShouldEqual(string.Empty);
            group.Properties["registertxt2"].ShouldEqual(string.Empty);
            group.Properties["registertxt3"].ShouldEqual(string.Empty);

            File.Groups.ContainsKey("Room temperatures").ShouldBeTrue();
            group = File.Groups["Room temperatures"];
            group.Properties.Count.ShouldEqual(4);
            group.Properties["description"].ShouldEqual("Temperatures in three different areas");
            group.Properties["registertxt1"].ShouldEqual(string.Empty);
            group.Properties["registertxt2"].ShouldEqual(string.Empty);
            group.Properties["registertxt3"].ShouldEqual(string.Empty);
        }
    }
}