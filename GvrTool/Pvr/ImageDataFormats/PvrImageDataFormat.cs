using System;

namespace GvrTool.Pvr.ImageDataFormats
{
    abstract class PvrImageDataFormat : ImageDataFormat
    {
        public PvrImageDataFormat(ushort width, ushort height) : base(width, height)
        {

        }

        public static PvrImageDataFormat Get(ushort width, ushort height, PvrDataFormat format)
        {
            switch (format)
            {
                case PvrDataFormat.Index8:
                    return new I8_PvrImageDataFormat(width, height);
                default:
                    throw new NotImplementedException($"Unsupported PVR image data format: {format}.");
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