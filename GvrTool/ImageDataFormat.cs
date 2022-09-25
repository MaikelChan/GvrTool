using System.IO;
using TGASharpLib;

namespace GvrTool
{
    abstract class ImageDataFormat
    {
        public ushort Width { get; set; }
        public ushort Height { get; set; }

        public abstract uint DecodedDataLength { get; }
        public abstract uint EncodedDataLength { get; }

        public abstract TgaPixelDepth TgaPixelDepth { get; }
        public abstract TgaImageType TgaImageType { get; }
        public abstract byte TgaAlphaChannelBits { get; }

        public ImageDataFormat(ushort width, ushort height)
        {
            Width = width;
            Height = height;
        }

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
    }
}