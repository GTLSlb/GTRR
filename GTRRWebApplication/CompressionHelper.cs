using System.IO.Compression;

namespace GTRRWebApplication
{
    public static class CompressionHelper
    {
        public static byte[] GzipBytes(byte[] input)
        {
            if (input == null || input.Length == 0)
                return input;

            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, CompressionMode.Compress, leaveOpen: true))
                {
                    gzip.Write(input, 0, input.Length);
                }
                return output.ToArray();
            }
        }
    }
}
