using System.Linq;
using NUnit.Framework;
using NationalInstruments.Tdms;

namespace Tests
{
    [TestFixture]
    public class SampleUsage
    {
        [Test, Ignore]
        public void Create_Overview_Text_File()
        {
            using (var output = new System.IO.StreamWriter(System.IO.File.Create(@"D:\temp\tdms.overview.txt")))
            {
                var tdms = new File("Sample.tdms");
                tdms.Open();

                output.WriteLine("Properties:");
                foreach (var property in tdms.Properties)
                    output.WriteLine("    {0}: {1}", property.Key, property.Value);
                output.WriteLine();
                foreach (var group in tdms)
                {
                    output.WriteLine("    Group: {0}", group.Name);
                    foreach (var property in group.Properties)
                        output.WriteLine("        {0}: {1}", property.Key, property.Value);
                    output.WriteLine();
                    foreach (var channel in group)
                    {
                        output.WriteLine("        Channel: {0}", channel.Name);
                        foreach (var property in channel.Properties)
                            output.WriteLine("            {0}: {1}", property.Key, property.Value);
                        output.WriteLine();
                    }
                }

                output.WriteLine("Data:");
                foreach (var group in tdms)
                {
                    output.WriteLine("    Group: {0}", group.Name);
                    foreach (var channel in group)
                    {
                        output.WriteLine("        Channel: {0} ({1} data points of type {2})", channel.Name,
                                         channel.DataCount, channel.DataType);
                        foreach (var value in channel.GetData<object>().Take(20))
                            output.WriteLine("                {0}", value);
                        if (channel.DataCount > 20) output.WriteLine("                ...");
                        output.WriteLine();
                    }
                }
            }
        }

        [Test, Ignore]
        public void Create_Channel_Export_Text_File()
        {
            using (var output = new System.IO.StreamWriter(System.IO.File.Create(@"D:\temp\tdms.channel.export.txt")))
            {   
                var tdms = new File("Sample.tdms");
                tdms.Open();

                foreach (var value in tdms.Groups["Noise data"].Channels["Noise_1"].GetData<double>())
                    output.WriteLine(value);
            }
        }
    }
}