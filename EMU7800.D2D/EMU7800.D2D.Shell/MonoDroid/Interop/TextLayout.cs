using System;

namespace EMU7800.D2D.Interop
{
    public class TextLayout : IDisposable
    {
        public int HR { get; private set; }

        public double Width { get; private set; }
        public double Height { get; private set; }
        public int LineCount { get; private set; }

        public int SetTextAlignment(DWriteTextAlignment textAlignment)
        {
            return 0;
        }

        public int SetParagraphAlignment(DWriteParaAlignment paragraphAlignment)
        {
            return 0;
        }

        #region IDisposable Members

        ~TextLayout()
        {
            Dispose(false);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

        public TextLayout(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize, string text, float width, float height)
        {
        }
    }
}