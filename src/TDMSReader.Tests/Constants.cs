using System.IO;

namespace TDMSReader.Tests
{
    public class Constants
    {
        public const string SampleFile = "Data\\Sample.tdms";

        public static Stream CreateStream()
        {
            return new FileStream(SampleFile, FileMode.Open, FileAccess.Read);
        }
    }
}