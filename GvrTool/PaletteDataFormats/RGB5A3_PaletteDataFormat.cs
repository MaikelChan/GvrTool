﻿using TGASharpLib;

namespace GvrTool.PaletteDataFormats
{
    class RGB5A3_PaletteDataFormat : PaletteDataFormat
    {
        public override uint DecodedDataLength => (uint)(PaletteEntryCount * 4);
        public override uint EncodedDataLength => (uint)(PaletteEntryCount * 2);

        public override TgaColorMapEntrySize TgaColorMapEntrySize => TgaColorMapEntrySize.A8R8G8B8;
        public override byte TgaAlphaChannelBits => 8;

        public RGB5A3_PaletteDataFormat(ushort paletteEntryCount) : base(paletteEntryCount)
        {

        }

        public override byte[] Decode(byte[] input)
        {
            byte[] output = new byte[DecodedDataLength];
            int offset = 0;

            for (int p = 0; p < output.Length; p += 4)
            {
                ushort entry = (ushort)((input[offset] << 8) | input[offset + 1]);
                offset += 2;

                if ((entry & 0b1000_0000_0000_0000) != 0) // Rgb555
                {
                    output[p + 3] = 255;
                    output[p + 2] = (byte)(((entry >> 10) & 0b0000_0000_0001_1111) * (255 / 31));
                    output[p + 1] = (byte)(((entry >> 05) & 0b0000_0000_0001_1111) * (255 / 31));
                    output[p + 0] = (byte)(((entry >> 00) & 0b0000_0000_0001_1111) * (255 / 31));
                }
                else // Argb3444
                {
                    output[p + 3] = (byte)(((entry >> 12) & 0b0000_0000_0000_0111) * (255 / 7));
                    output[p + 2] = (byte)(((entry >> 08) & 0b0000_0000_0000_1111) * (255 / 15));
                    output[p + 1] = (byte)(((entry >> 04) & 0b0000_0000_0000_1111) * (255 / 15));
                    output[p + 0] = (byte)(((entry >> 00) & 0b0000_0000_0000_1111) * (255 / 15));
                }
            }

            return output;
        }

        public override byte[] Encode(byte[] input)
        {
            byte[] output = new byte[EncodedDataLength];
            int offset = 0;

            for (int p = 0; p < output.Length; p += 2)
            {
                ushort color = 0x0;

                if (input[offset + 3] > 0xDA) // Rgb555
                {
                    color |= 0x8000;
                    color |= (ushort)((input[offset + 2] >> 3) << 10);
                    color |= (ushort)((input[offset + 1] >> 3) << 5);
                    color |= (ushort)((input[offset + 0] >> 3) << 0);
                }
                else // Argb3444
                {
                    color |= (ushort)((input[offset + 3] >> 5) << 12);
                    color |= (ushort)((input[offset + 2] >> 4) << 8);
                    color |= (ushort)((input[offset + 1] >> 4) << 4);
                    color |= (ushort)((input[offset + 0] >> 4) << 0);
                }

                output[p + 0] = (byte)(color >> 8);
                output[p + 1] = (byte)(color & 0b0000_0000_1111_1111);

                offset += 4;
            }

            return output;
        }
    }
}