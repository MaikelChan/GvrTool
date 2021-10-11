using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Security.Cryptography;

namespace GvrTool.Tests
{
    [TestClass]
    public class GvrToolTests
    {
        const string TestFilesDirectory = @"..\..\..\..\..\GVRs";

        [DataTestMethod]
        [DataRow("0000.gvr")] // ImageData: I4, PaletteData: RGB5A3 (RGB555)   - Resident Evil: Code Veronica (GameCube)
        [DataRow("0001.gvr")] // ImageData: I4, PaletteData: RGB565            - Resident Evil: Code Veronica (GameCube)
        public void Regenerate(string testFileName)
        {
            using (MD5 md5 = MD5.Create())
            {
                string gvrFilePath1 = Path.Combine(TestFilesDirectory, testFileName);
                string gvrFilePath2 = Path.ChangeExtension(gvrFilePath1, null) + "_2" + Path.GetExtension(gvrFilePath1);

                string gvpFilePath1 = Path.ChangeExtension(gvrFilePath1, ".gvp");
                string gvpFilePath2 = Path.ChangeExtension(gvpFilePath1, null) + "_2" + Path.GetExtension(gvpFilePath1);

                string jsonFilePath = Path.ChangeExtension(gvrFilePath1, ".json");
                string tgaFilePath = Path.ChangeExtension(gvrFilePath1, ".tga");

                GVR gvr = new GVR();
                gvr.LoadFromGvrFile(gvrFilePath1);
                gvr.SaveToTgaFile(tgaFilePath);

                gvr = new GVR();
                gvr.LoadFromTgaFile(tgaFilePath);
                gvr.SaveToGvrFile(gvrFilePath2);

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

                Assert.IsTrue(CompareHashes(gvrHash1, gvrHash2), $"File \"{testFileName}\" was not regenerated correctly.");

                if (File.Exists(gvpFilePath1))
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

                    Assert.IsTrue(CompareHashes(gvpHash1, gvpHash2), $"Palette of file \"{testFileName}\" was not regenerated correctly.");
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

        bool CompareHashes(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length) return false;

            for (int h = 0; h < hash1.Length; h++)
            {
                if (hash1[h] != hash2[h]) return false;
            }

            return true;
        }
    }
}