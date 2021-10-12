using System.IO;
using System.Text.Json;

namespace GvrTool
{
    class GVRMetadata
    {
        public uint MetadataVersion { get; set; }

        public uint GlobalIndex { get; set; }
        public uint Unknown1 { get; set; }

        public GvrPixelFormat PixelFormat { get; set; }
        public GvrDataFlags DataFlags { get; set; }
        public GvrDataFormat DataFormat { get; set; }

        public GvrPixelFormat PalettePixelFormat { get; set; }
        public ushort PaletteEntryCount { get; set; }

        const uint METADATA_VERSION = 1;

        public static void SaveMetadataToJson(GVRMetadata metadata, string jsonFilePath)
        {
            metadata.MetadataVersion = METADATA_VERSION;

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonString = JsonSerializer.Serialize(metadata, options);
            File.WriteAllText(jsonFilePath, jsonString);
        }

        public static GVRMetadata LoadMetadataFromJson(string jsonFilePath)
        {
            string jsonString = File.ReadAllText(jsonFilePath);
            return JsonSerializer.Deserialize<GVRMetadata>(jsonString);
        }
    }
}