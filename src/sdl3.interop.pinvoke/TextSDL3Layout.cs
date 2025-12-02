// © Mike Murphy

using EMU7800.Shell;

namespace EMU7800.SDL3.Interop;

public sealed class TextSDL3Layout : TextLayout
{
    public override void Draw(PointF location, SolidColorBrush brush)
    {
    }

    #region Constructors

    public TextSDL3Layout(string fontFamilyName, float fontSize, string text, float width, float height, WriteParaAlignment paragraphAlignment, WriteTextAlignment textAlignment)
    {
    }

    #endregion
}