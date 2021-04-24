using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace ImageCompression
{
    struct BmpHeader
    {
        public ushort Signature; // 2 bytes
        public uint Size; // 4 bytes
        // 2 bytes reserved
        // 2 bytes reserved
        public uint Offset; // 4 bytes

        public BmpHeader(byte[] arr)
        {
            Signature = BitConverter.ToUInt16(arr, 0);
            Size = BitConverter.ToUInt32(arr, 2);
            Offset = BitConverter.ToUInt32(arr, 10);
        }
    }
    class RleCompressor //: ICompressor
    {
        public byte[] Compress(FileStream image)
        {
            // check format
            List<byte> result = new List<byte>();


            // cut headers
            byte BmpHeaderSize = 14;
            byte[] arrHeader = new byte[BmpHeaderSize];
            image.Read(arrHeader, 0, BmpHeaderSize);
            var header = new BmpHeader(arrHeader);
            image.Position = header.Offset;

            List<Color> difRow = new List<Color>();
            Color current = Color.FromArgb(0, 0, 0);
            sbyte row = -1;
            byte[] buffer = new byte[3];
            byte[] prev = new byte[3];
            while (image.Read(buffer, 0, buffer.Length) > 0)
            {
                if (buffer[0] == current.R && buffer[1] == current.G && buffer[2] == current.B && row != 127)
                    row++;
                else
                {
                    if (row < 0)
                    {
                        difRow.Add(Color.FromArgb(buffer[0], buffer[1], buffer[2]));
                        row--;
                    }
                    else if (row > 0)
                    {
                        result.Add((byte)row);
                        result.Add((byte)current.R);
                        result.Add((byte)current.G);
                        result.Add((byte)current.B);
                    }
                    current = Color.FromArgb(buffer[0], buffer[1], buffer[2]);
                    row = 1;
                }
                prev[0] = buffer[0];
                prev[1] = buffer[1];
                prev[2] = buffer[2];
            }

            result.Add((byte)row);
            result.Add(current.R);
            result.Add(current.G);
            result.Add(current.B);

            return result.ToArray();
        }

        public byte[] Decompress(FileStream image)
        {
            List<byte> result = new List<byte>();
            //byte[] header = { 66, 77, 54, 0, 48, 0, 0, 0, 0, 0, 54, 0, 0, 0, 40, 0,
            //                  0, 0, 0, 4, 0, 0, 0, 4, 0, 0, 1, 0, 24, 0, 0, 0,
            //                  0, 0, 0, 0, 48, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            //                  0, 0, 0, 0, 0, 0 };
            byte[] header = { 66, 77, 138, 0, 192, 0, 0, 0, 0, 0, 138, 0, 0, 0, 124, 0,
                              0, 0, 0, 8, 0, 0, 0, 8, 0, 0, 1, 0, 24, 0, 0, 0,
                              0, 0, 0, 0, 192, 0, 18, 11, 0, 0, 18, 11, 0, 0, 0, 0,
                              0, 0, 0, 0, 0, 0 };
            foreach (byte h in header)
                result.Add(h);

            byte[] buffer = new byte[4];
            while (image.Read(buffer, 0, buffer.Length) > 0)
            {
                for (int i = 0; i < buffer[0]; i++)
                {
                    result.Add(buffer[1]);
                    result.Add(buffer[2]);
                    result.Add(buffer[3]);
                }
            }
            return result.ToArray();
        }
    }
}
