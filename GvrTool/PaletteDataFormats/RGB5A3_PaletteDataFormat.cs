using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using TGASharpLib;

namespace GvrTool.PaletteDataFormats
{
    class RGB5A3_PaletteDataFormat : PaletteDataFormat
    {
        public override uint DecodedDataLength => (uint)(PaletteEntryCount * 4);
        public override uint EncodedDataLength => (uint)(PaletteEntryCount * 2);

        public override TgaColorMapEntrySize TgaColorMapEntrySize => TgaColorMapEntrySize.A8R8G8B8;

        public RGB5A3_PaletteDataFormat(ushort paletteEntryCount) : base(paletteEntryCount)
        {

        }

        public override byte[] Decode(byte[] input)
        {
            byte[] output = new byte[DecodedDataLength];
            int offset = 0;

            //if (DecodeSIMD(input, output)) return output;

            for (int p = 0; p < output.Length; p += 4)
            {
                ushort entry = (ushort)((input[offset] << 8) | input[offset + 1]);
                offset += 2;

                if ((entry & 0b1000_0000_0000_0000) == 0) // Argb3444 
                {
                    output[p + 3] = (byte)(((entry >> 12) & 0b0000_0000_0000_0111) * (255 / 7));
                    output[p + 2] = (byte)(((entry >> 08) & 0b0000_0000_0000_1111) * (255 / 15));
                    output[p + 1] = (byte)(((entry >> 04) & 0b0000_0000_0000_1111) * (255 / 15));
                    output[p + 0] = (byte)(((entry >> 00) & 0b0000_0000_0000_1111) * (255 / 15));
                }
                else // Rgb555
                {
                    output[p + 3] = 255;
                    output[p + 2] = (byte)(((entry >> 10) & 0b0000_0000_0001_1111) * (255 / 31));
                    output[p + 1] = (byte)(((entry >> 05) & 0b0000_0000_0001_1111) * (255 / 31));
                    output[p + 0] = (byte)(((entry >> 00) & 0b0000_0000_0001_1111) * (255 / 31));
                }
            }

            return output;
        }

        //static unsafe bool DecodeSIMD(byte[] input, byte[] output)
        //{
        //    if (!Sse41.IsSupported) return false;

        //    unsafe
        //    {
        //        fixed (byte* inputPtr = input)
        //        fixed (byte* outputPtr = output)
        //        {
        //            int inputOffset = 0;
        //            int outputOffset = 0;

        //            Vector128<byte> maskShuffle = Vector128.Create((byte)1, 0, 3, 2, 5, 4, 7, 6, 9, 8, 11, 10, 13, 12, 15, 14);
        //            Vector128<ushort> mask3bit = Vector128.Create((ushort)0b0000_0000_0000_0111);
        //            Vector128<ushort> mask4bit = Vector128.Create((ushort)0b0000_0000_0000_1111);
        //            Vector128<ushort> mask5bit = Vector128.Create((ushort)0b0000_0000_0001_1111);
        //            Vector128<ushort> maskHighestBit = Vector128.Create((ushort)0b1000_0000_0000_0000);
        //            Vector128<ushort> mult_255_31 = Vector128.Create((ushort)(255 / 31));
        //            Vector128<ushort> mult_255_15 = Vector128.Create((ushort)(255 / 15));
        //            Vector128<ushort> mult_255_07 = Vector128.Create((ushort)(255 / 07));

        //            for (; ; )
        //            {
        //                // Obtain 8 colors (2 bytes each)

        //                Vector128<byte> bytes = Sse2.LoadVector128(inputPtr + inputOffset);
        //                bytes = Ssse3.Shuffle(bytes, maskShuffle);
        //                Vector128<ushort> colors = bytes.AsUInt16();

        //                // Rgb555

        //                Vector128<ushort> aRgb555 = Vector128.Create((ushort)255);
        //                Vector128<ushort> bRgb555 = Sse2.MultiplyLow(Sse2.And(Sse2.ShiftRightLogical(colors, 10), mask5bit), mult_255_31);
        //                Vector128<ushort> gRgb555 = Sse2.MultiplyLow(Sse2.And(Sse2.ShiftRightLogical(colors, 05), mask5bit), mult_255_31);
        //                Vector128<ushort> rRgb555 = Sse2.MultiplyLow(Sse2.And(colors, mask5bit), mult_255_31);

        //                // Argb3444

        //                Vector128<ushort> aArgb3444 = Sse2.MultiplyLow(Sse2.And(Sse2.ShiftRightLogical(colors, 12), mask3bit), mult_255_07);
        //                Vector128<ushort> bArgb3444 = Sse2.MultiplyLow(Sse2.And(Sse2.ShiftRightLogical(colors, 08), mask4bit), mult_255_15);
        //                Vector128<ushort> gArgb3444 = Sse2.MultiplyLow(Sse2.And(Sse2.ShiftRightLogical(colors, 04), mask4bit), mult_255_15);
        //                Vector128<ushort> rArgb3444 = Sse2.MultiplyLow(Sse2.And(colors, mask4bit), mult_255_15);

        //                // Compare if highest bit is set or not and store results. Not set: Argb3444; Set: Rgb555

        //                Vector128<ushort> comparisonResult = Sse2.CompareEqual(Sse2.And(colors, maskHighestBit), Vector128<ushort>.Zero);

        //                // Blend both color formats based on previous comparison results

        //                Vector128<ushort> a = Sse41.BlendVariable(aRgb555, aArgb3444, comparisonResult);
        //                Vector128<ushort> b = Sse41.BlendVariable(bRgb555, bArgb3444, comparisonResult);
        //                Vector128<ushort> g = Sse41.BlendVariable(gRgb555, gArgb3444, comparisonResult);
        //                Vector128<ushort> r = Sse41.BlendVariable(rRgb555, rArgb3444, comparisonResult);

        //                // Combine a+b+g+r and copy to output array

        //                Vector128<ushort> finalOutputL = Sse2.Or(Sse2.ShiftLeftLogical(Sse2.UnpackLow(r, b), 8), Sse2.UnpackLow(g, a));
        //                Vector128<ushort> finalOutputH = Sse2.Or(Sse2.ShiftLeftLogical(Sse2.UnpackHigh(r, b), 8), Sse2.UnpackHigh(g, a));
        //                Sse2.Store(outputPtr + outputOffset, Ssse3.Shuffle(finalOutputL.AsByte(), maskShuffle));
        //                Sse2.Store(outputPtr + outputOffset + Vector128<byte>.Count, Ssse3.Shuffle(finalOutputH.AsByte(), maskShuffle));

        //                //// Combine a+b+g+r into final 32bit vector and copy to output array

        //                //Vector256<uint> finalOutput = Avx2.ShiftLeftLogical(Avx2.ConvertToVector256Int32(a).AsUInt32(), 24);
        //                //finalOutput = Avx2.Or(finalOutput, Avx2.ShiftLeftLogical(Avx2.ConvertToVector256Int32(b).AsUInt32(), 16));
        //                //finalOutput = Avx2.Or(finalOutput, Avx2.ShiftLeftLogical(Avx2.ConvertToVector256Int32(g).AsUInt32(), 8));
        //                //finalOutput = Avx2.Or(finalOutput, Avx2.ConvertToVector256Int32(r).AsUInt32());
        //                //Avx.Store(outputPtr + outputOffset, finalOutput.AsByte());

        //                inputOffset += Vector128<byte>.Count;
        //                outputOffset += Vector256<byte>.Count;
        //                if (inputOffset >= input.Length && outputOffset >= output.Length) break;
        //            }
        //        }
        //    }

        //    return true;
        //}

        public override byte[] Encode(byte[] input)
        {
            byte[] output = new byte[EncodedDataLength];
            int offset = 0;

            for (int p = 0; p < output.Length; p += 2)
            {
                ushort color = 0x0;

                if (input[offset + 3] > 0xDA) // Rgb555
                {
                    color |= 0x8000;
                    color |= (ushort)((input[offset + 2] >> 3) << 10);
                    color |= (ushort)((input[offset + 1] >> 3) << 5);
                    color |= (ushort)((input[offset + 0] >> 3) << 0);
                }
                else // Argb3444
                {
                    color |= (ushort)((input[offset + 3] >> 5) << 12);
                    color |= (ushort)((input[offset + 2] >> 4) << 8);
                    color |= (ushort)((input[offset + 1] >> 4) << 4);
                    color |= (ushort)((input[offset + 0] >> 4) << 0);
                }

                output[p + 0] = (byte)(color >> 8);
                output[p + 1] = (byte)(color & 0b0000_0000_1111_1111);

                offset += 4;
            }

            return output;
        }
    }
}