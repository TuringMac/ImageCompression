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
            string resultFilename = "result.bmp";

            byte[] sourceImage = File.ReadAllBytes(sourceFilename);
            byte[] compressedImage = new RleCompressor().Compress(sourceImage);
            File.WriteAllBytes(targetFilename, compressedImage);


            compressedImage = File.ReadAllBytes(targetFilename);
            byte[] decompressedImage = new RleCompressor().Decompress(compressedImage);
            File.WriteAllBytes(resultFilename, decompressedImage);

            Console.WriteLine("Compression is: " + (double)sourceImage.Length / compressedImage.Length);
            if (Compare(sourceFilename, "result.bmp") != 0)
                Console.WriteLine("Files not equals!");
            else
                Console.WriteLine("Correct!");

            Console.ReadKey();
            return 0;
        }

        static uint Compare(string sourceFilename, string resultFileName)
        {
            byte[] src = File.ReadAllBytes(sourceFilename);
            byte[] rst = File.ReadAllBytes("result.bmp");
            uint wrong = 0;
            for (int i = 0; i < src.Length || i < rst.Length; i++)
            {
                if (i >= src.Length || i >= rst.Length || src[i] != rst[i])
                {
                    wrong++;
                    if (wrong < 128)
                    {
                        int a = -1;
                        int b = -1;
                        if (i < src.Length)
                            a = src[i];
                        if (i < rst.Length)
                            b = rst[i];
                        Console.WriteLine($"{i}:\t{a}\t{b}");
                    }
                }
            }
            return wrong;
        }
    }
}
