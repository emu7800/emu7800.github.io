// © Mike Murphy

using EMU7800.Core;
using EMU7800.Services;
using EMU7800.Win32.Interop;
using System;
using System.Threading.Tasks;

namespace EMU7800.D2D.Shell
{
    public sealed partial class TitlePage : PageBase
    {
        readonly TitleControl _titleControl;
        readonly Button _buttonPlayAtariToday;
        readonly ButtonCircleImage _buttonAbout;
        readonly LabelControl _labelCopyr, _labelVers, _labelBusyInit;

        bool _isImportCheckStarted;

        public TitlePage()
        {
            _titleControl = new TitleControl();
            _buttonPlayAtariToday = new Button
            {
                Text = "Play Atari Today!",
                TextFontFamilyName = Styles.ExtraLargeFontFamily,
                TextFontSize = Styles.ExtraLargeFontSize,
                Size = new(500, 100),
            };
            _labelBusyInit = new LabelControl
            {
                Text = "One Moment...",
                TextFontFamilyName = Styles.LargeFontFamily,
                TextFontSize = Styles.LargeFontSize,
                TextAlignment = DWriteTextAlignment.Center,
                ParagraphAlignment = DWriteParaAlignment.Center,
                Size = new(500, 100),
            };
            _buttonAbout = new QuestionMarkButton();
            _labelCopyr = new LabelControl
            {
                Text = "© 2012-2020 Mike Murphy (mike@emu7800.net)",
                TextFontFamilyName = Styles.SmallFontFamily,
                TextFontSize = Styles.SmallFontSize,
                TextAlignment = DWriteTextAlignment.Center,
                ParagraphAlignment = DWriteParaAlignment.Center,
                Size = new(500, 20)
            };

            _labelVers = new LabelControl
            {
                Text = GetVersionInfo(),
                TextFontFamilyName = Styles.SmallFontFamily,
                TextFontSize = Styles.SmallFontSize,
                TextAlignment = DWriteTextAlignment.Center,
                ParagraphAlignment = DWriteParaAlignment.Center,
                Size = new(500, 20)
            };
            Controls.Add(_titleControl, _buttonPlayAtariToday, _labelBusyInit, _buttonAbout, _labelCopyr, _labelVers);

            _buttonPlayAtariToday.Clicked += ButtonPlayAtariToday_Clicked;
            _buttonAbout.Clicked += ButtonAbout_Clicked;
        }

        #region PageBase Overrides

        public override void OnNavigatingHere()
        {
            base.OnNavigatingHere();

            if (_isImportCheckStarted)
                return;
            _isImportCheckStarted = true;

            ImportCheckAsync();
        }

        public override void Resized(D2D_SIZE_F size)
        {
            D2D_POINT_2F centerPt = new(
                size.Width / 2,
                size.Height / 2
                );
            D2D_POINT_2F centerPtPlayButton = new(
                _buttonPlayAtariToday.Size.Width / 2,
                _buttonPlayAtariToday.Size.Height / 2
                );
            _titleControl.Location = new(
                centerPt.X - (_titleControl.Size.Width / 2),
                centerPt.Y / 5
                );
            _buttonPlayAtariToday.Location = new(
                centerPt.X - centerPtPlayButton.X,
                centerPt.Y - centerPtPlayButton.Y
                );
            _labelBusyInit.Location = _buttonPlayAtariToday.Location;

            _labelCopyr.Location = new(
                size.Width / 2 - _labelCopyr.Size.Width / 2,
                _buttonPlayAtariToday.Location.Y + _buttonPlayAtariToday.Size.Height + 20
                );
            _labelVers.Location = new(
                size.Width / 2 - _labelVers.Size.Width / 2,
                _labelCopyr.Location.Y + _labelCopyr.Size.Height
                );

            _buttonAbout.Location  = new(
                size.Width / 2 - _buttonAbout.Size.Width / 2,
                size.Height - _buttonAbout.Size.Height - 50
                );
        }

        public override void ControllerButtonChanged(int controllerNo, MachineInput input, bool down)
        {
            base.ControllerButtonChanged(controllerNo, input, down);
            if (down)
                return;
            switch (input)
            {
                case MachineInput.Start:
                case MachineInput.Fire:
                case MachineInput.Fire2:
                    ButtonPlayAtariToday_Clicked(this, new());
                    break;
            }
        }

        #endregion

        #region Event Handlers

        void ButtonPlayAtariToday_Clicked(object? sender, EventArgs eventArgs)
        {
            PushPage(new GameProgramSelectionPage());
        }

        void ButtonAbout_Clicked(object? sender, EventArgs eventArgs)
        {
            PushPage(new AboutPage());
        }

        #endregion

        #region Helpers

        async void ImportCheckAsync()
        {
            _buttonPlayAtariToday.IsVisible = false;
            _buttonAbout.IsVisible = false;
            _labelBusyInit.IsVisible = true;

            await Task.Run(() => ImportCheck());
            await Task.Run(() => SettingsCheck());

            _buttonPlayAtariToday.IsVisible = true;
            _buttonAbout.IsVisible = true;
            _labelBusyInit.IsVisible = false;
        }

        static void ImportCheck()
        {
            RomImportService.ImportDefaultsIfNecessary();
        }

        static void SettingsCheck()
        {
            DatastoreService.GetSettings();
        }

        static string GetVersionInfo()
        {
            var ea = System.Reflection.Assembly.GetExecutingAssembly();
            var name = ea.GetName();
            var version = name.Version;
            return $"Version {version?.Major ?? 0}.{version?.Minor ?? 0}.{version?.MajorRevision ?? 0}.{version?.MinorRevision} {GetBuildConfiguration()}";
        }

        static string GetBuildConfiguration()
        {
#if DEBUG
            return "DEBUG";
#elif PROFILE
            return "PROFILE";
#else
            return string.Empty;
#endif
        }

        #endregion
    }
}
