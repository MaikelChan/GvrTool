# GvrTool
Tool that allows to convert from GVR image format to TGA and viceversa. The GVR format is a texture format used in some GameCube and Wii games, and it's a variant of the PVR format used in some DreamCast games. When converting to TGA, it will also export a metadata file that contains info about the GVR. That allows to easily convert back again from TGA to GVR and preserve all the GVR exclusive data.

It's still very basic and it has only been tested with the GameCube version of Resident Evil: Code Veronica.

This tool contains code from:
- [Puyo Tools](https://github.com/nickworonekin/puyotools)
- [TGASharpLib](https://github.com/ALEXGREENALEX/TGASharpLib)

## Requirements
The program requires the [.NET Runtime 5.0](https://dotnet.microsoft.com/download/dotnet/5.0) and it works on Windows, Linux and MacOS.

## Changelog
You can check the changelog [here](https://github.com/MaikelChan/GvrTool/blob/main/CHANGELOG.md).

## Usage
```
 Decode GVR File:
    GvrTool -d <input_gvr_file> <output_tga_file>
    GvrTool --decode <input_gvr_file> <output_tga_file>

  Encode GVR file:
    GvrTool -e <input_tga_file> <output_gvr_file>
    GvrTool --encode <input_tga_file> <output_gvr_file>
```

## Format Specification
- https://code.google.com/archive/p/puyotools/wikis/GVRTexture.wiki
- https://github.com/nickworonekin/puyotools/wiki/PVR-Texture