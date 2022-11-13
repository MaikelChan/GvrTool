using System;
using TGASharpLib;

namespace GvrTool.Pvr.ImageDataFormats
{
    class I4_PvrImageDataFormat : PvrImageDataFormat
    {
        public override uint BitsPerPixel => 4;

        public override uint DecodedDataLength => (uint)(Width * Height);
        public override uint EncodedDataLength => (uint)((Width * Height) >> 1);

        public override TgaPixelDepth TgaPixelDepth => TgaPixelDepth.Bpp8;
        public override TgaImageType TgaImageType => TgaImageType.Uncompressed_ColorMapped;
        public override byte TgaAlphaChannelBits => 0;

        public I4_PvrImageDataFormat(ushort width, ushort height) : base(width, height)
        {

        }

        public override byte[] Decode(byte[] input)
        {
            byte[] output = new byte[DecodedDataLength];

            int size = Math.Min(Width, Height);
            int[] twiddleMap = CreateTwiddleMap(Width);

            int sourceIndex;
            int destinationIndex;
            int sourceBlockIndex = 0;

            for (int yStart = 0; yStart < Height; yStart += size)
            {
                for (int xStart = 0; xStart < Width; xStart += size)
                {
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            sourceIndex = sourceBlockIndex + (((twiddleMap[x] << 1) | twiddleMap[y]) / 2);
                            destinationIndex = ((yStart + y) * Width) + xStart + x;

                            byte paletteIndex = (byte)((input[sourceIndex] >> ((y & 0x1) * 4)) & 0xF);

                            output[destinationIndex + 0] = paletteIndex;
                        }
                    }

                    sourceBlockIndex += size * size / 2;
                }
            }

            return output;
        }

        public override byte[] Encode(byte[] input)
        {
            byte[] output = new byte[EncodedDataLength];

            int size = Math.Min(Width, Height);
            int[] twiddleMap = CreateTwiddleMap(Width);

            int sourceIndex;
            int destinationIndex;
            int destinationBlockIndex = 0;

            for (int yStart = 0; yStart < Height; yStart += size)
            {
                for (int xStart = 0; xStart < Width; xStart += size)
                {
                    for (int y = 0; y < size; y++)
                    {
                        for (int x = 0; x < size; x++)
                        {
                            sourceIndex = ((yStart + y) * Width) + xStart + x;
                            destinationIndex = destinationBlockIndex + (((twiddleMap[x] << 1) | twiddleMap[y]) / 2);

                            output[destinationIndex] |= (byte)((input[sourceIndex] & 0xF) << ((y & 0x1) * 4));
                        }
                    }

                    destinationBlockIndex += size * size / 2;
                }
            }

            return output;
        }
    }
}