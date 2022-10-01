using System;

namespace GvrTool.Pvr.ImageDataFormats
{
    abstract class PvrImageDataFormat : ImageDataFormat
    {
        public PvrImageDataFormat(ushort width, ushort height) : base(width, height)
        {

        }

        public static PvrImageDataFormat Get(ushort width, ushort height, PvrDataFormat dataFormat, PvrPixelFormat pixelFormat)
        {
            switch (dataFormat)
            {
                case PvrDataFormat.Index4:
                    return new I4_PvrImageDataFormat(width, height);
                case PvrDataFormat.Index8:
                    return new I8_PvrImageDataFormat(width, height);
                case PvrDataFormat.SquareTwiddled:
                    return new SquareTwiddled_PvrImageDataFormat(width, height, pixelFormat);
                default:
                    throw new NotImplementedException($"Unsupported PVR image data format: {dataFormat}.");
            }
        }

        /// <summary>
        /// Creates and returns the twiddle map for the specified size.
        /// </summary>
        protected static int[] CreateTwiddleMap(int size)
        {
            int[] twiddleMap = new int[size];

            for (int i = 0; i < size; i++)
            {
                twiddleMap[i] = 0;

                for (int j = 0, k = 1; k <= i; j++, k <<= 1)
                {
                    twiddleMap[i] |= (i & k) << j;
                }
            }

            return twiddleMap;
        }
    }
}