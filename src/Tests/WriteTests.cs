using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using NationalInstruments.Tdms;

namespace Tests
{
    [TestFixture]
    public class WriteTests
    {
        [SetUp]
        public void Init()
        {
            //we'll set the cwd to the project, so that the Visual Studio Test framework can find the test files.
            Environment.CurrentDirectory = System.IO.Path.GetDirectoryName( this.GetType().Assembly.Location);
        }

        private bool IsEqual(IDictionary<string, object> dic1, IDictionary<string, object> dic2)
        {
            return (dic1 == dic2) || (dic1.Count == dic2.Count && !dic1.Except(dic2).Any());
        }

        private bool IsEqual(Group a, Group b)
        {
            if (a == b) return true;
            if (a.Name != b.Name) return false;
            if (!IsEqual(a.Properties, b.Properties)) return false;
            return IsEqual(a.Channels, b.Channels);
        }

        private bool IsEqual(Channel a, Channel b)
        {
            if (a == b) return true;
            if (!IsEqual(a.Properties, b.Properties)) return false;
            if (a.Name != b.Name) return false;
            if (a.HasData != b.HasData) return false;
            if (a.DataCount != b.DataCount) return false;
            if (a.DataType != b.DataType) return false;
            object[] arr1 = a.GetData<object>().ToArray();
            object[] arr2 = b.GetData<object>().ToArray();
            if (arr1 == null && arr2 == null) return true;
            if (arr1 == null || arr2 == null) return false;
            if (arr1.Length != arr2.Length) return false;
            for (int i = 0; i < arr1.Length; i++)
                if (!arr1[i].Equals(arr2[i])) return false;
            return true;
        }

        private bool IsEqual(IDictionary<string, Group> dic1, IDictionary<string, Group> dic2)
        {
            if(dic1 == dic2) return true;
            if (dic1.Count != dic2.Count) return false;
            KeyValuePair<string, Group>[] d1 = dic1.OrderBy(o => o.Key).ToArray();
            KeyValuePair<string, Group>[] d2 = dic2.OrderBy(o => o.Key).ToArray();
            for(int i=0;i<d1.Length;i++)
            {
                if (d1[i].Key != d2[i].Key) return false;
                if (!IsEqual(d1[i].Value, d2[i].Value)) return false;
            }
            return true;
        }

        private bool IsEqual(IDictionary<string, Channel> dic1, IDictionary<string, Channel> dic2)
        {
            if (dic1 == dic2) return true;
            if (dic1.Count != dic2.Count) return false;
            KeyValuePair<string, Channel>[] d1 = dic1.OrderBy(o => o.Key).ToArray();
            KeyValuePair<string, Channel>[] d2 = dic2.OrderBy(o => o.Key).ToArray();
            for (int i = 0; i < d1.Length; i++)
            {
                if (d1[i].Key != d2[i].Key) return false;
                if (!IsEqual(d1[i].Value, d2[i].Value)) return false;
            }
            return true;
        }

        private void TestSampleFile(string path)
        {
            var tdms = new File(path).Open();

            //write
            string tmp_file = System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(), "tdms");
            tdms.ReWrite(tmp_file);

            //read it back
            var tdms_rewritten = new File(tmp_file).Open();

            //compare
            Assert.IsTrue(IsEqual(tdms.Properties, tdms_rewritten.Properties));
            Assert.IsTrue(IsEqual(tdms.Groups, tdms_rewritten.Groups));
        }

        [Test]
        public void ReWriteFile()
        {
            TestSampleFile(Constants.SampleFile);
            TestSampleFile(Constants.PropertyRewrittenFile);
            TestSampleFile(Constants.AdditionalPropertiesFile);
            TestSampleFile(Constants.IncrementalMetaInformation);
            TestSampleFile(Constants.IncrementalMetaInformationInterleavedData);    //interleaved data is not really supported in re-write. If data is interleaved, it'll be converted to non-interleaved
        }

        [Test]
        public void Create1ChannelSinusFile()
        {
            //file
            string tmp_file = System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(), "tdms");

            //create base information
            WriteSegment f = new WriteSegment(tmp_file);
            f.Header.TableOfContents.HasRawData = true;
            f.MetaData.Add(WriteSegment.GenerateStandardRoot("See my Sine", "BigFoot48", "Test Dataset", DateTime.Now));
            f.MetaData.Add(WriteSegment.GenerateStandardGroup("Generated Harmonics", "Collection of test sines"));
            Reader.Metadata channel_meta = WriteSegment.GenerateStandardChannel("Generated Harmonics", "Sine 1", "Calculated with A * Math.Sin(t/(2*Math.PI))", "Bannanas", "s", "Time", DateTime.Now, 1/1000.0, typeof(double), 0);
            f.MetaData.Add(channel_meta);

            //open for raw
            f.Open();

            //puke up our sinus curve
            int dataCount = 0;
            double A = 100.0;
            double t = 0;
            double dT = 1 / 1000.0;
            System.IO.BinaryWriter w = new System.IO.BinaryWriter(f.BaseStream);
            for (int i = 0; i < 100000; i++)
            {
                w.Write(A * Math.Sin(t / (2*Math.PI)));
                t += dT;
                dataCount++;
            }

            //adjust actual data count. (Let's assume that we did't know this count before we started streaming.)
            channel_meta.RawData.Count = dataCount;

            //close up
            f.Close();

            //open it (with the reader)
            var tdms = new File(tmp_file).Open();

            //validate the sinus
            t = 0;
            foreach (double v in tdms.Groups["Generated Harmonics"].Channels["Sine 1"].GetData<double>())
            {
                double y = A * Math.Sin(t / (2 * Math.PI));
                Assert.IsTrue(y == v);
                t += dT;
            }
        }

        [Test]
        public void Create2ChannelInterleavedSinusFile()
        {
            //file
            string tmp_file = System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(), "tdms");

            //create base information
            WriteSegment f = new WriteSegment(tmp_file);
            f.Header.TableOfContents.HasRawData = true;
            f.Header.TableOfContents.RawDataIsInterleaved = true;
            f.MetaData.Add(WriteSegment.GenerateStandardRoot("See my Sine", "BigFoot48", "Test Dataset", DateTime.Now));
            f.MetaData.Add(WriteSegment.GenerateStandardGroup("Generated Harmonics", "Collection of test sines"));
            Reader.Metadata channel1_meta = WriteSegment.GenerateStandardChannel("Generated Harmonics", "Sine 1", "Calculated with A * Math.Sin(t/(2*Math.PI))", "Bannanas", "s", "Time", DateTime.Now, 1 / 1000.0, typeof(double), 0);
            f.MetaData.Add(channel1_meta);
            Reader.Metadata channel2_meta = WriteSegment.GenerateStandardChannel("Generated Harmonics", "Sine 2", "Calculated with A2 * Math.Sin(t/(2*Math.PI) + f)", "Bannanas", "s", "Time", DateTime.Now, 1 / 1000.0, typeof(double), 0);
            f.MetaData.Add(channel2_meta);

            //open for raw
            f.Open();

            //puke up our sinus curve
            int dataCount = 0;
            double A = 100.0;
            double A2 = 80.0;
            double t = 0;
            double dT = 1 / 1000.0;
            double ph = 0.1;
            System.IO.BinaryWriter w = new System.IO.BinaryWriter(f.BaseStream);
            for (int i = 0; i < 100000; i++)
            {
                w.Write(A * Math.Sin(t / (2 * Math.PI)));
                w.Write(A2 * Math.Sin(t / (2 * Math.PI) + ph));
                t += dT;
                dataCount++;
            }

            //adjust actual data count. (Let's assume that we did't know this count before we started streaming.)
            channel1_meta.RawData.Count = dataCount;
            channel2_meta.RawData.Count = dataCount;

            //close up
            f.Close();

            //open it (with the reader)
            var tdms = new File(tmp_file).Open();

            //validate the sinus
            t = 0;
            foreach (double v in tdms.Groups["Generated Harmonics"].Channels["Sine 1"].GetData<double>())
            {
                double y = A * Math.Sin(t / (2 * Math.PI));
                Assert.IsTrue(y == v);
                t += dT;
            }
            t = 0;
            foreach (double v in tdms.Groups["Generated Harmonics"].Channels["Sine 2"].GetData<double>())
            {
                double y = A2 * Math.Sin(t / (2 * Math.PI) + ph);
                Assert.IsTrue(y == v);
                t += dT;
            }
        }
    }
}