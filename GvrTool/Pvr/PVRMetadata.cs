using System.IO;
using System.Text.Json;

namespace GvrTool.Pvr
{
    class PVRMetadata
    {
        public uint MetadataVersion { get; set; }

        public uint GlobalIndex { get; set; }
        public uint Unknown1 { get; set; }

        public PvrPixelFormat PixelFormat { get; set; }
        public PvrDataFormat DataFormat { get; set; }

        public PvrPixelFormat PalettePixelFormat { get; set; }
        public ushort PaletteEntryCount { get; set; }

        public ushort ExternalPaletteUnknown1 { get; set; }
        public ushort ExternalPaletteUnknown2 { get; set; }

        const uint METADATA_VERSION = 1;

        public static void SaveMetadataToJson(PVRMetadata metadata, string jsonFilePath)
        {
            metadata.MetadataVersion = METADATA_VERSION;

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(metadata, options);
            File.WriteAllText(jsonFilePath, jsonString);
        }

        public static PVRMetadata LoadMetadataFromJson(string jsonFilePath)
        {
            string jsonString = File.ReadAllText(jsonFilePath);
            return JsonSerializer.Deserialize<PVRMetadata>(jsonString);
        }
    }
}