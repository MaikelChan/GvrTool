using GvrTool.Gvr.ImageDataFormats;
using GvrTool.Gvr.PaletteDataFormats;
using System;
using System.IO;
using TGASharpLib;

namespace GvrTool.Gvr
{
    public class GVR
    {
        public ushort Width { get; private set; }
        public ushort Height { get; private set; }

        public uint GlobalIndex { get; private set; }
        public uint Unknown1 { get; private set; }

        public GvrPixelFormat PixelFormat { get; private set; }
        public GvrDataFlags DataFlags { get; private set; }
        public GvrDataFormat DataFormat { get; private set; }

        public GvrPixelFormat PalettePixelFormat { get; private set; }
        public ushort PaletteEntryCount { get; private set; }

        public byte ExternalPaletteUnknown1 { get; private set; }
        public ushort ExternalPaletteUnknown2 { get; private set; }
        public ushort ExternalPaletteUnknown3 { get; private set; }

        public byte[] Pixels { get; private set; }
        public byte[] Palette { get; private set; }

        // Helpers
        public bool HasExternalPalette => (DataFlags & GvrDataFlags.ExternalPalette) != 0;
        public bool HasInternalPalette => (DataFlags & GvrDataFlags.InternalPalette) != 0;
        public bool HasPalette => (DataFlags & GvrDataFlags.Palette) != 0;
        public bool HasMipmaps => (DataFlags & GvrDataFlags.Mipmaps) != 0;

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
                Unknown1 = br.ReadUInt32Endian(BIG_ENDIAN);

                fs.Position = 0x1A;
                byte pixelFormatAndFlags = br.ReadByte();
                PixelFormat = (GvrPixelFormat)(pixelFormatAndFlags >> 4);
                DataFlags = (GvrDataFlags)(pixelFormatAndFlags & 0x0F);
                DataFormat = (GvrDataFormat)br.ReadByte();
                Width = br.ReadUInt16Endian(BIG_ENDIAN);
                Height = br.ReadUInt16Endian(BIG_ENDIAN);

                if (HasMipmaps)
                {
                    throw new NotImplementedException($"Textures with mip maps are not supported.");
                }

                GvrImageDataFormat format = GvrImageDataFormat.Get(Width, Height, DataFormat);
                Pixels = format.Decode(fs);
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
                        throw new InvalidDataException($"\"{gvpPath}\" is not a valid GVPL file.");
                    }

                    fs.Position = 0x8;
                    ExternalPaletteUnknown1 = br.ReadByte();
                    PalettePixelFormat = (GvrPixelFormat)br.ReadByte();
                    ExternalPaletteUnknown2 = br.ReadUInt16Endian(BIG_ENDIAN);
                    ExternalPaletteUnknown3 = br.ReadUInt16Endian(BIG_ENDIAN);
                    PaletteEntryCount = br.ReadUInt16Endian(BIG_ENDIAN);

                    GvrPaletteDataFormat format = GvrPaletteDataFormat.Get(PaletteEntryCount, PalettePixelFormat);
                    Palette = format.Decode(fs);
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
                throw new FileNotFoundException($"\"{tgaFilePath}\" TGA file has not been found.");
            }

            if (!Path.GetExtension(tgaFilePath).Equals(".tga", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException($"{tgaFilePath} is not a valid TGA file.");
            }

            GVRMetadata metadata = GVRMetadata.LoadMetadataFromJson(Path.ChangeExtension(tgaFilePath, ".json"));

            GlobalIndex = metadata.GlobalIndex;
            Unknown1 = metadata.Unknown1;
            PixelFormat = metadata.PixelFormat;
            DataFlags = metadata.DataFlags;
            DataFormat = metadata.DataFormat;
            PalettePixelFormat = metadata.PalettePixelFormat;
            PaletteEntryCount = metadata.PaletteEntryCount;
            ExternalPaletteUnknown1 = metadata.ExternalPaletteUnknown1;
            ExternalPaletteUnknown2 = metadata.ExternalPaletteUnknown2;
            ExternalPaletteUnknown3 = metadata.ExternalPaletteUnknown3;

            TGA tga = new TGA(tgaFilePath);

            Width = tga.Width;
            Height = tga.Height;

            switch (tga.Header.ImageSpec.ImageDescriptor.ImageOrigin)
            {
                case TgaImgOrigin.TopLeft:

                    Pixels = tga.ImageOrColorMapArea.ImageData;
                    break;

                case TgaImgOrigin.BottomLeft:

                    Pixels = Utils.FlipImageY(tga.ImageOrColorMapArea.ImageData, Width, Height, (byte)tga.Header.ImageSpec.PixelDepth >> 3);
                    break;

                default:

                    throw new NotImplementedException($"TGA file ImageOrigin mode not supported: {tga.Header.ImageSpec.ImageDescriptor.ImageOrigin}.");
            }

            //if (tga.Header.ImageSpec.PixelDepth != TgaPixelDepth.Bpp8)
            //{
            //    throw new InvalidDataException($"TGA file PixelDepth is {tga.Header.ImageSpec.PixelDepth} but {TgaPixelDepth.Bpp8} is expected.");
            //}

            if (HasExternalPalette)
            {
                //if (tga.Header.ColorMapSpec.ColorMapEntrySize != TgaColorMapEntrySize.A8R8G8B8)
                //{
                //    throw new InvalidDataException($"TGA file ColorMapEntrySize is {tga.Header.ColorMapSpec.ColorMapEntrySize} but {TgaColorMapEntrySize.A8R8G8B8} is expected.");
                //}

                //if (tga.Header.ColorMapSpec.ColorMapEntrySize != TgaColorMapEntrySize.R8G8B8)
                //{
                //    throw new InvalidDataException($"TGA file ColorMapEntrySize is {tga.Header.ColorMapSpec.ColorMapEntrySize} but {TgaColorMapEntrySize.R8G8B8} is expected.");
                //}

                Palette = tga.ImageOrColorMapArea.ColorMapData;
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

            GvrImageDataFormat imageFormat = GvrImageDataFormat.Get(Width, Height, DataFormat);

            TGA tga = new TGA(Width, Height, imageFormat.TgaPixelDepth, imageFormat.TgaImageType);
            tga.Header.ImageSpec.ImageDescriptor.ImageOrigin = TgaImgOrigin.TopLeft;
            tga.Header.ImageSpec.ImageDescriptor.AlphaChannelBits = imageFormat.TgaAlphaChannelBits;
            tga.Header.ImageSpec.Y_Origin = Height;
            tga.ImageOrColorMapArea.ImageData = Pixels;

            if (HasPalette)
            {
                GvrPaletteDataFormat paletteFormat = GvrPaletteDataFormat.Get(PaletteEntryCount, PalettePixelFormat);

                tga.Header.ColorMapSpec.ColorMapEntrySize = paletteFormat.TgaColorMapEntrySize;
                tga.Header.ColorMapSpec.ColorMapLength = PaletteEntryCount;
                tga.ImageOrColorMapArea.ColorMapData = Palette;
            }

            tga.Save(tgaFilePath);

            GVRMetadata metadata = new GVRMetadata()
            {
                GlobalIndex = GlobalIndex,
                Unknown1 = Unknown1,
                PixelFormat = PixelFormat,
                DataFlags = DataFlags,
                DataFormat = DataFormat,
                PalettePixelFormat = PalettePixelFormat,
                PaletteEntryCount = PaletteEntryCount,
                ExternalPaletteUnknown1 = ExternalPaletteUnknown1,
                ExternalPaletteUnknown2 = ExternalPaletteUnknown2,
                ExternalPaletteUnknown3 = ExternalPaletteUnknown3
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

            GvrImageDataFormat format = GvrImageDataFormat.Get(Width, Height, DataFormat);

            using (FileStream fs = File.OpenWrite(gvrFilePath))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(GCIX_MAGIC);
                bw.Write((uint)0x8); // GCIX data size
                bw.WriteEndian(GlobalIndex, BIG_ENDIAN);
                bw.WriteEndian(Unknown1, BIG_ENDIAN);

                bw.Write(GVRT_MAGIC);
                bw.Write(format.EncodedDataLength + 8);
                bw.WriteEndian((ushort)0x0, BIG_ENDIAN); //TODO: ???
                bw.Write((byte)(((byte)PixelFormat << 4) | ((byte)DataFlags & 0xF)));
                bw.Write((byte)DataFormat);
                bw.WriteEndian(Width, BIG_ENDIAN);
                bw.WriteEndian(Height, BIG_ENDIAN);

                byte[] gvrtPixels = format.Encode(Pixels);
                fs.Write(gvrtPixels, 0, gvrtPixels.Length);
            }

            if (HasExternalPalette)
            {
                string gvpFilePath = Path.ChangeExtension(gvrFilePath, ".gvp");

                GvrPaletteDataFormat paletteFormat = GvrPaletteDataFormat.Get(PaletteEntryCount, PalettePixelFormat);

                using (FileStream fs = File.OpenWrite(gvpFilePath))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(GVPL_MAGIC);
                    bw.Write(paletteFormat.EncodedDataLength + 8);
                    bw.Write(ExternalPaletteUnknown1);
                    bw.Write((byte)PalettePixelFormat);
                    bw.WriteEndian(ExternalPaletteUnknown2, BIG_ENDIAN);
                    bw.WriteEndian(ExternalPaletteUnknown3, BIG_ENDIAN);
                    bw.WriteEndian(PaletteEntryCount, BIG_ENDIAN);

                    byte[] palette = paletteFormat.Encode(Palette);
                    fs.Write(palette, 0, palette.Length);
                }
            }
        }
    }
}