using System;
using System.IO;
using TGASharpLib;

namespace GvrTool.PaletteDataFormats
{
    abstract class PaletteDataFormat
    {
        public ushort PaletteEntryCount { get; set; }

        public abstract TgaColorMapEntrySize TgaColorMapEntrySize { get; }

        public abstract uint DecodedDataLength { get; }
        public abstract uint EncodedDataLength { get; }

        public byte[] Decode(Stream inputStream)
        {
            byte[] input = new byte[EncodedDataLength];
            inputStream.Read(input, 0, input.Length);

            return Decode(input);
        }

        public byte[] Encode(Stream inputStream)
        {
            byte[] input = new byte[DecodedDataLength];
            inputStream.Read(input, 0, input.Length);

            return Encode(input);
        }

        public abstract byte[] Decode(byte[] input);
        public abstract byte[] Encode(byte[] input);

        public PaletteDataFormat(ushort paletteEntryCount)
        {
            PaletteEntryCount = paletteEntryCount;
        }

        public static PaletteDataFormat Get(ushort paletteEntryCount, GvrPixelFormat format)
        {
            switch (format)
            {
                case GvrPixelFormat.Rgb5a3:
                    return new RGB5A3_PaletteDataFormat(paletteEntryCount);
                case GvrPixelFormat.Rgb565:
                    return new RGB565_PaletteDataFormat(paletteEntryCount);
                default:
                    throw new NotImplementedException($"Unsupported palette data format: {format}.");
            }
        }
    }
}