// © Mike Murphy

namespace EMU7800.Shell;

public sealed class Button : ButtonBase
{
    TextLayout _textLayout = TextLayout.Empty;

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
            GraphicsDevice.Draw(_textLayout, Location, SolidColorBrush.Black);
        }
        else if (IsMouseOver)
        {
            GraphicsDevice.DrawRectangle(new RectF(Location, Size), 2.0f, SolidColorBrush.White);
            GraphicsDevice.Draw(_textLayout, Location, SolidColorBrush.White);
        }
        else
        {
            GraphicsDevice.DrawRectangle(new RectF(Location, Size), 1.0f, SolidColorBrush.White);
            GraphicsDevice.Draw(_textLayout, Location, SolidColorBrush.White);
        }
    }

    protected override void CreateResources()
    {
        base.CreateResources();
        _textLayout = GraphicsDevice.CreateTextLayout(TextFontFamilyName, TextFontSize, Text, Size.Width, Size.Height, WriteParaAlignment.Center, WriteTextAlignment.Center);
    }

    protected override void DisposeResources()
    {
        SafeDispose(ref _textLayout);
        base.DisposeResources();
    }

    #endregion
}