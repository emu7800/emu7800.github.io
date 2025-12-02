// © Mike Murphy

using System;

namespace EMU7800.Shell;

public sealed class ButtonToggle : ButtonBase
{
    static readonly EventHandler<EventArgs> DefaultEventHandler = (s, o) => {};

    TextLayout _textLayout = TextLayout.Empty;

    public string Text { get; set; } = string.Empty;
    public string TextFontFamilyName { get; set; } = string.Empty;
    public int TextFontSize { get; set; }

    public bool IsChecked { get; set; }

    public event EventHandler<EventArgs> Checked = DefaultEventHandler;
    public event EventHandler<EventArgs> Unchecked = DefaultEventHandler;

    public ButtonToggle()
    {
        TextFontFamilyName = Styles.NormalFontFamily;
        TextFontSize = Styles.NormalFontSize;
        Clicked += OnClicked;
    }

    #region ControlBase Overrides

    public override void Render()
    {
        if (IsPressed || IsChecked)
        {
            GraphicsDevice.FillRectangle(new(Location, Size), SolidColorBrush.White);
            GraphicsDevice.Draw(_textLayout, Location, SolidColorBrush.Black);
        }
        else if (IsMouseOver)
        {
            GraphicsDevice.DrawRectangle(new(Location, Size), 2.0f, SolidColorBrush.White);
            GraphicsDevice.Draw(_textLayout, Location, SolidColorBrush.White);
        }
        else
        {
            GraphicsDevice.DrawRectangle(new(Location, Size), 1.0f, SolidColorBrush.White);
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

    #region IDisposable Members

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Checked = DefaultEventHandler;
            Unchecked = DefaultEventHandler;
        }
        base.Dispose(disposing);
    }

    #endregion

    #region Helpers

    private void OnClicked(object? sender, EventArgs e)
    {
        IsChecked = !IsChecked;
        if (IsChecked)
        {
            Checked(sender, e);
        }
        else
        {
            Unchecked(sender, e);
        }
    }

    #endregion
}