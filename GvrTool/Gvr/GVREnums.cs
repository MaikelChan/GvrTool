﻿using System;

namespace GvrTool.Gvr
{
    // https://github.com/nickworonekin/puyotools

    // Gvr Pixel Formats
    public enum GvrPixelFormat : byte
    {
        IntensityA8 = 0x00,
        Rgb565 = 0x01,
        Rgb5a3 = 0x02,
        Unknown = 0xFF,
    }

    // Gvr Data Formats
    public enum GvrDataFormat : byte
    {
        Intensity4 = 0x00,
        Intensity8 = 0x01,
        IntensityA4 = 0x02,
        IntensityA8 = 0x03,
        Rgb565 = 0x04,
        Rgb5a3 = 0x05,
        Argb8888 = 0x06,
        Index4 = 0x08,
        Index8 = 0x09,
        Dxt1 = 0x0E,
        Unknown = 0xFF,
    }

    // Gvr Data Flags
    [Flags]
    public enum GvrDataFlags : byte
    {
        None = 0x0,
        Mipmaps = 0x1,
        ExternalPalette = 0x2,
        InternalPalette = 0x8,
        Palette = ExternalPalette | InternalPalette,
    }
}