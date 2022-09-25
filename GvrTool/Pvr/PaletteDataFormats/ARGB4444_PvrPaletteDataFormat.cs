using TGASharpLib;

namespace GvrTool.Pvr.PaletteDataFormats
{
    class ARGB4444_PvrPaletteDataFormat : PvrPaletteDataFormat
    {
        public override uint DecodedDataLength => (uint)(PaletteEntryCount * 4);
        public override uint EncodedDataLength => (uint)(PaletteEntryCount * 2);

        public override TgaColorMapEntrySize TgaColorMapEntrySize => TgaColorMapEntrySize.A8R8G8B8;

        public ARGB4444_PvrPaletteDataFormat(ushort paletteEntryCount) : base(paletteEntryCount)
        {

        }

        public override byte[] Decode(byte[] input)
        {
            byte[] output = new byte[DecodedDataLength];

            int offset = 0;

            for (int p = 0; p < output.Length; p += 4)
            {
                ushort entry = (ushort)((input[offset + 1] << 8) | input[offset + 0]);
                offset += 2;

                output[p + 0] = (byte)(((entry >> 00) & 0b0000_0000_0000_1111) * (255 / 15));
                output[p + 1] = (byte)(((entry >> 04) & 0b0000_0000_0000_1111) * (255 / 15));
                output[p + 2] = (byte)(((entry >> 08) & 0b0000_0000_0000_1111) * (255 / 15));
                output[p + 3] = (byte)(((entry >> 12) & 0b0000_0000_0000_1111) * (255 / 15));
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

                color |= (ushort)((input[offset + 0] >> 4) << 0);
                color |= (ushort)((input[offset + 1] >> 4) << 4);
                color |= (ushort)((input[offset + 2] >> 4) << 8);
                color |= (ushort)((input[offset + 3] >> 4) << 12);

                output[p + 0] = (byte)(color & 0b0000_0000_1111_1111);
                output[p + 1] = (byte)(color >> 8);

                offset += 4;
            }

            return output;
        }
    }
}