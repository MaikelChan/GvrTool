using System;
using TGASharpLib;

namespace GvrTool.Pvr.ImageDataFormats
{
    class I8_PvrImageDataFormat : PvrImageDataFormat
    {
        public override uint BitsPerPixel => 8;

        public override uint DecodedDataLength => (uint)(Width * Height);
        public override uint EncodedDataLength => (uint)(Width * Height);

        public override TgaPixelDepth TgaPixelDepth => TgaPixelDepth.Bpp8;
        public override TgaImageType TgaImageType => TgaImageType.Uncompressed_ColorMapped;
        public override byte TgaAlphaChannelBits => 0;

        public I8_PvrImageDataFormat(ushort width, ushort height) : base(width, height)
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
                            sourceIndex = sourceBlockIndex + ((twiddleMap[x] << 1) | twiddleMap[y]);
                            destinationIndex = ((yStart + y) * Width) + xStart + x;

                            output[destinationIndex] = input[sourceIndex];
                        }
                    }

                    sourceBlockIndex += size * size;
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
                            destinationIndex = destinationBlockIndex + ((twiddleMap[x] << 1) | twiddleMap[y]);

                            output[destinationIndex] = input[sourceIndex];
                        }
                    }

                    destinationBlockIndex += size * size;
                }
            }

            return output;
        }
    }
}