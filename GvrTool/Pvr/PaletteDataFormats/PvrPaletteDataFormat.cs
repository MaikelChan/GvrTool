﻿using System;

namespace GvrTool.Pvr.PaletteDataFormats
{
    abstract class PvrPaletteDataFormat : PaletteDataFormat
    {
        public PvrPaletteDataFormat(ushort paletteEntryCount) : base(paletteEntryCount)
        {

        }

        public static PvrPaletteDataFormat Get(ushort paletteEntryCount, PvrPixelFormat format)
        {
            switch (format)
            {
                case PvrPixelFormat.Argb1555:
                    return new ARGB1555_PvrPaletteDataFormat(paletteEntryCount);
                case PvrPixelFormat.Argb4444:
                    return new ARGB4444_PvrPaletteDataFormat(paletteEntryCount);
                case PvrPixelFormat.Argb8888:
                    return new ARGB8888_PvrPaletteDataFormat(paletteEntryCount);
                case PvrPixelFormat.Rgb565:
                    return new RGB565_PvrPaletteDataFormat(paletteEntryCount);
                default:
                    throw new NotImplementedException($"Unsupported PVR palette data format: {format}.");
            }
        }
    }
}