using TGASharpLib;

namespace GvrTool.Pvr.PaletteDataFormats
{
    class ARGB8888_PvrPaletteDataFormat : PvrPaletteDataFormat
    {
        public override uint DecodedDataLength => (uint)(PaletteEntryCount * 4);
        public override uint EncodedDataLength => (uint)(PaletteEntryCount * 4);

        public override TgaColorMapEntrySize TgaColorMapEntrySize => TgaColorMapEntrySize.A8R8G8B8;

        public ARGB8888_PvrPaletteDataFormat(ushort paletteEntryCount) : base(paletteEntryCount)
        {

        }

        public override byte[] Decode(byte[] input)
        {
            byte[] output = new byte[DecodedDataLength];

            for (int p = 0; p < output.Length; p += 4)
            {
                output[p + 0] = input[p + 0];
                output[p + 1] = input[p + 1];
                output[p + 2] = input[p + 2];
                output[p + 3] = input[p + 3];
            }

            return output;
        }

        public override byte[] Encode(byte[] input)
        {
            byte[] output = new byte[EncodedDataLength];

            for (int p = 0; p < output.Length; p += 4)
            {
                output[p + 0] = input[p + 0];
                output[p + 1] = input[p + 1];
                output[p + 2] = input[p + 2];
                output[p + 3] = input[p + 3];
            }

            return output;
        }
    }
}