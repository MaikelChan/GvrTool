﻿using System;
using TGASharpLib;

namespace GvrTool.ImageDataFormats
{
    class I8_ImageDataFormat : ImageDataFormat
    {
        public override uint DecodedDataLength => (uint)(Width * Height);
        public override uint EncodedDataLength => (uint)(Width * Height);

        public override TgaPixelDepth TgaPixelDepth => TgaPixelDepth.Bpp8;
        public override TgaImageType TgaImageType => TgaImageType.Uncompressed_ColorMapped;
        public override byte TgaAlphaChannelBits => 0;

        public I8_ImageDataFormat(ushort width, ushort height) : base(width, height)
        {

        }

        public override byte[] Decode(byte[] input)
        {
            byte[] output = new byte[DecodedDataLength];
            int offset = 0;

            for (int y = 0; y < Height; y += 4)
            {
                for (int x = 0; x < Width; x += 8)
                {
                    for (int y2 = 0; y2 < 4; y2++)
                    {
                        Array.Copy(input, offset, output, ((y + y2) * Width) + x, 8);
                        offset += 8;
                    }
                }
            }

            return output;
        }

        public override byte[] Encode(byte[] input)
        {
            byte[] output = new byte[EncodedDataLength];
            int offset = 0;

            for (int y = 0; y < Height; y += 4)
            {
                for (int x = 0; x < Width; x += 8)
                {
                    for (int y2 = 0; y2 < 4; y2++)
                    {
                        Array.Copy(input, ((y + y2) * Width) + x, output, offset, 8);
                        offset += 8;
                    }
                }
            }

            return output;
        }
    }
}