// © Mike Murphy

using System;

namespace EMU7800.Shell;

public sealed class ButtonToggle : ButtonBase
{
    static readonly EventHandler<EventArgs> DefaultEventHandler = (s, o) => {};

    TextLayout _textLayoutBlack = TextLayout.Empty;
    TextLayout _textLayoutWhite = TextLayout.Empty;

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

    public override void Render(IGraphicsDeviceDriver graphicsDevice)
    {
        if (IsPressed || IsChecked)
        {
            graphicsDevice.FillRectangle(new(Location, Size), SolidColorBrush.White);
            graphicsDevice.Draw(_textLayoutBlack, Location);
        }
        else if (IsMouseOver)
        {
            graphicsDevice.DrawRectangle(new(Location, Size), 2.0f, SolidColorBrush.White);
            graphicsDevice.Draw(_textLayoutWhite, Location);
        }
        else
        {
            graphicsDevice.DrawRectangle(new(Location, Size), 1.0f, SolidColorBrush.White);
            graphicsDevice.Draw(_textLayoutWhite, Location);
        }
    }

    protected override void CreateResources(IGraphicsDeviceDriver graphicsDevice)
    {
        base.CreateResources(graphicsDevice);
        _textLayoutBlack = graphicsDevice.CreateTextLayout(TextFontFamilyName, TextFontSize, Text, Size.Width, Size.Height, WriteParaAlignment.Center, WriteTextAlignment.Center, SolidColorBrush.Black);
        _textLayoutWhite = graphicsDevice.CreateTextLayout(TextFontFamilyName, TextFontSize, Text, Size.Width, Size.Height, WriteParaAlignment.Center, WriteTextAlignment.Center, SolidColorBrush.White);
    }

    protected override void DisposeResources()
    {
        SafeDispose(ref _textLayoutBlack);
        SafeDispose(ref _textLayoutWhite);
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