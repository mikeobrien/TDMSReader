using System.IO;

namespace Tests
{
    public class Constants
    {
        public const string SampleFile = "Sample.tdms";

        public static Stream CreateStream()
        {
            return new FileStream(SampleFile, FileMode.Open, FileAccess.Read);
        }
    }
}