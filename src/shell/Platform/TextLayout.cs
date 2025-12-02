namespace EMU7800.Shell;

public class TextLayout : DisposableResource
{
    public static TextLayout Empty { get; } = new();

    #region Fields

    public SizeF Size { get; protected set; }
    public float Width => Size.Width;
    public float Height => Size.Height;
    public int LineCount { get; protected set; }

    #endregion

    public virtual void Draw(PointF location, SolidColorBrush brush) {}

    #region Constructors

    protected TextLayout() {}

    #endregion
}