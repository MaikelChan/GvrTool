using System.IO;

namespace GvrTool.ImageDataFormats
{
    class I8_ImageDataFormat : ImageDataFormat
    {
        public override uint DecodedDataLength => (uint)(Width * Height);
        public override uint EncodedDataLength => (uint)(Width * Height);
        
        public I8_ImageDataFormat(ushort width, ushort height) : base(width, height)
        {

        }

        public override byte[] Decode(Stream inputStream)
        {
            byte[] input = new byte[EncodedDataLength];
            inputStream.Read(input, 0, input.Length);

            return Decode(input);
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
                        for (int x2 = 0; x2 < 8; x2++)
                        {
                            output[(((y + y2) * Width) + (x + x2))] = input[offset];
                            offset++;
                        }
                    }
                }
            }

            return output;
        }

        public override byte[] Encode(Stream inputStream)
        {
            byte[] input = new byte[DecodedDataLength];
            inputStream.Read(input, 0, input.Length);

            return Encode(input);
        }

        public override byte[] Encode(byte[] input)
        {
            int offset = 0;
            byte[] output = new byte[EncodedDataLength];

            for (int y = 0; y < Height; y += 4)
            {
                for (int x = 0; x < Width; x += 8)
                {
                    for (int y2 = 0; y2 < 4; y2++)
                    {
                        for (int x2 = 0; x2 < 8; x2++)
                        {
                            output[offset] = input[((y + y2) * Width) + (x + x2)];
                            offset++;
                        }
                    }
                }
            }

            return output;
        }
    }
}