using System;
using System.Collections.Generic;
using System.Text;

namespace ImageCompression
{
    interface ICompressor
    {
        byte[] Compress(byte[] image);
        byte[] Decompress(byte[] image);
    }
}
