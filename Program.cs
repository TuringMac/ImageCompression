using System;
using System.IO;
using System.Linq;

namespace ImageCompression
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Count() != 2)
            {
                Console.WriteLine("Wrong args count");
                return 100;
            }

            string sourceFilename = args[0];
            string targetFilename = args[1];

            FileStream fs = new FileStream(sourceFilename, FileMode.Open);
            byte[] compressedImage = new RleCompressor().Compress(fs);
            File.WriteAllBytes(targetFilename, compressedImage);

            return 0;
        }
    }
}
