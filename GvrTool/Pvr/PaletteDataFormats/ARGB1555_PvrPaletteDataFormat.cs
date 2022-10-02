using TGASharpLib;

namespace GvrTool.Pvr.PaletteDataFormats
{
    class ARGB1555_PvrPaletteDataFormat : PvrPaletteDataFormat
    {
        public override uint DecodedDataLength => (uint)(PaletteEntryCount * 2);
        public override uint EncodedDataLength => (uint)(PaletteEntryCount * 2);

        public override TgaColorMapEntrySize TgaColorMapEntrySize => TgaColorMapEntrySize.A1R5G5B5;

        public ARGB1555_PvrPaletteDataFormat(ushort paletteEntryCount) : base(paletteEntryCount)
        {

        }

        public override byte[] Decode(byte[] input)
        {
            byte[] output = new byte[DecodedDataLength];

            for (int p = 0; p < output.Length; p += 2)
            {
                output[p + 0] = input[p + 0];
                output[p + 1] = input[p + 1];
            }

            return output;
        }

        public override byte[] Encode(byte[] input)
        {
            byte[] output = new byte[EncodedDataLength];

            for (int p = 0; p < output.Length; p += 2)
            {
                output[p + 0] = input[p + 0];
                output[p + 1] = input[p + 1];
            }

            return output;
        }
    }
}