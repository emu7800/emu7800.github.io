// © Mike Murphy

namespace EMU7800.Shell;

public sealed class Button : ButtonBase
{
    TextLayout _textLayoutBlack = TextLayout.Empty;
    TextLayout _textLayoutWhite = TextLayout.Empty;

    public string Text { get; set; } = string.Empty;
    public string TextFontFamilyName { get; set; } = string.Empty;
    public int TextFontSize { get; set; }

    public Button()
    {
        TextFontFamilyName = Styles.NormalFontFamily;
        TextFontSize = Styles.NormalFontSize;
    }

    #region ControlBase Overrides

    public override void Render()
    {
        if (IsPressed)
        {
            GraphicsDevice.FillRectangle(new RectF(Location, Size), SolidColorBrush.White);
            GraphicsDevice.Draw(_textLayoutBlack, Location);
        }
        else if (IsMouseOver)
        {
            GraphicsDevice.DrawRectangle(new RectF(Location, Size), 2.0f, SolidColorBrush.White);
            GraphicsDevice.Draw(_textLayoutWhite, Location);
        }
        else
        {
            GraphicsDevice.DrawRectangle(new RectF(Location, Size), 1.0f, SolidColorBrush.White);
            GraphicsDevice.Draw(_textLayoutWhite, Location);
        }
    }

    protected override void CreateResources()
    {
        base.CreateResources();
        _textLayoutBlack = GraphicsDevice.CreateTextLayout(TextFontFamilyName, TextFontSize, Text, Size.Width, Size.Height, WriteParaAlignment.Center, WriteTextAlignment.Center, SolidColorBrush.Black);
        _textLayoutWhite = GraphicsDevice.CreateTextLayout(TextFontFamilyName, TextFontSize, Text, Size.Width, Size.Height, WriteParaAlignment.Center, WriteTextAlignment.Center, SolidColorBrush.White);
    }

    protected override void DisposeResources()
    {
        SafeDispose(ref _textLayoutBlack);
        SafeDispose(ref _textLayoutWhite);
        base.DisposeResources();
    }

    #endregion
}