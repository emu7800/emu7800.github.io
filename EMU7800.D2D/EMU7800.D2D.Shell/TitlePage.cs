﻿// © Mike Murphy

using System;
using System.Threading.Tasks;
using EMU7800.D2D.Interop;
using EMU7800.Services;

namespace EMU7800.D2D.Shell
{
    public sealed partial class TitlePage : PageBase
    {
        readonly TitleControl _titleControl;
        readonly Button _buttonPlayAtariToday;
        readonly ButtonCircleImage _buttonAbout, _buttonFindRoms;
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
                Size = Struct.ToSizeF(500, 100),
            };
            _labelBusyInit = new LabelControl
            {
                Text = "One Moment...",
                TextFontFamilyName = Styles.LargeFontFamily,
                TextFontSize = Styles.LargeFontSize,
                TextAlignment = DWriteTextAlignment.Center,
                ParagraphAlignment = DWriteParaAlignment.Center,
                Size = Struct.ToSizeF(500, 100),
            };
            _buttonAbout = new QuestionMarkButton();
            _buttonFindRoms = new SearchButton();
            _labelCopyr = new LabelControl
            {
                Text = "© 2012-2013 Mike Murphy (mike@jones-murphy.org)",
                TextFontFamilyName = Styles.SmallFontFamily,
                TextFontSize = Styles.SmallFontSize,
                TextAlignment = DWriteTextAlignment.Center,
                ParagraphAlignment = DWriteParaAlignment.Center,
                Size = Struct.ToSizeF(500, 20)
            };

            _labelVers = new LabelControl
            {
                Text = GetVersionInfo(),
                TextFontFamilyName = Styles.SmallFontFamily,
                TextFontSize = Styles.SmallFontSize,
                TextAlignment = DWriteTextAlignment.Center,
                ParagraphAlignment = DWriteParaAlignment.Center,
                Size = Struct.ToSizeF(500, 20)
            };
            Controls.Add(_titleControl, _buttonPlayAtariToday, _labelBusyInit, _buttonAbout, _buttonFindRoms, _labelCopyr, _labelVers);

            _buttonPlayAtariToday.Clicked += _buttonPlayAtariToday_Clicked;
            _buttonAbout.Clicked += _buttonAbout_Clicked;
            _buttonFindRoms.Clicked += _buttonFindRoms_Clicked;
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

        public override void Resized(SizeF size)
        {
            var centerPt = Struct.ToPointF(
                size.Width / 2,
                size.Height / 2
                );
            var centerPtPlayButton = Struct.ToPointF(
                _buttonPlayAtariToday.Size.Width / 2,
                _buttonPlayAtariToday.Size.Height / 2
                );
            _titleControl.Location = Struct.ToPointF(
                centerPt.X - (_titleControl.Size.Width / 2),
                centerPt.Y / 5
                );
            _buttonPlayAtariToday.Location = Struct.ToPointF(
                centerPt.X - centerPtPlayButton.X,
                centerPt.Y - centerPtPlayButton.Y
                );
            _labelBusyInit.Location = _buttonPlayAtariToday.Location;

            _labelCopyr.Location = Struct.ToPointF(
                size.Width / 2 - _labelCopyr.Size.Width / 2,
                _buttonPlayAtariToday.Location.Y + _buttonPlayAtariToday.Size.Height + 20
                );
            _labelVers.Location = Struct.ToPointF(
                size.Width / 2 - _labelVers.Size.Width / 2,
                _labelCopyr.Location.Y + _labelCopyr.Size.Height
                );

            _buttonAbout.Location  = Struct.ToPointF(
                size.Width / 2 - (_buttonAbout.Size.Width + _buttonFindRoms.Size.Width + 48 + 5) / 2,
                size.Height - _buttonAbout.Size.Height - 5
                );
            _buttonFindRoms.Location = Struct.ToRightOf(_buttonAbout, 48 + 5, 0);
        }

        #endregion

        #region Event Handlers

        void _buttonPlayAtariToday_Clicked(object sender, EventArgs eventArgs)
        {
            PushPage(new GameProgramSelectionPage());
        }

        void _buttonAbout_Clicked(object sender, EventArgs eventArgs)
        {
            PushPage(new AboutPage());
        }

        void _buttonFindRoms_Clicked(object sender, EventArgs eventArgs)
        {
            PushPage(new FindRomsPage());
        }

        #endregion

        #region Helpers

        async void ImportCheckAsync()
        {
            _buttonPlayAtariToday.IsVisible = false;
            _labelBusyInit.IsVisible = true;

            await Task.Run(() => ImportCheck());
            await Task.Run(() => SettingsCheck());

            _buttonPlayAtariToday.IsVisible = true;
            _labelBusyInit.IsVisible = false;
        }

        static void ImportCheck()
        {
            var importService = new RomImportService();
            importService.ImportDefaultsIfNecessary();
        }

        static void SettingsCheck()
        {
            var settingsService = new SettingsService();
            settingsService.GetSettings();
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
