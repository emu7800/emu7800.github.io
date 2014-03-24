// © Mike Murphy

using System;
using EMU7800.D2D.Interop;

namespace EMU7800.D2D.Shell
{
    public sealed class ButtonToggle : ButtonBase
    {
        TextLayout _textLayout;

        public string Text { get; set; }
        public string TextFontFamilyName { get; set; }
        public int TextFontSize { get; set; }

        public bool IsChecked { get; set; }

        public event EventHandler<EventArgs> Checked;
        public event EventHandler<EventArgs> Unchecked;

        public ButtonToggle()
        {
            TextFontFamilyName = Styles.NormalFontFamily;
            TextFontSize = Styles.NormalFontSize;
            Clicked += OnClicked;
        }

        #region ControlBase Overrides

        public override void Render(GraphicsDevice gd)
        {
            var rect = Struct.ToRectF(Location, Size);
            if (IsPressed || IsChecked)
            {
                gd.FillRectangle(rect, D2DSolidColorBrush.White);
                gd.DrawText(_textLayout, Location, D2DSolidColorBrush.Black);
            }
            else if (IsMouseOver)
            {
                gd.DrawRectangle(rect, 2.0f, D2DSolidColorBrush.White);
                gd.DrawText(_textLayout, Location, D2DSolidColorBrush.White);
            }
            else
            {
                gd.DrawRectangle(rect, 1.0f, D2DSolidColorBrush.White);
                gd.DrawText(_textLayout, Location, D2DSolidColorBrush.White);
            }
        }

        protected override void CreateResources(GraphicsDevice gd)
        {
            base.CreateResources(gd);
            _textLayout = gd.CreateTextLayout(TextFontFamilyName, TextFontSize, Text, Size.Width, Size.Height);
            _textLayout.SetTextAlignment(DWriteTextAlignment.Center);
            _textLayout.SetParagraphAlignment(DWriteParaAlignment.Center);
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
                Checked = null;
                Unchecked = null;
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Helpers

        private void OnClicked(object sender, EventArgs e)
        {
            IsChecked = !IsChecked;
            if (IsChecked)
            {
                if (Checked != null)
                    Checked(sender, e);
            }
            else
            {
                if (Unchecked != null)
                    Unchecked(sender, e);
            }
        }

        #endregion
    }
}