// © Mike Murphy

using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public sealed class LabelControl : ControlBase
    {
        #region Fields

        TextLayout _textLayout = TextLayoutDefault;
        string _text = string.Empty, _textFontFamilyName = Styles.NormalFontFamily;
        int _textFontSize = Styles.NormalFontSize;
        DWriteTextAlignment _textAlignment = DWriteTextAlignment.Leading;
        DWriteParaAlignment _paraAlignment = DWriteParaAlignment.Near;

        #endregion

        #region Public Properties

        public string Text
        {
            get => _text;
            set
            {
                if (_text == value)
                    return;
                _text = value;
                SafeDispose(ref _textLayout);
            }
        }

        public string TextFontFamilyName
        {
            get { return _textFontFamilyName; }
            set
            {
                if (_textFontFamilyName == value)
                    return;
                _textFontFamilyName = value;
                SafeDispose(ref _textLayout);
            }
        }

        public int TextFontSize
        {
            get { return _textFontSize; }
            set
            {
                if (_textFontSize == value)
                    return;
                _textFontSize = value;
                SafeDispose(ref _textLayout);
            }
        }

        public DWriteTextAlignment TextAlignment
        {
            get { return _textAlignment; }
            set
            {
                if (_textAlignment == value)
                    return;
                _textAlignment = value;
                SafeDispose(ref _textLayout);
            }
        }

        public DWriteParaAlignment ParagraphAlignment
        {
            get { return _paraAlignment; }
            set
            {
                if (_paraAlignment == value)
                    return;
                _paraAlignment = value;
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

        public override void Render(GraphicsDevice gd)
        {
            if (_textLayout == TextLayoutDefault)
                CreateResources2(gd);
            gd.DrawText(_textLayout, Location, D2DSolidColorBrush.White);
        }

        protected override void CreateResources(GraphicsDevice gd)
        {
            base.CreateResources(gd);
            CreateResources2(gd);
        }

        protected override void DisposeResources()
        {
            SafeDispose(ref _textLayout);
            base.DisposeResources();
        }

        #endregion

        #region Helpers

        void CreateResources2(GraphicsDevice gd)
        {
            _textLayout = gd.CreateTextLayout(TextFontFamilyName, TextFontSize, Text, Size.Width, Size.Height);
            _textLayout.SetTextAlignment(TextAlignment);
            _textLayout.SetParagraphAlignment(ParagraphAlignment);
        }

        #endregion
    }
}
