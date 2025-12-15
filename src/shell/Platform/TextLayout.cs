namespace EMU7800.Shell;

public class TextLayout : DisposableResource
{
    public static TextLayout Empty { get; } = new();

    #region Fields

    public SizeF Size { get; protected set; }
    public float Width => Size.Width;
    public float Height => Size.Height;

    #endregion

    public virtual void Draw(PointF location) {}

    #region Constructors

    protected TextLayout() {}

    #endregion
}