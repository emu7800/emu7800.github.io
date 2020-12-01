using System;

using static EMU7800.Win32.Interop.Direct2DNativeMethods;

namespace EMU7800.Win32.Interop
{
    public class TextFormat : IDisposable
    {
        public IntPtr TextFormatPtr { get; private set; }

        public int HR { get; private set; }

        public void SetTextAlignment(DWriteTextAlignment textAlignment)
            => Direct2D_SetTextAlignmentForTextFormat(TextFormatPtr, textAlignment);

        public void SetParagraphAlignment(DWriteParaAlignment paragraphAlignment)
            => Direct2D_SetParagraphAlignmentForTextFormat(TextFormatPtr, paragraphAlignment);

        #region IDispose Members

        public void Dispose()
        {
            Direct2D_ReleaseTextFormat(TextFormatPtr);
            TextFormatPtr = IntPtr.Zero;
            HR = 0;
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Constructors

        public TextFormat(string fontFamilyName, float fontSize)
        {
            var ptr = IntPtr.Zero;
            HR = Direct2D_CreateTextFormat(fontFamilyName, fontSize, ref ptr);
            TextFormatPtr = ptr;
        }

        public TextFormat(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch,float fontSize)
        {
            var ptr = IntPtr.Zero;
            HR = Direct2D_CreateTextFormatEx(fontFamilyName, fontWeight, fontStyle, fontStretch, fontSize, ref ptr);
            TextFormatPtr = ptr;
        }

        #endregion
    }
}
