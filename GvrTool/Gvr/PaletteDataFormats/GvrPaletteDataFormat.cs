using System;

namespace GvrTool.Gvr.PaletteDataFormats
{
    abstract class GvrPaletteDataFormat : PaletteDataFormat
    {
        public GvrPaletteDataFormat(ushort paletteEntryCount) : base(paletteEntryCount)
        {

        }

        public static GvrPaletteDataFormat Get(ushort paletteEntryCount, GvrPixelFormat format)
        {
            switch (format)
            {
                case GvrPixelFormat.Rgb5a3:
                    return new RGB5A3_GvrPaletteDataFormat(paletteEntryCount);
                case GvrPixelFormat.Rgb565:
                    return new RGB565_GvrPaletteDataFormat(paletteEntryCount);
                default:
                    throw new NotImplementedException($"Unsupported GVR palette data format: {format}.");
            }
        }
    }
}