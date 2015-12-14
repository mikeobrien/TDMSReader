using System;
using System.Linq;
using NUnit.Framework;
using Should;
using NationalInstruments.Tdms;

namespace Tests
{
    [TestFixture]
    public class ChannelTests
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
        public void Should_Contain_Channel_Information()
        {
            var group = _file.Groups["Noise data"];

            group.Channels.Count.ShouldEqual(3);

            group.Channels.ContainsKey("Noise_1").ShouldBeTrue();
            var channel = group.Channels["Noise_1"];

            channel.HasData.ShouldBeTrue();

            channel.Properties.Count.ShouldEqual(29);

            channel.Properties["description"].ShouldEqual("Turn on car ignition");
            channel.Properties["minimum"].ShouldEqual(-0.588409873692028);
            channel.Properties["maximum"].ShouldEqual(0.632589685319464);
            channel.Properties["registerint5"].ShouldEqual(0);
            channel.Properties["registerint6"].ShouldEqual(0);
            channel.Properties["unit_string"].ShouldEqual("Pa");
            channel.Properties["datatype"].ShouldEqual("DT_DOUBLE");
            channel.Properties["registertxt3"].ShouldEqual(string.Empty);
            channel.Properties["registerint3"].ShouldEqual(0);
            channel.Properties["registerint4"].ShouldEqual(0);
            channel.Properties["registerint1"].ShouldEqual(0);
            channel.Properties["registerint2"].ShouldEqual(0);
            channel.Properties["registertxt1"].ShouldEqual(string.Empty);
            channel.Properties["registertxt2"].ShouldEqual(string.Empty);
            channel.Properties["displaytype"].ShouldEqual("Numeric");
            channel.Properties["monotony"].ShouldEqual("not monotone");
            channel.Properties["novaluekey"].ShouldEqual("No");
            channel.Properties["ResultStatArithMean"].ShouldEqual(-8.06729014402752E-06);
            channel.Properties["ResultStatMax"].ShouldEqual(0.632589685319464);
            channel.Properties["ResultStatMin"].ShouldEqual(-0.588409873692028);
            channel.Properties["ResultStatSqrMean"].ShouldEqual(0.0589834649704995);
            channel.Properties["ResultStatSum"].ShouldEqual(-2.62186929680894);
            channel.Properties["wf_increment"].ShouldEqual(2E-05);
            channel.Properties["wf_samples"].ShouldEqual(325000);
            channel.Properties["wf_start_offset"].ShouldEqual(0.0);
            channel.Properties["wf_start_time"].ShouldEqual(new DateTime(1903, 12, 31, 23, 0, 0, DateTimeKind.Utc).ToLocalTime());
            channel.Properties["wf_time_pref"].ShouldEqual("relative");
            channel.Properties["wf_xname"].ShouldEqual("Time");
            channel.Properties["wf_xunit_string"].ShouldEqual("s");
        }

        [Test]
        public void Should_Read_Raw_Data()
        {
            var channel = _file.Groups["Noise data"].Channels["Noise_1"];
            var data = channel.GetData<double>().ToList();
            data.Count().ShouldEqual(325000);
            data.Any(x => x == 0.0).ShouldBeFalse();
        }
    }
}