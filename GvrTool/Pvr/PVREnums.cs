
namespace GvrTool.Pvr
{
    // https://github.com/nickworonekin/puyotools

    public enum PvrPixelFormat : byte
    {
        Argb1555 = 0x00,
        Rgb565 = 0x01,
        Argb4444 = 0x02,
        Yuv422 = 0x03,
        Bump = 0x04,
        Argb8888 = 0x06,
    }

    public enum PvrDataFormat : byte
    {
        SquareTwiddled = 0x01,
        SquareTwiddledMipmaps = 0x02,
        Vq = 0x03,
        VqMipmaps = 0x04,
        Index4 = 0x05,
        Index4Mipmaps = 0x06,
        Index8 = 0x07,
        Index8Mipmaps = 0x08,
        Rectangle = 0x09,
        Stride = 0x0B,
        RectangleTwiddled = 0x0D,
        SmallVq = 0x10,
        SmallVqMipmaps = 0x11,
        SquareTwiddledMipmapsAlt = 0x12,
    }

    public enum PvrCompressionFormat
    {
        None,
        Rle,
    }

    //[Flags]
    //public enum PvrDataFlags : byte
    //{
    //    None = 0x0,
    //    Mipmaps = 0x1,
    //    ExternalPalette = 0x2,
    //    InternalPalette = 0x8,
    //    Palette = ExternalPalette | InternalPalette,
    //}
}