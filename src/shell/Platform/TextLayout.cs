using System;

namespace EMU7800.Shell;

public interface ITextLayoutDriver
{
    int Create(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize, string text, float width, float height);
    void Draw(PointF location, SolidColorBrush brush);
    (SizeF, int) GetMetrics();
    void Release();
    void SetParagraphAlignment(WriteParaAlignment paragraphAlignment);
    void SetTextAlignment(WriteTextAlignment textAlignment);
}

public sealed class EmptyTextLayoutDriver : ITextLayoutDriver
{
    public static readonly EmptyTextLayoutDriver Default = new();
    EmptyTextLayoutDriver() {}

    #region ITextLayoutDriver Members

    public int Create(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize, string text, float width, float height) => -1;
    public void Draw(PointF location, SolidColorBrush brush) {}
    public (SizeF, int) GetMetrics() => (new(0, 0), 0);
    public void Release() {}
    public void SetParagraphAlignment(WriteParaAlignment paragraphAlignment) {}
    public void SetTextAlignment(WriteTextAlignment textAlignment) {}

    #endregion
}

public sealed class TextLayout : IDisposable
{
    public static readonly TextLayout Default = new();
    public static Func<ITextLayoutDriver> DriverFactory { get; set; } = () => EmptyTextLayoutDriver.Default;

    #region Fields

    readonly ITextLayoutDriver _driver = EmptyTextLayoutDriver.Default;
    bool _disposed;

    public int HR { get; init; } = -1;
    public SizeF Size { get; init; }
    public float Width => Size.Width;
    public float Height => Size.Height;
    public int LineCount { get; private set; }

    #endregion

    public void SetTextAlignment(WriteTextAlignment textAlignment)
      => _driver.SetTextAlignment(textAlignment);

    public void SetParagraphAlignment(WriteParaAlignment paragraphAlignment)
      => _driver.SetParagraphAlignment(paragraphAlignment);

    public void Draw(PointF location, SolidColorBrush brush)
      => _driver.Draw(location, brush);

    #region IDispose Members

    public void Dispose()
    {
        if (_disposed)
            return;
        _driver.Release();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~TextLayout()
      => _driver.Release();

    #endregion

    #region Constructors

    TextLayout() {}

    public TextLayout(string fontFamilyName, float fontSize, string text, float width, float height)
        : this(fontFamilyName, -1, -1, -1, fontSize, text, width, height) {}

    public TextLayout(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize, string text, float width, float height)
    {
        _driver = DriverFactory();
        HR = _driver.Create(fontFamilyName, fontWeight, fontStyle, fontStretch, fontSize, text, width, height);

        if (HR == 0)
        {
            (SizeF size, int lineCount) = _driver.GetMetrics();
            Size = size;
            LineCount = lineCount;
        }
    }

    #endregion
}