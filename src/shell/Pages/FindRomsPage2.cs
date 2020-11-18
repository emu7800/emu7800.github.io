// © Mike Murphy

using System;
using System.Threading.Tasks;
using EMU7800.Services;

namespace EMU7800.D2D.Shell
{
    public sealed partial class FindRomsPage2 : PageBase
    {
        #region Fields

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

            _buttonOk.Clicked += ButtonOk_Clicked;
            _buttonCancel.Clicked += ButtonCancel_Clicked;

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
            _numbercontrolRomCount.Value = RomImportService.FilesRecognized;
        }

        #endregion

        #region Event Handlers

        void ButtonOk_Clicked(object? sender, EventArgs eventArgs)
        {
            PopPage();
        }

        void ButtonCancel_Clicked(object? sender, EventArgs eventArgs)
        {
            RomImportService.CancelRequested = true;
        }

        #endregion

        #region Helpers

        async void StartImport()
        {
            _buttonCancel.IsVisible = true;

            var result = await Task.Run(() => RomImportService.Import());

            if (RomImportService.CancelRequested)
            {
                _labelStep.Text = result.IsFail ? "Canceled via internal error." : "Canceled.";
            }
            else
            {
                _labelStep.Text = "Completed.";
            }

            _buttonOk.IsVisible = true;
            _buttonCancel.IsVisible = false;
        }

        #endregion
    }
}
