using System;

namespace GvrTool.Gvr.ImageDataFormats
{
    internal abstract class GvrImageDataFormat : ImageDataFormat
    {
        public GvrImageDataFormat(ushort width, ushort height) : base(width, height)
        {

        }

        public static GvrImageDataFormat Get(ushort width, ushort height, GvrDataFormat format)
        {
            switch (format)
            {
                case GvrDataFormat.Index4:
                    return new I4_GvrImageDataFormat(width, height);
                case GvrDataFormat.Index8:
                    return new I8_GvrImageDataFormat(width, height);
                case GvrDataFormat.Rgb5a3:
                    return new RGB5A3_GvrImageDataFormat(width, height);
                case GvrDataFormat.Argb8888:
                    return new ARGB8888_GvrImageDataFormat(width, height);
                case GvrDataFormat.Dxt1:
                    return new Dxt1_GvrImageDataFormat(width, height);
                default:
                    throw new NotImplementedException($"Unsupported GVR image data format: {format}.");
            }
        }
    }
}