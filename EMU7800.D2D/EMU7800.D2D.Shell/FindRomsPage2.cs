// © Mike Murphy

using System;
using EMU7800.Services;

namespace EMU7800.D2D.Shell
{
    public sealed partial class FindRomsPage2 : PageBase
    {
        #region Fields

        readonly RomImportService _romImportService = new RomImportService();
        readonly LabelControl _labelStep;
        readonly ButtonBase _buttonOk, _buttonCancel;
        readonly NumberControl _numbercontrolRomCount;

        #endregion

        public FindRomsPage2()
        {
            _labelStep = new LabelControl
            {
                Text = "One moment please...",
                TextFontFamilyName = Styles.NormalFontFamily,
                TextFontSize = Styles.NormalFontSize,
                Location = Struct.ToPointF(40, 80),
                Size = Struct.ToSizeF(600, 30),
            };
            _numbercontrolRomCount = new NumberControl
            {
                TextFontFamilyName = Styles.TitleFontFamily,
                TextFontSize = Styles.TitleFontSize,
                Size = Struct.ToSizeF(100, 60)
            };
            _buttonOk = new CheckButton
            {
                Location = Struct.ToPointF(_labelStep.Location.X, _labelStep.Location.Y + _labelStep.Size.Height + 25),
                IsVisible = false
            };
            _buttonCancel = new CancelButton
            {
                Location = _buttonOk.Location,
                IsVisible = false
            };
            Controls.Add(_labelStep, _buttonOk, _buttonCancel, _numbercontrolRomCount);

            _buttonOk.Clicked += _buttonOk_Clicked;
            _buttonCancel.Clicked += _buttonCancel_Clicked;

            StartImport();
        }

        #region PageBase Overrides

        public override void Resized(Interop.SizeF size)
        {
            base.Resized(size);
            _numbercontrolRomCount.Location = Struct.ToPointF(
                size.Width - _numbercontrolRomCount.Size.Width - 5,
                5);
        }

        public override void Update(TimerDevice td)
        {
            base.Update(td);
            _numbercontrolRomCount.Value = _romImportService.FilesRecognized;
        }

        #endregion

        #region Event Handlers

        void _buttonOk_Clicked(object sender, EventArgs eventArgs)
        {
            PopPage();
        }

        void _buttonCancel_Clicked(object sender, EventArgs eventArgs)
        {
            _romImportService.CancelRequested = true;
        }

        #endregion
    }
}
