using System;

namespace EMU7800.D2D.Interop
{
    public enum DWriteTextAlignment { Leading, Trailing, Center };
    public enum DWriteParaAlignment { Near, Far, Center };

    public class TextFormat : IDisposable
    {
        public int HR { get; private set; }

        public int SetTextAlignment(DWriteTextAlignment textAlignment)
        {
            return 0;
        }

        public int SetParagraphAlignment(DWriteParaAlignment paragraphAlignment)
        {
            return 0;
        }

        #region IDisposable Members

        ~TextFormat()
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
            GC.SuppressFinalize(this);
        }

        #endregion

        public TextFormat(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize, string text, float width, float height)
        {
        }
    }
}