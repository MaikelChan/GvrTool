using GvrTool.Pvr.ImageDataFormats;
using GvrTool.Pvr.PaletteDataFormats;
using System;
using System.IO;
using TGASharpLib;

namespace GvrTool.Pvr
{
    public class PVR
    {
        public ushort Width { get; private set; }
        public ushort Height { get; private set; }

        public uint GlobalIndex { get; private set; }
        public uint Padding1 { get; private set; }

        public PvrPixelFormat PixelFormat { get; private set; }
        public PvrDataFormat DataFormat { get; private set; }

        public PvrPixelFormat PalettePixelFormat { get; private set; }
        public ushort PaletteEntryCount { get; private set; }

        public ushort ExternalPaletteUnknown1 { get; private set; }
        public ushort ExternalPaletteUnknown2 { get; private set; }

        public byte[] Pixels { get; private set; }
        public byte[] Palette { get; private set; }

        // Helpers
        public bool HasExternalPalette =>
            DataFormat == PvrDataFormat.Index4 ||
            DataFormat == PvrDataFormat.Index4Mipmaps ||
            DataFormat == PvrDataFormat.Index8 ||
            DataFormat == PvrDataFormat.Index8Mipmaps;
        public bool HasInternalPalette =>
            DataFormat == PvrDataFormat.Vq ||
            DataFormat == PvrDataFormat.VqMipmaps ||
            DataFormat == PvrDataFormat.SmallVq ||
            DataFormat == PvrDataFormat.SmallVqMipmaps;
        public bool HasPalette => HasExternalPalette || HasInternalPalette;
        public bool HasMipmaps =>
            DataFormat == PvrDataFormat.SquareTwiddledMipmaps ||
            DataFormat == PvrDataFormat.VqMipmaps ||
            DataFormat == PvrDataFormat.Index4Mipmaps ||
            DataFormat == PvrDataFormat.Index8Mipmaps ||
            DataFormat == PvrDataFormat.SmallVqMipmaps ||
            DataFormat == PvrDataFormat.SquareTwiddledMipmapsAlt;

        bool isLoaded;

        const uint GBIX_MAGIC = 0x58494247;
        const uint PVRT_MAGIC = 0x54525650;
        const uint PVPL_MAGIC = 0x4c505650;

        const bool BIG_ENDIAN = false;

        public PVR()
        {
            isLoaded = false;
        }

        public void LoadFromPvrFile(string pvrPath)
        {
            if (string.IsNullOrWhiteSpace(pvrPath))
            {
                throw new ArgumentNullException(nameof(pvrPath));
            }

            if (!File.Exists(pvrPath))
            {
                throw new FileNotFoundException($"PVR file has not been found: {pvrPath}.");
            }

            using (FileStream fs = File.OpenRead(pvrPath))
            using (BinaryReader br = new BinaryReader(fs))
            {
                uint gbixMagic = br.ReadUInt32();
                if (gbixMagic != GBIX_MAGIC)
                {
                    throw new InvalidDataException($"\"{pvrPath}\" is not a valid GBIX/PVRT file.");
                }

                fs.Position = 0x10;

                uint pvrtMagic = br.ReadUInt32();
                if (pvrtMagic != PVRT_MAGIC)
                {
                    throw new InvalidDataException($"\"{pvrPath}\" is not a valid GBIX/PVRT file.");
                }

                fs.Position = 0x8;
                GlobalIndex = br.ReadUInt32Endian(BIG_ENDIAN);
                Padding1 = br.ReadUInt32Endian(BIG_ENDIAN);

                fs.Position = 0x18;
                PixelFormat = (PvrPixelFormat)br.ReadByte();
                DataFormat = (PvrDataFormat)br.ReadByte();
                ushort unknown = Width = br.ReadUInt16Endian(BIG_ENDIAN);
                Width = br.ReadUInt16Endian(BIG_ENDIAN);
                Height = br.ReadUInt16Endian(BIG_ENDIAN);

                if (HasMipmaps)
                {
                    throw new NotImplementedException($"Textures with mip maps are not supported.");
                }

                PvrImageDataFormat format = PvrImageDataFormat.Get(Width, Height, DataFormat, PixelFormat);
                Pixels = format.Decode(fs);
            }

            if (HasExternalPalette)
            {
                string pvpPath = Path.ChangeExtension(pvrPath, ".pvp");

                if (!File.Exists(pvpPath))
                {
                    throw new FileNotFoundException($"External PVP palette has not been found: {pvpPath}.");
                }

                using (FileStream fs = File.OpenRead(pvpPath))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    uint pvplMagic = br.ReadUInt32();
                    if (pvplMagic != PVPL_MAGIC)
                    {
                        throw new InvalidDataException($"\"{pvpPath}\" is not a valid PVPL file.");
                    }

                    fs.Position = 0x8;
                    PalettePixelFormat = (PvrPixelFormat)br.ReadUInt16Endian(BIG_ENDIAN);
                    ExternalPaletteUnknown1 = br.ReadUInt16Endian(BIG_ENDIAN);
                    ExternalPaletteUnknown2 = br.ReadUInt16Endian(BIG_ENDIAN);
                    PaletteEntryCount = br.ReadUInt16Endian(BIG_ENDIAN);

                    PvrPaletteDataFormat format = PvrPaletteDataFormat.Get(PaletteEntryCount, PalettePixelFormat);
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

            PVRMetadata metadata = PVRMetadata.LoadMetadataFromJson(Path.ChangeExtension(tgaFilePath, ".json"));

            GlobalIndex = metadata.GlobalIndex;
            Padding1 = metadata.Unknown1;
            PixelFormat = metadata.PixelFormat;
            DataFormat = metadata.DataFormat;
            PalettePixelFormat = metadata.PalettePixelFormat;
            PaletteEntryCount = metadata.PaletteEntryCount;
            ExternalPaletteUnknown1 = metadata.ExternalPaletteUnknown1;
            ExternalPaletteUnknown2 = metadata.ExternalPaletteUnknown2;

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
                throw new Exception($"PVR was not successfully initialized. Cannot proceed.");
            }

            if (string.IsNullOrWhiteSpace(tgaFilePath))
            {
                throw new ArgumentNullException(nameof(tgaFilePath));
            }

            PvrImageDataFormat imageFormat = PvrImageDataFormat.Get(Width, Height, DataFormat, PixelFormat);

            TGA tga = new TGA(Width, Height, imageFormat.TgaPixelDepth, imageFormat.TgaImageType);
            tga.Header.ImageSpec.ImageDescriptor.ImageOrigin = TgaImgOrigin.TopLeft;
            tga.Header.ImageSpec.ImageDescriptor.AlphaChannelBits = imageFormat.TgaAlphaChannelBits;
            tga.Header.ImageSpec.Y_Origin = Height;
            tga.ImageOrColorMapArea.ImageData = Pixels;

            if (HasPalette)
            {
                PvrPaletteDataFormat paletteFormat = PvrPaletteDataFormat.Get(PaletteEntryCount, PalettePixelFormat);

                tga.Header.ColorMapSpec.ColorMapEntrySize = paletteFormat.TgaColorMapEntrySize;
                tga.Header.ColorMapSpec.ColorMapLength = PaletteEntryCount;
                tga.ImageOrColorMapArea.ColorMapData = Palette;
            }

            tga.Save(tgaFilePath);

            PVRMetadata metadata = new PVRMetadata()
            {
                GlobalIndex = GlobalIndex,
                Unknown1 = Padding1,
                PixelFormat = PixelFormat,
                DataFormat = DataFormat,
                PalettePixelFormat = PalettePixelFormat,
                PaletteEntryCount = PaletteEntryCount,
                ExternalPaletteUnknown1 = ExternalPaletteUnknown1,
                ExternalPaletteUnknown2 = ExternalPaletteUnknown2
            };

            PVRMetadata.SaveMetadataToJson(metadata, Path.ChangeExtension(tgaFilePath, ".json"));
        }

        public void SaveToPvrFile(string pvrFilePath)
        {
            if (!isLoaded)
            {
                throw new Exception($"PVR was not successfully initialized. Cannot proceed.");
            }

            if (string.IsNullOrWhiteSpace(pvrFilePath))
            {
                throw new ArgumentNullException(nameof(pvrFilePath));
            }

            PvrImageDataFormat format = PvrImageDataFormat.Get(Width, Height, DataFormat, PixelFormat);

            using (FileStream fs = File.OpenWrite(pvrFilePath))
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(GBIX_MAGIC);
                bw.Write((uint)0x8); // GBIX data size
                bw.WriteEndian(GlobalIndex, BIG_ENDIAN);
                bw.WriteEndian(Padding1, BIG_ENDIAN);

                bw.Write(PVRT_MAGIC);
                bw.Write(format.EncodedDataLength + 8);
                bw.Write((byte)PixelFormat);
                bw.Write((byte)DataFormat);
                bw.WriteEndian((ushort)0x0, BIG_ENDIAN); //TODO: ???
                bw.WriteEndian(Width, BIG_ENDIAN);
                bw.WriteEndian(Height, BIG_ENDIAN);

                byte[] pvrtPixels = format.Encode(Pixels);
                fs.Write(pvrtPixels, 0, pvrtPixels.Length);
            }

            if (HasExternalPalette)
            {
                string pvpFilePath = Path.ChangeExtension(pvrFilePath, ".pvp");

                PvrPaletteDataFormat paletteFormat = PvrPaletteDataFormat.Get(PaletteEntryCount, PalettePixelFormat);

                using (FileStream fs = File.OpenWrite(pvpFilePath))
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(PVPL_MAGIC);
                    bw.Write(paletteFormat.EncodedDataLength + 8);
                    bw.Write((ushort)PalettePixelFormat);
                    bw.WriteEndian(ExternalPaletteUnknown1, BIG_ENDIAN);
                    bw.WriteEndian(ExternalPaletteUnknown2, BIG_ENDIAN);
                    bw.WriteEndian(PaletteEntryCount, BIG_ENDIAN);

                    byte[] palette = paletteFormat.Encode(Palette);
                    fs.Write(palette, 0, palette.Length);
                }
            }
        }
    }
}