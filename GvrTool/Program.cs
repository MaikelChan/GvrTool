using GvrTool.Gvr;
using GvrTool.Pvr;
using System;
using System.IO;
using System.Reflection;

namespace GvrTool
{
    class Program
    {
        static void Main(string[] args)
        {
            ShowHeader();

            if (args.Length < 3)
            {
                ShowUsage();
                return;
            }

#if !DEBUG
            try
            {
#endif
            switch (args[0])
            {
                case "-d":
                case "--decode":
                {
                    string extension = Path.GetExtension(args[1]);

                    if (extension.Equals(".gvr", StringComparison.OrdinalIgnoreCase))
                    {
                        GVR gvr = new GVR();
                        gvr.LoadFromGvrFile(args[1]);
                        gvr.SaveToTgaFile(args[2]);
                    }
                    else if (extension.Equals(".pvr", StringComparison.OrdinalIgnoreCase))
                    {
                        PVR pvr = new PVR();
                        pvr.LoadFromPvrFile(args[1]);
                        pvr.SaveToTgaFile(args[2]);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\"{args[1]}\" doesn't have the right extension. It must be either .gvr or .pvr.");

                        break;
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\"{args[1]}\" has been decoded successfully!");

                    break;
                }
                case "-e":
                case "--encode":
                {
                    string extension = Path.GetExtension(args[2]);

                    if (extension.Equals(".gvr", StringComparison.OrdinalIgnoreCase))
                    {
                        GVR gvr = new GVR();
                        gvr.LoadFromTgaFile(args[1]);
                        gvr.SaveToGvrFile(args[2]);
                    }
                    else if (extension.Equals(".pvr", StringComparison.OrdinalIgnoreCase))
                    {
                        PVR pvr = new PVR();
                        pvr.LoadFromTgaFile(args[1]);
                        pvr.SaveToPvrFile(args[2]);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"\"{args[2]}\" doesn't have the right extension. It must be either .gvr or .pvr.");

                        break;
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\"{args[1]}\" has been encoded successfully!");

                    break;
                }
                default:
                {
                    ShowUsage();
                    return;
                }
            }
#if !DEBUG
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
            }
#endif

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void ShowHeader()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            string v = $"{version.Major}.{version.Minor}.{version.Build}";

            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine();
            Console.WriteLine("        #----------------------------------------------------------------#");

            Console.WriteLine("        #                    GvrTool - Version " + v + "                     #");
            Console.Write("        #              ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("https://github.com/MaikelChan/GvrTool");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("             #");

            Console.WriteLine("        #                                                                #");
            Console.WriteLine("        #                    By MaikelChan / PacoChan                    #");
            Console.WriteLine("        #----------------------------------------------------------------#\n\n");

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void ShowUsage()
        {
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine("Usage:\n");

            Console.WriteLine("  Decode GVR File:");

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("    GvrTool -d <input_gvr_file> <output_tga_file>");
            Console.WriteLine("    GvrTool --decode <input_gvr_file> <output_tga_file>");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine("  Encode GVR file:");

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("    GvrTool -e <input_tga_file> <output_gvr_file>");
            Console.WriteLine("    GvrTool --encode <input_tga_file> <output_gvr_file>");

            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}