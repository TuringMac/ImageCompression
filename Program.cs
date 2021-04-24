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

            FileStream fsSource = new FileStream(sourceFilename, FileMode.Open);
            long sourceLen = fsSource.Length;
            byte[] compressedImage = new RleCompressor().Compress(fsSource);
            fsSource.Close();
            File.WriteAllBytes(targetFilename, compressedImage);


            FileStream fsTarget = new FileStream(targetFilename, FileMode.Open);
            byte[] decompressedImage = new RleCompressor().Decompress(fsTarget);
            fsTarget.Close();
            File.WriteAllBytes(resultFilename, decompressedImage);

            Console.WriteLine("Compression is: " + (double)sourceLen / compressedImage.Length);
            if (Compare(sourceFilename, "result.bmp") != 0)
                Console.WriteLine("Files not equals!");
            else
                Console.WriteLine("Correct!");

            Console.ReadKey();
            return 0;
        }

        static int Compare(string sourceFilename, string resultFileName)
        {
            byte[] src = File.ReadAllBytes(sourceFilename);
            byte[] rst = File.ReadAllBytes("result.bmp");
            int result = 0;
            for (int i = 0; i < src.Length || i < rst.Length; i++)
            {
                if (i >= src.Length || i >= rst.Length || src[i] != rst[i])
                {
                    result = 1;
                    int a = -1;
                    int b = -1;
                    if (i < src.Length)
                        a = src[i];
                    if (i < rst.Length)
                        b = rst[i];
                    Console.WriteLine($"{i}:\t{a}\t{b}");
                }
            }
            return result;
        }
    }
}
