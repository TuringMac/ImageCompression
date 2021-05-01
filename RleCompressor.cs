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
    enum Format
    {
        RGB,
        BGR
    }
    class RleCompressor //: ICompressor
    {
        public byte[] Compress(byte[] image)
        {
            uint position = 0;
            // check format
            List<byte> result = new List<byte>();


            // cut headers
            var header = new BmpHeader(image);
            position = header.Offset;

            Format fmt = Format.BGR;

            #region PRE actions

            List<Color> difRow = new List<Color>();
            Color prev = ReadColor(image, position, fmt);
            position += 3;
            Color current = ReadColor(image, position, fmt);
            position += 3;
            Color next = Color.FromArgb(2, 45, 78);

            sbyte row = 0;
            if (prev == current)
                row = 1;
            else
                row = -1;

            #endregion PRE actions

            #region MAIN loop

            for (; position < image.Length; position += 3)
            {
                next = ReadColor(image, position, fmt);

                if (current == prev) // AAx
                {
                    if (row >= 0)
                    {
                        row++;
                        if (row >= 127)// TODO check overflow
                        {
                            result.Add((byte)row);
                            result.Add(prev.R);
                            result.Add(prev.G);
                            result.Add(prev.B);
                            row = 0;
                        }
                    }
                    else
                    {
                        throw new Exception("Something went wrong");
                    }
                }
                else // ABx
                {
                    if (row >= 0)
                    {
                        row++;
                        result.Add((byte)row);
                        result.Add(prev.R);
                        result.Add(prev.G);
                        result.Add(prev.B);

                        if (current == next)
                            row = 0;
                        else
                            row = -1;
                    }
                    else
                    {
                        difRow.Add(prev);
                        if (current == next || difRow.Count >= 128) // Check overflow
                        {
                            SaveDif(result, difRow);
                            row = 0;
                        }
                    }
                }

                prev = current;
                current = next;
            }

            #endregion MAIN loop

            #region POST actions

            if (current == prev) // AAx
            {
                if (row >= 0)
                {
                    row++;
                    if (row >= 127)
                    {
                        result.Add((byte)row);
                        result.Add(prev.R);
                        result.Add(prev.G);
                        result.Add(prev.B);
                        row = 0;
                    }
                    row++;
                    result.Add((byte)row);
                    result.Add(prev.R);
                    result.Add(prev.G);
                    result.Add(prev.B);
                }
                else
                {
                    throw new Exception("Something went wrong");
                }
            }
            else // ABx
            {
                if (row >= 0)
                {
                    row++;
                    result.Add((byte)row);
                    result.Add(prev.R);
                    result.Add(prev.G);
                    result.Add(prev.B);
                }
                else
                {
                    difRow.Add(prev);
                    difRow.Add(current);
                    SaveDif(result, difRow);
                }
            }

            #endregion POST actions

            return result.ToArray();
        }

        void SaveDif(List<byte> result, List<Color> dif)
        {
            result.Add((byte)-dif.Count); // write length of different items run
            foreach (Color col in dif) // write different items run
            {
                result.Add(col.R);
                result.Add(col.G);
                result.Add(col.B);
            }
            dif.Clear();
        }

        Color ReadColor(byte[] image, uint position, Format fmt = Format.RGB)
        {
            switch (fmt)
            {
                case Format.RGB: return Color.FromArgb(image[position], image[position + 1], image[position + 2]);
                case Format.BGR: return Color.FromArgb(image[position + 2], image[position + 1], image[position]);
                default: throw new Exception("Wrong format");
            }
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
