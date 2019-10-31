using NUnit.Framework;
using System;
using System.IO;

namespace Tests
{
    public class Constants
    {
        private static readonly string TestDirectory = TestContext.CurrentContext.TestDirectory;
        public static readonly string SampleFile = Path.Combine(TestDirectory, @"Sample.tdms");
        public static readonly string PropertyRewrittenFile = Path.Combine(TestDirectory, @"PropertyRewritten.tdms");
        public static readonly string AdditionalPropertiesFile = Path.Combine(TestDirectory, @"AdditionalProperties.tdms");
        public static readonly string IncrementalMetaInformation = Path.Combine(TestDirectory, @"IncrementalMetaInformation.tdms");
        public static readonly string IncrementalMetaInformationInterleavedData = Path.Combine(TestDirectory, @"IncrementalMetaInformationInterleavedData.tdms");

        public static Stream CreateStream()
        {
            return new FileStream(SampleFile, FileMode.Open, FileAccess.Read);
        }
    }
}