using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ImageCompression
{
    class RleCompressor //: ICompressor
    {
        public byte[] Compress(FileStream image)
        {
            // check format

            List<byte> result = new List<byte>();
            byte currentColor = 0;
            byte row = 0;

            // cut headers
            byte BmpHeaderSize = 14;
            byte[] header = new byte[BmpHeaderSize];
            image.Read(header, 0, BmpHeaderSize);

            bool newChar = true;
            byte[] buffer = new byte[1];
            while (image.Read(buffer, 0, 1) > 0)
            {
                if (newChar)
                {
                    currentColor = buffer[0];
                    row = 1;
                    newChar = false;
                    continue;
                }

                if (buffer[0] == currentColor)
                    row++;
                else
                {
                    result.Add(row);
                    result.Add(currentColor);
                    currentColor = buffer[0];
                    row = 0;
                    newChar = true;
                }

                if (row == 255)
                {
                    result.Add(row);
                    result.Add(currentColor);
                    newChar = true;
                }

            }



            return result.ToArray();
        }

        public byte[] Decompress(byte[] image)
        {
            throw new NotImplementedException();
        }
    }
}
