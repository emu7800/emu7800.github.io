// © Mike Murphy

namespace EMU7800.Shell;

public sealed class LabelControl : ControlBase
{
    #region Fields

    TextLayout _textLayout = TextLayout.Empty;

    #endregion

    #region Public Properties

    public string Text
    {
        get => field ?? string.Empty;
        set
        {
            if (field == value)
                return;
            field = value;
            SafeDispose(ref _textLayout);
        }
    }

    public string TextFontFamilyName
    {
        get => field ?? Styles.NormalFontFamily;
        set
        {
            if (field == value)
                return;
            field = value;
            SafeDispose(ref _textLayout);
        }
    }

    public int TextFontSize
    {
        get => field;
        set
        {
            if (field == value)
                return;
            field = value;
            SafeDispose(ref _textLayout);
        }
    }

    public WriteTextAlignment TextAlignment
    {
        get => field;
        set
        {
            if (field == value)
                return;
            field = value;
            SafeDispose(ref _textLayout);
        }
    }

    public WriteParaAlignment ParagraphAlignment
    {
        get => field;
        set
        {
            if (field == value)
                return;
            field = value;
            SafeDispose(ref _textLayout);
        }
    }

    #endregion

    #region ControlBase Overrides

    public override void SizeChanged()
    {
        base.SizeChanged();
        SafeDispose(ref _textLayout);
    }

    public override void Render()
    {
        if (_textLayout == TextLayout.Empty)
        {
            CreateResources2();
        }
        GraphicsDevice.Draw(_textLayout, Location, SolidColorBrush.White);
    }

    protected override void CreateResources()
    {
        base.CreateResources();
        CreateResources2();
    }

    protected override void DisposeResources()
    {
        SafeDispose(ref _textLayout);
        base.DisposeResources();
    }

    #endregion

    #region Helpers

    void CreateResources2()
    {
        _textLayout = GraphicsDevice.CreateTextLayout(TextFontFamilyName, TextFontSize, Text, Size.Width, Size.Height, ParagraphAlignment, TextAlignment);
    }

    #endregion
}