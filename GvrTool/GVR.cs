using System;
using System.IO;
using TGASharpLib;

namespace GvrTool
{
    public class GVR
    {
        public ushort Width { get; private set; }
        public ushort Height { get; private set; }

        public uint GlobalIndex { get; private set; }

        public GvrPixelFormat PixelFormat { get; private set; }
        public GvrDataFlags DataFlags { get; private set; }
        public GvrDataFormat DataFormat { get; private set; }

        public GvrPixelFormat PalettePixelFormat { get; private set; }
        public ushort PaletteEntryCount { get; private set; }

        public byte[] Pixels { get; private set; }
        public byte[] Palette { get; private set; }

        // Helpers
        public bool HasExternalPalette => (DataFlags & GvrDataFlags.ExternalPalette) != 0;
        public bool HasInternalPalette => (DataFlags & GvrDataFlags.InternalPalette) != 0;
        public bool HasPalette => (DataFlags & GvrDataFlags.Palette) != 0;

        bool isLoaded;

        const uint GCIX_MAGIC = 0x58494347;
        const uint GVRT_MAGIC = 0x54525647;
        const uint GVPL_MAGIC = 0x4c505647;

        const bool BIG_ENDIAN = true;

        public GVR()
        {
            isLoaded = false;
        }

        public void LoadFromGvrFile(string gvrPath)
        {
            if (string.IsNullOrWhiteSpace(gvrPath))
            {
                throw new ArgumentNullException(nameof(gvrPath));
            }

            if (!File.Exists(gvrPath))
            {
                throw new FileNotFoundException($"GVR file has not been found: {gvrPath}.");
            }

            using (FileStream fs = File.OpenRead(gvrPath))
            using (BinaryReader br = new BinaryReader(fs))
            {
                uint gcixMagic = br.ReadUInt32();
                if (gcixMagic != GCIX_MAGIC)
                {
                    throw new InvalidDataException($"\"{gvrPath}\" is not a valid GCIX/GVRT file.");
                }

                fs.Position = 0x10;

                uint gvrtMagic = br.ReadUInt32();
                if (gvrtMagic != GVRT_MAGIC)
                {
                    throw new InvalidDataException($"\"{gvrPath}\" is not a valid GCIX/GVRT file.");
                }

                fs.Position = 0x8;
                GlobalIndex = br.ReadUInt32Endian(BIG_ENDIAN);

                fs.Position = 0x1A;
                byte pixelFormatAndFlags = br.ReadByte();
                PixelFormat = (GvrPixelFormat)(pixelFormatAndFlags >> 4);
                DataFlags = (GvrDataFlags)(pixelFormatAndFlags & 0x0F);
                DataFormat = (GvrDataFormat)br.ReadByte();
                Width = br.ReadUInt16Endian(BIG_ENDIAN);
                Height = br.ReadUInt16Endian(BIG_ENDIAN);

                if ((DataFlags & GvrDataFlags.Mipmaps) != 0)
                {
                    throw new NotImplementedException($"Textures with mip maps are not supported.");
                }

                if (!HasExternalPalette)
                {
                    throw new NotImplementedException($"Textures without external palette are not supported.");
                }

                switch (DataFormat)
                {
                    case GvrDataFormat.Index4:

                        Pixels = new byte[(Width * Height) >> 1];
                        fs.Read(Pixels, 0, Pixels.Length);
                        break;

                    default:

                        throw new NotImplementedException($"\"{gvrPath}\" has an unsupported data format: {DataFormat}.");
                }
            }

            if (HasExternalPalette)
            {
                string gvpPath = Path.ChangeExtension(gvrPath, ".gvp");

                if (!File.Exists(gvpPath))
                {
                    throw new FileNotFoundException($"External GVP palette has not been found: {gvpPath}.");
                }

                using (FileStream fs = File.OpenRead(gvpPath))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    uint gvplMagic = br.ReadUInt32();
                    if (gvplMagic != GVPL_MAGIC)
                    {
                        Console.WriteLine($"\"{gvpPath}\" is not a valid GVPL file.");
                        return;
                    }

                    fs.Position = 0x9;
                    PalettePixelFormat = (GvrPixelFormat)br.ReadByte();

                    fs.Position = 0xe;
                    PaletteEntryCount = br.ReadUInt16Endian(BIG_ENDIAN);

                    switch (PalettePixelFormat)
                    {
                        case GvrPixelFormat.Rgb5a3:
                        case GvrPixelFormat.Rgb565:

                            Palette = new byte[PaletteEntryCount << 1];
                            fs.Read(Palette, 0, Palette.Length);
                            break;

                        default:

                            Console.WriteLine($"\"{gvpPath}\" has an unsupported palette pixel format: {PalettePixelFormat}.");
                            return;
                    }
                }
            }

            isLoaded = true;
        }

        public void LoadFromTgaFile(string tgaFilePath)
        {
            if (string.IsNullOrWhiteSpace(tgaFilePath))
            {
                throw new ArgumentNullException(nameof(tgaFilePath));
            }

            if (!File.Exists(tgaFilePath))
            {
                throw new FileNotFoundException($"TGA file has not been found: {tgaFilePath}.");
            }

            GVRMetadata metadata = GVRMetadata.LoadMetadataFromJson(Path.ChangeExtension(tgaFilePath, ".json"));

            GlobalIndex = metadata.GlobalIndex;
            PixelFormat = metadata.PixelFormat;
            DataFlags = metadata.DataFlags;
            DataFormat = metadata.DataFormat;
            PalettePixelFormat = metadata.PalettePixelFormat;
            PaletteEntryCount = metadata.PaletteEntryCount;

            TGA tga = new TGA(tgaFilePath);

            Width = tga.Width;
            Height = tga.Height;

            switch (DataFormat)
            {
                case GvrDataFormat.Index4:

                    if (tga.Header.ImageSpec.PixelDepth != TgaPixelDepth.Bpp8)
                    {
                        throw new InvalidDataException($"TGA file PixelDepth is {tga.Header.ImageSpec.PixelDepth} but {TgaPixelDepth.Bpp8} is expected.");
                    }

                    int offset = 0;
                    byte[] tgaPixels = tga.ImageOrColorMapArea.ImageData;
                    Pixels = new byte[(Width * Height) >> 1];

                    for (int y = 0; y < Height; y += 8)
                    {
                        for (int x = 0; x < Width; x += 8)
                        {
                            for (int y2 = 0; y2 < 8; y2++)
                            {
                                for (int x2 = 0; x2 < 8; x2++)
                                {
                                    byte entry = (byte)(tgaPixels[((y + y2) * Width) + (x + x2)] & 0x0F);
                                    entry = (byte)((Pixels[offset] & (0x0F << (x2 & 0x01) * 4)) | (entry << ((~x2 & 0x01) * 4)));

                                    Pixels[offset] = entry;

                                    if ((x2 & 0x01) != 0) offset++;
                                }
                            }
                        }
                    }

                    break;

                default:

                    throw new NotImplementedException($"GVR has an unsupported data format: {DataFormat}.");
            }

            if (HasExternalPalette)
            {
                switch (PalettePixelFormat)
                {
                    case GvrPixelFormat.Rgb5a3:
                    {
                        if (tga.Header.ColorMapSpec.ColorMapEntrySize != TgaColorMapEntrySize.A8R8G8B8)
                        {
                            throw new InvalidDataException($"TGA file ColorMapEntrySize is {tga.Header.ColorMapSpec.ColorMapEntrySize} but {TgaColorMapEntrySize.A8R8G8B8} is expected.");
                        }

                        Palette = new byte[PaletteEntryCount * 2];
                        byte[] tgaPalette = tga.ImageOrColorMapArea.ColorMapData;
                        uint tgaPaletteOffset = 0;

                        for (int p = 0; p < Palette.Length; p += 2)
                        {
                            ushort color = 0x0;

                            if (tgaPalette[tgaPaletteOffset + 3] > 0xDA) // Rgb555
                            {
                                color |= 0x8000;
                                color |= (ushort)((tgaPalette[tgaPaletteOffset + 2] >> 3) << 10);
                                color |= (ushort)((tgaPalette[tgaPaletteOffset + 1] >> 3) << 5);
                                color |= (ushort)((tgaPalette[tgaPaletteOffset + 0] >> 3) << 0);
                            }
                            else // Argb3444
                            {
                                color |= (ushort)((tgaPalette[tgaPaletteOffset + 3] >> 5) << 12);
                                color |= (ushort)((tgaPalette[tgaPaletteOffset + 2] >> 4) << 8);
                                color |= (ushort)((tgaPalette[tgaPaletteOffset + 1] >> 4) << 4);
                                color |= (ushort)((tgaPalette[tgaPaletteOffset + 0] >> 4) << 0);
                            }

                            Palette[p + 0] = (byte)(color >> 8);
                            Palette[p + 1] = (byte)(color & 0b0000_0000_1111_1111);

                            tgaPaletteOffset += 4;
                        }

                        break;
                    }
                    case GvrPixelFormat.Rgb565:
                    {
                        if (tga.Header.ColorMapSpec.ColorMapEntrySize != TgaColorMapEntrySize.R8G8B8)
                        {
                            throw new InvalidDataException($"TGA file ColorMapEntrySize is {tga.Header.ColorMapSpec.ColorMapEntrySize} but {TgaColorMapEntrySize.R8G8B8} is expected.");
                        }

                        Palette = new byte[PaletteEntryCount * 2];
                        byte[] tgaPalette = tga.ImageOrColorMapArea.ColorMapData;
                        uint tgaPaletteOffset = 0;

                        for (int p = 0; p < Palette.Length; p += 2)
                        {
                            ushort color = 0x0;
                            color |= (ushort)((tgaPalette[tgaPaletteOffset + 2] >> 3) << 11);
                            color |= (ushort)((tgaPalette[tgaPaletteOffset + 1] >> 2) << 5);
                            color |= (ushort)((tgaPalette[tgaPaletteOffset + 0] >> 3) << 0);

                            Palette[p + 0] = (byte)(color >> 8);
                            Palette[p + 1] = (byte)(color & 0b0000_0000_1111_1111);

                            tgaPaletteOffset += 3;
                        }

                        break;
                    }
                    default:

                        throw new NotImplementedException($"GVP has an unsupported palette pixel format: {PalettePixelFormat}.");
                }
            }

            isLoaded = true;
        }

        public void SaveToTgaFile(string tgaFilePath)
        {
            if (!isLoaded)
            {
                throw new Exception($"GVR was not successfully initialized. Cannot proceed.");
            }

            if (string.IsNullOrWhiteSpace(tgaFilePath))
            {
                throw new ArgumentNullException(nameof(tgaFilePath));
            }

            TgaColorMapEntrySize tgaColorMapEntrySize = TgaColorMapEntrySize.Other;
            byte[] tgaPixels = null;
            byte[] tgaPalette = null;

            switch (DataFormat)
            {
                case GvrDataFormat.Index4:

                    int offset = 0;

                    tgaPixels = new byte[Width * Height];

                    for (int y = 0; y < Height; y += 8)
                    {
                        for (int x = 0; x < Width; x += 8)
                        {
                            for (int y2 = 0; y2 < 8; y2++)
                            {
                                for (int x2 = 0; x2 < 8; x2++)
                                {
                                    byte entry = (byte)((Pixels[offset] >> ((~x2 & 0x01) * 4)) & 0x0F);

                                    tgaPixels[(((y + y2) * Width) + (x + x2))] = entry;

                                    if ((x2 & 0x01) != 0) offset++;
                                }
                            }
                        }
                    }

                    break;

                default:

                    throw new NotImplementedException($"GVR has an unsupported data format: {DataFormat}.");
            }

            if (Palette != null)
            {
                int paletteOffset = 0;

                switch (PalettePixelFormat)
                {
                    case GvrPixelFormat.Rgb5a3:

                        tgaColorMapEntrySize = TgaColorMapEntrySize.A8R8G8B8;
                        tgaPalette = new byte[PaletteEntryCount * 4];

                        for (int p = 0; p < tgaPalette.Length; p += 4)
                        {
                            ushort entry = (ushort)((Palette[paletteOffset] << 8) | Palette[paletteOffset + 1]);
                            paletteOffset += 2;

                            if ((entry & 0b1000_0000_0000_0000) != 0) // Rgb555
                            {
                                tgaPalette[p + 3] = 255;
                                tgaPalette[p + 2] = (byte)(((entry >> 10) & 0b0000_0000_0001_1111) * (255 / 31));
                                tgaPalette[p + 1] = (byte)(((entry >> 05) & 0b0000_0000_0001_1111) * (255 / 31));
                                tgaPalette[p + 0] = (byte)(((entry >> 00) & 0b0000_0000_0001_1111) * (255 / 31));
                            }
                            else // Argb3444
                            {
                                tgaPalette[p + 3] = (byte)(((entry >> 12) & 0b0000_0000_0000_0111) * (255 / 7));
                                tgaPalette[p + 2] = (byte)(((entry >> 08) & 0b0000_0000_0000_1111) * (255 / 15));
                                tgaPalette[p + 1] = (byte)(((entry >> 04) & 0b0000_0000_0000_1111) * (255 / 15));
                                tgaPalette[p + 0] = (byte)(((entry >> 00) & 0b0000_0000_0000_1111) * (255 / 15));
                            }
                        }

                        break;

                    case GvrPixelFormat.Rgb565:

                        tgaColorMapEntrySize = TgaColorMapEntrySize.R8G8B8;
                        tgaPalette = new byte[PaletteEntryCount * 3];

                        for (int p = 0; p < tgaPalette.Length; p += 3)
                        {
                            ushort entry = (ushort)((Palette[paletteOffset] << 8) | Palette[paletteOffset + 1]);
                            paletteOffset += 2;

                            tgaPalette[p + 2] = (byte)(((entry >> 11) & 0b0000_0000_0001_1111) * (255 / 31));
                            tgaPalette[p + 1] = (byte)(((entry >> 05) & 0b0000_0000_0011_1111) * (255 / 63));
                            tgaPalette[p + 0] = (byte)(((entry >> 00) & 0b0000_0000_0001_1111) * (255 / 31));
                        }

                        break;

                    default:

                        throw new NotImplementedException($"GVP has an unsupported palette pixel format: {PalettePixelFormat}.");
                }
            }

            TGA tga = new TGA(Width, Height, TgaPixelDepth.Bpp8, TgaImageType.Uncompressed_ColorMapped);
            tga.Header.ImageSpec.ImageDescriptor.ImageOrigin = TgaImgOrigin.TopLeft;
            tga.Header.ImageSpec.Y_Origin = Height;
            tga.Header.ColorMapSpec.ColorMapEntrySize = tgaColorMapEntrySize;
            tga.Header.ColorMapSpec.ColorMapLength = PaletteEntryCount;
            tga.ImageOrColorMapArea.ImageData = tgaPixels;
            tga.ImageOrColorMapArea.ColorMapData = tgaPalette;

            tga.Save(tgaFilePath);

            GVRMetadata metadata = new GVRMetadata()
            {
                GlobalIndex = GlobalIndex,
                PixelFormat = PixelFormat,
                DataFlags = DataFlags,
                DataFormat = DataFormat,
                PalettePixelFormat = PalettePixelFormat,
                PaletteEntryCount = PaletteEntryCount
            };

            GVRMetadata.SaveMetadataToJson(metadata, Path.ChangeExtension(tgaFilePath, ".json"));
        }

        public void SaveToGvrFile(string gvrFilePath)
        {
            if (!isLoaded)
            {
                throw new Exception($"GVR was not successfully initialized. Cannot proceed.");
            }

            if (string.IsNullOrWhiteSpace(gvrFilePath))
            {
                throw new ArgumentNullException(nameof(gvrFilePath));
            }

            using (FileStream fs = File.OpenWrite(gvrFilePath))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(GCIX_MAGIC);
                bw.Write((uint)0x8); // Chunk content size
                bw.WriteEndian(GlobalIndex, BIG_ENDIAN);
                bw.WriteEndian((uint)0x3f, BIG_ENDIAN); // TODO: ???

                bw.Write(GVRT_MAGIC);
                bw.Write((uint)((Width * Height) >> 1) + 8); // Chunk content size
                bw.WriteEndian((ushort)0x0, BIG_ENDIAN); //TODO: ???
                bw.Write((byte)(((byte)PixelFormat << 4) | ((byte)DataFlags & 0xF)));
                bw.Write((byte)DataFormat);
                bw.WriteEndian(Width, BIG_ENDIAN);
                bw.WriteEndian(Height, BIG_ENDIAN);

                switch (DataFormat)
                {
                    case GvrDataFormat.Index4:

                        fs.Write(Pixels, 0, Pixels.Length);
                        break;

                    default:

                        throw new NotImplementedException($"GVR has an unsupported data format: {DataFormat}.");
                }
            }

            if (HasExternalPalette)
            {
                string gvpFilePath = Path.ChangeExtension(gvrFilePath, ".gvp");

                using (FileStream fs = File.OpenWrite(gvpFilePath))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(GVPL_MAGIC);
                    bw.Write((uint)((PaletteEntryCount << 1) + 0x8)); // Chunk content size
                    bw.Write((byte)0); // TODO: ???
                    bw.Write((byte)PalettePixelFormat);
                    bw.WriteEndian((ushort)0x00ff, BIG_ENDIAN); // TODO: ???
                    bw.WriteEndian((ushort)0x0000, BIG_ENDIAN); // TODO: ???
                    bw.WriteEndian(PaletteEntryCount, BIG_ENDIAN); // TODO: ???

                    switch (PalettePixelFormat)
                    {
                        case GvrPixelFormat.Rgb5a3:
                        case GvrPixelFormat.Rgb565:

                            fs.Write(Palette, 0, Palette.Length);
                            break;

                        default:

                            Console.WriteLine($"GVP has an unsupported palette pixel format: {PalettePixelFormat}.");
                            return;
                    }
                }
            }
        }
    }
}