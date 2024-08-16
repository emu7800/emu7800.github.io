// © Mike Murphy

using EMU7800.Win32.Interop;

namespace EMU7800.D2D.Shell;

public sealed class Button : ButtonBase
{
    TextLayout _textLayout = TextLayout.Default;

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
            GraphicsDevice.FillRectangle(new(Location, Size), D2DSolidColorBrush.White);
            GraphicsDevice.Draw(_textLayout, Location, D2DSolidColorBrush.Black);
        }
        else if (IsMouseOver)
        {
            GraphicsDevice.DrawRectangle(new(Location, Size), 2.0f, D2DSolidColorBrush.White);
            GraphicsDevice.Draw(_textLayout, Location, D2DSolidColorBrush.White);
        }
        else
        {
            GraphicsDevice.DrawRectangle(new(Location, Size), 1.0f, D2DSolidColorBrush.White);
            GraphicsDevice.Draw(_textLayout, Location, D2DSolidColorBrush.White);
        }
    }

    protected override void CreateResources()
    {
        base.CreateResources();
        _textLayout = new TextLayout(TextFontFamilyName, TextFontSize, Text, Size.Width, Size.Height);
        _textLayout.SetTextAlignment(DWriteTextAlignment.Center);
        _textLayout.SetParagraphAlignment(DWriteParaAlignment.Center);
    }

    protected override void DisposeResources()
    {
        SafeDispose(ref _textLayout);
        base.DisposeResources();
    }

    #endregion
}