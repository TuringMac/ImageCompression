using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace ImageCompression
{
    struct BmpHeader
    {
        // ushort 2 bytes
        // uint   4 bytes
        public ushort Signature; // 2 bytes
        public uint FileSize; // 4 bytes
        ushort Reserved1; // 2 bytes reserved
        ushort Reserved2; // 2 bytes reserved
        public uint OffsetBits; // 4 bytes
        uint HeaderSize;
        uint Width;
        uint Height;
        ushort Planes;
        ushort BitCount;
        uint Compression;
        uint SizeImage;
        uint XpelsPerMeter;
        uint YpelsPerMeter;
        uint ColorsUsed;
        uint ColorsImportant;

        public BmpHeader(byte[] arr)
        {
            Signature = BitConverter.ToUInt16(arr, 0);
            FileSize = BitConverter.ToUInt32(arr, 2);
            Reserved1 = BitConverter.ToUInt16(arr, 6);
            Reserved2 = BitConverter.ToUInt16(arr, 8);
            OffsetBits = BitConverter.ToUInt32(arr, 10);
            HeaderSize = BitConverter.ToUInt32(arr, 14);
            Width = BitConverter.ToUInt32(arr, 18);
            Height = BitConverter.ToUInt32(arr, 22);
            Planes = BitConverter.ToUInt16(arr, 26);
            BitCount = BitConverter.ToUInt16(arr, 28);
            Compression = BitConverter.ToUInt32(arr, 30);
            SizeImage = BitConverter.ToUInt32(arr, 34);
            XpelsPerMeter = BitConverter.ToUInt32(arr, 38);
            YpelsPerMeter = BitConverter.ToUInt32(arr, 42);
            ColorsUsed = BitConverter.ToUInt32(arr, 46);
            ColorsImportant = BitConverter.ToUInt32(arr, 50);
        }

        public byte[] ToArray()
        {
            var list = new List<byte>();

            list.Add((byte)Signature);
            list.Add((byte)(Signature >> 8));
            list.Add((byte)FileSize);
            list.Add((byte)(FileSize >> 8));
            list.Add((byte)(FileSize >> 16));
            list.Add((byte)(FileSize >> 24));
            list.Add((byte)Reserved1);
            list.Add((byte)(Reserved1 >> 8));
            list.Add((byte)Reserved2);
            list.Add((byte)(Reserved2 >> 8));
            list.Add((byte)OffsetBits);
            list.Add((byte)(OffsetBits >> 8));
            list.Add((byte)(OffsetBits >> 16));
            list.Add((byte)(OffsetBits >> 24));
            list.Add((byte)HeaderSize);
            list.Add((byte)(HeaderSize >> 8));
            list.Add((byte)(HeaderSize >> 16));
            list.Add((byte)(HeaderSize >> 24));
            list.Add((byte)Width);
            list.Add((byte)(Width >> 8));
            list.Add((byte)(Width >> 16));
            list.Add((byte)(Width >> 24));
            list.Add((byte)Height);
            list.Add((byte)(Height >> 8));
            list.Add((byte)(Height >> 16));
            list.Add((byte)(Height >> 24));
            list.Add((byte)Planes);
            list.Add((byte)(Planes >> 8));
            list.Add((byte)BitCount);
            list.Add((byte)(BitCount >> 8));
            list.Add((byte)Compression);
            list.Add((byte)(Compression >> 8));
            list.Add((byte)(Compression >> 16));
            list.Add((byte)(Compression >> 24));
            list.Add((byte)SizeImage);
            list.Add((byte)(SizeImage >> 8));
            list.Add((byte)(SizeImage >> 16));
            list.Add((byte)(SizeImage >> 24));
            list.Add((byte)XpelsPerMeter);
            list.Add((byte)(XpelsPerMeter >> 8));
            list.Add((byte)(XpelsPerMeter >> 16));
            list.Add((byte)(XpelsPerMeter >> 24));
            list.Add((byte)YpelsPerMeter);
            list.Add((byte)(YpelsPerMeter >> 8));
            list.Add((byte)(YpelsPerMeter >> 16));
            list.Add((byte)(YpelsPerMeter >> 24));
            list.Add((byte)ColorsUsed);
            list.Add((byte)(ColorsUsed >> 8));
            list.Add((byte)(ColorsUsed >> 16));
            list.Add((byte)(ColorsUsed >> 24));
            list.Add((byte)ColorsImportant);
            list.Add((byte)(ColorsImportant >> 8));
            list.Add((byte)(ColorsImportant >> 16));
            list.Add((byte)(ColorsImportant >> 24));

            return list.ToArray();
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
            foreach (byte b in header.ToArray())
            {
                result.Add(b);
            }
            position = header.OffsetBits;

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
                row = 0;
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

        public byte[] Decompress(byte[] image)
        {
            List<byte> result = new List<byte>();
            var header = new BmpHeader(image);
            foreach (byte b in header.ToArray())
                result.Add(b);
            uint position = header.OffsetBits;

            sbyte row = 0;
            while (position < image.Length)
            {
                row = (sbyte)image[position];
                position++;
                if (row > 0)
                {
                    Color col = ReadColor(image, position, Format.BGR);
                    position += 3;
                    while (row > 0)
                    {
                        result.Add(col.R);
                        result.Add(col.G);
                        result.Add(col.B);
                        row--;
                    }
                }
                else
                {
                    while (row < 0)
                    {
                        Color col = ReadColor(image, position, Format.BGR);
                        position += 3;
                        result.Add(col.R);
                        result.Add(col.G);
                        result.Add(col.B);
                        row++;
                    }
                }
            }
            return result.ToArray();
        }
    }
}
