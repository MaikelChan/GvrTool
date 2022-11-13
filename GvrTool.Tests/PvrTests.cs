using GvrTool.Pvr;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Security.Cryptography;

namespace GvrTool.Tests
{
    [TestClass]
    public class PvrTests
    {
        const string TEST_FILES_DIRECTORY = @"..\..\..\..\..\PVRs";
        const double THRESHOLD = 2d;

        [DataTestMethod]
        [DataRow("0000.pvr")] // ImageData: I8, PaletteData: ARGB8888               - Resident Evil: Code Veronica (Dreamcast)
        [DataRow("0001.pvr")] // ImageData: I8, PaletteData: ARGB4444               - Resident Evil: Code Veronica (Dreamcast)
        [DataRow("0002.pvr")] // ImageData: SquareTwiddled, PixelFormat: ARGB1555   - Resident Evil: Code Veronica (Dreamcast)
        [DataRow("0003.pvr")] // ImageData: I4, PaletteData: ARGB8888               - Resident Evil: Code Veronica (Dreamcast)
        [DataRow("0004.pvr")] // ImageData: I8 Mipmaps, PaletteData: ARGB8888       - Resident Evil: Code Veronica (Dreamcast)

        public void RegenerateAndCheckIfEqual(string testFileName)
        {
            using (MD5 md5 = MD5.Create())
            {
                string pvrFilePath1 = Path.Combine(TEST_FILES_DIRECTORY, testFileName);
                string pvrFilePath2 = Path.ChangeExtension(pvrFilePath1, null) + "_2" + Path.GetExtension(pvrFilePath1);

                string pvpFilePath1 = Path.ChangeExtension(pvrFilePath1, ".pvp");
                string pvpFilePath2 = Path.ChangeExtension(pvpFilePath1, null) + "_2" + Path.GetExtension(pvpFilePath1);

                string jsonFilePath = Path.ChangeExtension(pvrFilePath1, ".json");
                string tgaFilePath = Path.ChangeExtension(pvrFilePath1, ".tga");

                PVR pvr1 = new PVR();
                pvr1.LoadFromPvrFile(pvrFilePath1);
                pvr1.SaveToTgaFile(tgaFilePath);

                PVR pvr2 = new PVR();
                pvr2.LoadFromTgaFile(tgaFilePath);
                pvr2.SaveToPvrFile(pvrFilePath2);

                if (pvr1.HasMipmaps)
                {
                    byte[] pvrHeaderHash1, pvrDataHash1;
                    byte[] pvrHeaderHash2, pvrDataHash2;

                    using (FileStream fs = File.OpenRead(pvrFilePath1))
                    {
                        byte[] header = new byte[0x20];
                        fs.Read(header, 0, header.Length);

                        byte[] data = new byte[fs.Length - (0x20 + pvr1.MainTextureOffset)];
                        fs.Position += pvr1.MainTextureOffset;
                        fs.Read(data, 0, data.Length);

                        pvrHeaderHash1 = md5.ComputeHash(header);
                        pvrDataHash1 = md5.ComputeHash(data);
                    }

                    using (FileStream fs = File.OpenRead(pvrFilePath2))
                    {
                        byte[] header = new byte[0x20];
                        fs.Read(header, 0, header.Length);

                        byte[] data = new byte[fs.Length - (0x20 + pvr1.MainTextureOffset)];
                        fs.Position += pvr1.MainTextureOffset;
                        fs.Read(data, 0, data.Length);

                        pvrHeaderHash2 = md5.ComputeHash(header);
                        pvrDataHash2 = md5.ComputeHash(data);
                    }

                    Assert.IsTrue(CompareArrays(pvrHeaderHash1, pvrHeaderHash2) && CompareArrays(pvrDataHash1, pvrDataHash2), $"\"{testFileName}\": file has not been regenerated correctly.");
                }
                else
                {
                    byte[] pvrHash1;
                    byte[] pvrHash2;

                    using (FileStream fs = File.OpenRead(pvrFilePath1))
                    {
                        pvrHash1 = md5.ComputeHash(fs);
                    }

                    using (FileStream fs = File.OpenRead(pvrFilePath2))
                    {
                        pvrHash2 = md5.ComputeHash(fs);
                    }

                    Assert.IsTrue(CompareArrays(pvrHash1, pvrHash2), $"\"{testFileName}\": file has not been regenerated correctly.");
                }

                if (pvr1.HasExternalPalette)
                {
                    byte[] pvpHash1;
                    byte[] pvpHash2;

                    using (FileStream fs = File.OpenRead(pvpFilePath1))
                    {
                        pvpHash1 = md5.ComputeHash(fs);
                    }

                    using (FileStream fs = File.OpenRead(pvpFilePath2))
                    {
                        pvpHash2 = md5.ComputeHash(fs);
                    }

                    Assert.IsTrue(CompareArrays(pvpHash1, pvpHash2), $"\"{testFileName}\": palette has not been regenerated correctly.");
                }

                File.Delete(pvrFilePath2);
                File.Delete(jsonFilePath);
                File.Delete(tgaFilePath);

                if (File.Exists(pvpFilePath1))
                {
                    File.Delete(pvpFilePath2);
                }
            }
        }

        static bool CompareArrays(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length) return false;

            for (int h = 0; h < array1.Length; h++)
            {
                if (array1[h] != array2[h]) return false;
            }

            return true;
        }
    }
}