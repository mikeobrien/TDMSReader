using System.IO;

namespace Tests
{
    public class Constants
    {
        public const string SampleFile = "Sample.tdms";
        public const string PropertyRewrittenFile = "PropertyRewritten.tdms";
        public const string AdditionalPropertiesFile = "AdditionalProperties.tdms";
        public const string IncrementalMetaInformation = "IncrementalMetaInformation.tdms";
        public const string IncrementalMetaInformationInterleavedData = "IncrementalMetaInformationInterleavedData.tdms";

        public static Stream CreateStream()
        {
            return new FileStream(SampleFile, FileMode.Open, FileAccess.Read);
        }
    }
}