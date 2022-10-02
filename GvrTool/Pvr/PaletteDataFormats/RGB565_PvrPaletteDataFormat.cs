using System;
using TGASharpLib;

namespace GvrTool.Pvr.PaletteDataFormats
{
    class RGB565_PvrPaletteDataFormat : PvrPaletteDataFormat
    {
        public override uint DecodedDataLength => (uint)(PaletteEntryCount * 3);
        public override uint EncodedDataLength => (uint)(PaletteEntryCount * 2);

        public override TgaColorMapEntrySize TgaColorMapEntrySize => TgaColorMapEntrySize.R8G8B8;

        public RGB565_PvrPaletteDataFormat(ushort paletteEntryCount) : base(paletteEntryCount)
        {

        }

        public override byte[] Decode(byte[] input)
        {
            byte[] output = new byte[DecodedDataLength];

            for (int i = 0; i < PaletteEntryCount; i++)
            {
                int sourceIndex = i * 2;
                int destinationIndex = i * 3;

                ushort pixel = BitConverter.ToUInt16(input, sourceIndex);

                output[destinationIndex + 2] = (byte)(((pixel >> 11) & 0x1F) * 0xFF / 0x1F);
                output[destinationIndex + 1] = (byte)(((pixel >> 5) & 0x3F) * 0xFF / 0x3F);
                output[destinationIndex + 0] = (byte)(((pixel >> 0) & 0x1F) * 0xFF / 0x1F);
            }

            return output;
        }

        public override byte[] Encode(byte[] input)
        {
            byte[] output = new byte[EncodedDataLength];

            for (int i = 0; i < PaletteEntryCount; i++)
            {
                int sourceIndex = i * 3;
                int destinationIndex = i * 2;

                ushort pixel = 0x0000;
                pixel |= (ushort)((input[sourceIndex + 2] >> 3) << 11);
                pixel |= (ushort)((input[sourceIndex + 1] >> 2) << 5);
                pixel |= (ushort)((input[sourceIndex + 0] >> 3) << 0);

                output[destinationIndex + 1] = (byte)((pixel >> 8) & 0xFF);
                output[destinationIndex + 0] = (byte)(pixel & 0xFF);
            }

            return output;
        }
    }
}