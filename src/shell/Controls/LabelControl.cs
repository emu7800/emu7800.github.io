// © Mike Murphy

using EMU7800.Win32.Interop;

namespace EMU7800.D2D.Shell
{
    public sealed class LabelControl : ControlBase
    {
        #region Fields

        TextLayout _textLayout = TextLayout.Default;
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
            get => _textFontFamilyName;
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
            get => _textFontSize;
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
            get => _textAlignment;
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
            get => _paraAlignment;
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

        public override void Render()
        {
            if (_textLayout == TextLayout.Default)
                CreateResources2();
            GraphicsDevice.Draw(_textLayout, Location, D2DSolidColorBrush.White);
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
            _textLayout = new TextLayout(TextFontFamilyName, TextFontSize, Text, Size.Width, Size.Height);
            _textLayout.SetTextAlignment(TextAlignment);
            _textLayout.SetParagraphAlignment(ParagraphAlignment);
        }

        #endregion
    }
}
