using Joveler.ZLibWrapper;
using System;
using System.Runtime.InteropServices;

namespace TLOU_PSARC_Tool.Core
{
    class Compression
    {
        internal enum OodleFormat : uint
        {
            LZH = 0,
            LZHLW = 1,
            LZNIB = 2,
            None = 3,
            LZB16 = 4,
            LZBLW = 5,
            LZA = 6,
            LZNA = 7,
            Kraken = 8,
            Mermaid = 9,
            BitKnit = 10,
            Selkie = 11,
            Hydra = 12,
            Leviathan = 13
        }

        internal enum OodleCompressionLevel : uint
        {
            None = 0,
            SuperFast = 1,
            VeryFast = 2,
            Fast = 3,
            Normal = 4,
            Optimal1 = 5,
            Optimal2 = 6,
            Optimal3 = 7,
            Optimal4 = 8,
            Optimal5 = 9
        }

        [DllImport("oo2core_9_win64.dll")]
        private static extern int OodleLZ_Decompress(byte[] Buffer, long BufferSize, byte[] OutputBuffer, long OutputBufferSize, uint a, uint b, uint c, uint d, uint e, uint f, uint g, uint h, uint i, int ThreadModule);
        [DllImport("oo2core_9_win64.dll")]
        private static extern int OodleLZ_Compress(OodleFormat Format, byte[] Buffer, long BufferSize, byte[] OutputBuffer, OodleCompressionLevel Level, uint a, uint b, uint c);

        public static byte[] OodleLZ_Decompress(byte[] data, int decompressedSize)
        {
            byte[] decompressedData = new byte[decompressedSize];
            var verificationSize = (uint)OodleLZ_Decompress(data, data.Length, decompressedData, decompressedData.Length, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3);

            if (verificationSize != decompressedSize)
            {
                throw new Exception("Decompression failed. Verification size does not match given size.");
            }

            return decompressedData;
        }

        public static byte[] OodleLZ_Compress(byte[] data, OodleFormat format = OodleFormat.Kraken, OodleCompressionLevel level = OodleCompressionLevel.Normal)
        {
            byte[] compressedData = new byte[data.Length];
            var verificationSize = OodleLZ_Compress(format, data, data.Length, compressedData, level, 0, 0, 0);

            if (verificationSize == 0)
            {
                throw new Exception("Compression failed. Verification size is 0.");
            }
            return compressedData;
        }

        public static byte[] ZilbDecompress(byte[] data)
        {
            return ZLibCompressor.Decompress(data);
        }

        public static byte[] ZilbCompress(byte[] data)
        {
            return ZLibCompressor.Compress(data);
        }


        public static byte[] Decompress(byte[] data, int decompressedSize, string CompressionFormat)
        {
            switch (CompressionFormat)
            {
                case "oodl":
                    return OodleLZ_Decompress(data, decompressedSize);
                case "zlib":
                    return ZilbDecompress(data);
                default:
                    throw new Exception("Unsupported Compression Format!");
            }
        }


    }

}