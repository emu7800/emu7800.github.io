using System;

namespace EMU7800.Shell;

public interface ITextFormatDriver
{
    int Create(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize);
    void Draw(string text, RectF rect, SolidColorBrush brush);
    void Release();
    void SetParagraphAlignment(WriteParaAlignment paragraphAlignment);
    void SetTextAlignment(WriteTextAlignment textAlignment);
}

public sealed class EmptyTextFormatDriver : ITextFormatDriver
{
    public static readonly EmptyTextFormatDriver Default = new();
    EmptyTextFormatDriver() {}

    #region ITextFormatDriver Members

    public int Create(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize) => -1;
    public void Draw(string text, RectF rect, SolidColorBrush brush) {}
    public void Release() {}
    public void SetParagraphAlignment(WriteParaAlignment paragraphAlignment) {}
    public void SetTextAlignment(WriteTextAlignment textAlignment) {}

    #endregion
}

public class TextFormat : IDisposable
{
    public static readonly TextFormat Default = new();
    public static Func<ITextFormatDriver> DriverFactory { get; set; } = () => EmptyTextFormatDriver.Default;

    #region Fields

    readonly ITextFormatDriver _driver = EmptyTextFormatDriver.Default;
    bool _disposed;

    public int HR { get; init; } = -1;

    #endregion

    public void SetTextAlignment(WriteTextAlignment textAlignment)
      => _driver.SetTextAlignment(textAlignment);

    public void SetParagraphAlignment(WriteParaAlignment paragraphAlignment)
      => _driver.SetParagraphAlignment(paragraphAlignment);

    public void Draw(string text, RectF rect, SolidColorBrush brush)
      => _driver.Draw(text, rect, brush);

    #region IDispose Members

    public void Dispose()
    {
        if (_disposed)
            return;
        _driver.Release();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    ~TextFormat()
      => _driver.Release();

    #endregion

    #region Constructors

    TextFormat() {}

    public TextFormat(string fontFamilyName, float fontSize)
        : this(fontFamilyName, -1, -1, -1, fontSize) {}

    public TextFormat(string fontFamilyName, int fontWeight, int fontStyle, int fontStretch, float fontSize)
    {
        _driver = DriverFactory();
        HR = _driver.Create(fontFamilyName, fontWeight, fontStyle, fontStretch, fontSize);
    }

    #endregion
}