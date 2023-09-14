using GvrTool.Gvr;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Security.Cryptography;
using TGASharpLib;

namespace GvrTool.Tests
{
    [TestClass]
    public class GvrTests
    {
        const string TEST_FILES_DIRECTORY = @"..\..\..\..\..\GVRs";
        const double THRESHOLD = 2d;

        [DataTestMethod]
        [DataRow("0000.gvr")] // ImageData: I4, PaletteData: RGB5A3   - Resident Evil: Code Veronica (GameCube)
        [DataRow("0001.gvr")] // ImageData: I4, PaletteData: RGB565   - Resident Evil: Code Veronica (GameCube)
        [DataRow("0002.gvr")] // ImageData: I8, PaletteData: RGB5A3   - Resident Evil: Code Veronica (GameCube)
        [DataRow("0004.gvr")] // ImageData: RGB5A3                    - Resident Evil: Code Veronica (GameCube)
        [DataRow("0005.gvr")] // ImageData: ARGB8888                  - Resident Evil: Code Veronica (GameCube)
        public void RegenerateAndCheckIfEqual(string testFileName)
        {
            using (MD5 md5 = MD5.Create())
            {
                string gvrFilePath1 = Path.Combine(TEST_FILES_DIRECTORY, testFileName);
                string gvrFilePath2 = Path.ChangeExtension(gvrFilePath1, null) + "_2" + Path.GetExtension(gvrFilePath1);

                string gvpFilePath1 = Path.ChangeExtension(gvrFilePath1, ".gvp");
                string gvpFilePath2 = Path.ChangeExtension(gvpFilePath1, null) + "_2" + Path.GetExtension(gvpFilePath1);

                string jsonFilePath = Path.ChangeExtension(gvrFilePath1, ".json");
                string tgaFilePath = Path.ChangeExtension(gvrFilePath1, ".tga");

                GVR gvr1 = new GVR();
                gvr1.LoadFromGvrFile(gvrFilePath1);
                gvr1.SaveToTgaFile(tgaFilePath);

                GVR gvr2 = new GVR();
                gvr2.LoadFromTgaFile(tgaFilePath);
                gvr2.SaveToGvrFile(gvrFilePath2);

                byte[] gvrHash1;
                byte[] gvrHash2;

                using (FileStream fs = File.OpenRead(gvrFilePath1))
                {
                    gvrHash1 = md5.ComputeHash(fs);
                }

                using (FileStream fs = File.OpenRead(gvrFilePath2))
                {
                    gvrHash2 = md5.ComputeHash(fs);
                }

                Assert.IsTrue(CompareArrays(gvrHash1, gvrHash2), $"\"{testFileName}\": file has not been regenerated correctly.");

                if (gvr1.HasExternalPalette)
                {
                    byte[] gvpHash1;
                    byte[] gvpHash2;

                    using (FileStream fs = File.OpenRead(gvpFilePath1))
                    {
                        gvpHash1 = md5.ComputeHash(fs);
                    }

                    using (FileStream fs = File.OpenRead(gvpFilePath2))
                    {
                        gvpHash2 = md5.ComputeHash(fs);
                    }

                    Assert.IsTrue(CompareArrays(gvpHash1, gvpHash2), $"\"{testFileName}\": palette has not been regenerated correctly.");
                }

                File.Delete(gvrFilePath2);
                File.Delete(jsonFilePath);
                File.Delete(tgaFilePath);

                if (File.Exists(gvpFilePath1))
                {
                    File.Delete(gvpFilePath2);
                }
            }
        }

        [DataTestMethod]
        [DataRow("0003.gvr")] // ImageData: Dxt1                      - Resident Evil: Code Veronica (GameCube)
        [DataRow("0006.gvr")] // ImageData: Dxt1                      - Resident Evil: Code Veronica (GameCube)
        public void RegenerateAndCheckIfSimilar(string testFileName)
        {
            using (MD5 md5 = MD5.Create())
            {
                string gvrFilePath1 = Path.Combine(TEST_FILES_DIRECTORY, testFileName);
                string gvrFilePath2 = Path.ChangeExtension(gvrFilePath1, null) + "_2" + Path.GetExtension(gvrFilePath1);

                string tgaFilePath1 = Path.ChangeExtension(gvrFilePath1, ".tga");
                string tgaFilePath2 = Path.ChangeExtension(gvrFilePath2, ".tga");

                string jsonFilePath1 = Path.ChangeExtension(gvrFilePath1, ".json");
                string jsonFilePath2 = Path.ChangeExtension(gvrFilePath2, ".json");

                GVR gvr1 = new GVR();
                gvr1.LoadFromGvrFile(gvrFilePath1);
                gvr1.SaveToTgaFile(tgaFilePath1);

                GVR gvr2 = new GVR();
                gvr2.LoadFromTgaFile(tgaFilePath1);
                gvr2.SaveToGvrFile(gvrFilePath2);

                GVR gvr3 = new GVR();
                gvr1.LoadFromGvrFile(gvrFilePath2);
                gvr1.SaveToTgaFile(tgaFilePath2);

                byte[] gvrHeader1;
                byte[] gvrHeader2;
                long gvrFileSize1;
                long gvrFileSize2;

                using (FileStream fs = File.OpenRead(gvrFilePath1))
                {
                    gvrFileSize1 = fs.Length;
                    gvrHeader1 = new byte[0x20];
                    fs.Read(gvrHeader1, 0, gvrHeader1.Length);
                }

                using (FileStream fs = File.OpenRead(gvrFilePath2))
                {
                    gvrFileSize2 = fs.Length;
                    gvrHeader2 = new byte[0x20];
                    fs.Read(gvrHeader2, 0, gvrHeader2.Length);
                }

                Assert.IsTrue(gvrFileSize1 == gvrFileSize2, $"\"{testFileName}\": GVR files are not the same size.");
                Assert.IsTrue(CompareArrays(gvrHeader1, gvrHeader2), $"\"{testFileName}\": GVR header has not been regenerated correctly.");

                TGA tga1 = new TGA(tgaFilePath1);
                TGA tga2 = new TGA(tgaFilePath2);

                byte[] pixels1 = tga1.ImageOrColorMapArea.ImageData;
                byte[] pixels2 = tga2.ImageOrColorMapArea.ImageData;

                Assert.IsTrue(pixels1.Length == pixels2.Length, $"\"{testFileName}\": TGA files are not the same size.");

                double colorAverage = 0;
                for (int p = 0; p < pixels1.Length; p += 4)
                {
                    int r = Math.Abs(pixels1[p + 0] - pixels2[p + 0]);
                    int g = Math.Abs(pixels1[p + 1] - pixels2[p + 1]);
                    int b = Math.Abs(pixels1[p + 2] - pixels2[p + 2]);
                    int a = Math.Abs(pixels1[p + 3] - pixels2[p + 3]);
                    colorAverage += (ulong)(r * r + g * g + b * b + a * a);
                }

                colorAverage /= pixels1.Length / 4d;
                Assert.IsTrue(colorAverage <= THRESHOLD * THRESHOLD, $"\"{testFileName}\": TGA files pixel data differences are beyond the threshold.");

                File.Delete(gvrFilePath2);
                File.Delete(tgaFilePath1);
                File.Delete(tgaFilePath2);
                File.Delete(jsonFilePath1);
                File.Delete(jsonFilePath2);
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