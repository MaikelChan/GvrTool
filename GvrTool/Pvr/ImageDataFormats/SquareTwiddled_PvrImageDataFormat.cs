using System;
using TGASharpLib;

namespace GvrTool.Pvr.ImageDataFormats
{
    class SquareTwiddled_PvrImageDataFormat : PvrImageDataFormat
    {
        public override uint DecodedDataLength => (uint)(Width * Height * 4);
        public override uint EncodedDataLength => (uint)(Width * Height * bytesPerPixel);

        public override TgaPixelDepth TgaPixelDepth => TgaPixelDepth.Bpp32;
        public override TgaImageType TgaImageType => TgaImageType.Uncompressed_TrueColor;
        public override byte TgaAlphaChannelBits => 8;

        readonly PvrPixelFormat pixelFormat;
        readonly int bytesPerPixel;

        public SquareTwiddled_PvrImageDataFormat(ushort width, ushort height, PvrPixelFormat pixelFormat) : base(width, height)
        {
            this.pixelFormat = pixelFormat;

            switch (pixelFormat)
            {
                case PvrPixelFormat.Argb1555:
                    bytesPerPixel = 2;
                    break;
                default:
                    throw new NotImplementedException($"PixelFormat {pixelFormat} has not been implemented in {nameof(SquareTwiddled_PvrImageDataFormat)}.");
            }
        }

        public override byte[] Decode(byte[] input)
        {
            byte[] output = new byte[DecodedDataLength];

            int[] twiddleMap = CreateTwiddleMap(Width);

            int sourceIndex;
            int destinationIndex = 0;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    sourceIndex = ((twiddleMap[x] << 1) | twiddleMap[y]) * bytesPerPixel;

                    switch (pixelFormat)
                    {
                        case PvrPixelFormat.Argb1555:
                        {
                            ushort pixel = BitConverter.ToUInt16(input, sourceIndex);

                            output[destinationIndex + 3] = (byte)(((pixel >> 15) & 0x01) * 0xFF);
                            output[destinationIndex + 2] = (byte)(((pixel >> 10) & 0x1F) * 0xFF / 0x1F);
                            output[destinationIndex + 1] = (byte)(((pixel >> 5) & 0x1F) * 0xFF / 0x1F);
                            output[destinationIndex + 0] = (byte)(((pixel >> 0) & 0x1F) * 0xFF / 0x1F);

                            break;
                        }
                    }

                    destinationIndex += 4;
                }
            }

            return output;
        }

        public override byte[] Encode(byte[] input)
        {
            byte[] output = new byte[EncodedDataLength];

            int[] twiddleMap = CreateTwiddleMap(Width);

            int sourceIndex = 0;
            int destinationIndex;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    destinationIndex = ((twiddleMap[x] << 1) | twiddleMap[y]) * bytesPerPixel;

                    switch (pixelFormat)
                    {
                        case PvrPixelFormat.Argb1555:
                        {
                            ushort pixel = 0x0000;
                            pixel |= (ushort)((input[sourceIndex + 3] >> 7) << 15);
                            pixel |= (ushort)((input[sourceIndex + 2] >> 3) << 10);
                            pixel |= (ushort)((input[sourceIndex + 1] >> 3) << 5);
                            pixel |= (ushort)((input[sourceIndex + 0] >> 3) << 0);

                            output[destinationIndex + 1] = (byte)((pixel >> 8) & 0xFF);
                            output[destinationIndex + 0] = (byte)(pixel & 0xFF);

                            break;
                        }
                    }

                    sourceIndex += 4;
                }
            }

            return output;
        }
    }
}