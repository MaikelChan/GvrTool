﻿using System;
using System.IO;

namespace GvrTool.ImageDataFormats
{
    abstract class ImageDataFormat
    {
        public ushort Width { get; set; }
        public ushort Height { get; set; }

        public abstract uint DecodedDataLength { get; }
        public abstract uint EncodedDataLength { get; }

        public byte[] Decode(Stream inputStream)
        {
            byte[] input = new byte[EncodedDataLength];
            inputStream.Read(input, 0, input.Length);

            return Decode(input);
        }

        public byte[] Encode(Stream inputStream)
        {
            byte[] input = new byte[DecodedDataLength];
            inputStream.Read(input, 0, input.Length);

            return Encode(input);
        }

        public abstract byte[] Decode(byte[] input);
        public abstract byte[] Encode(byte[] input);

        public ImageDataFormat(ushort width, ushort height)
        {
            Width = width;
            Height = height;
        }

        public static ImageDataFormat Get(ushort width, ushort height, GvrDataFormat format)
        {
            switch (format)
            {
                case GvrDataFormat.Index4:
                    return new I4_ImageDataFormat(width, height);
                case GvrDataFormat.Index8:
                    return new I8_ImageDataFormat(width, height);
                default:
                    throw new NotImplementedException($"Unsupported image data format: {format}.");
            }
        }
    }
}