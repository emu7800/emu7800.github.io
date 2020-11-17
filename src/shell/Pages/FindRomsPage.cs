// © Mike Murphy

using System;
using System.Threading.Tasks;
using EMU7800.D2D.Interop;
using EMU7800.Services;
using EMU7800.Services.Dto;

namespace EMU7800.D2D.Shell
{
    public sealed class FindRomsPage : PageBase
    {
        #region Fields

        readonly ButtonBase _backButton, _nextButton;
        readonly TextControl _findRomsTextControl;

        #endregion

        public FindRomsPage()
        {
            _backButton = new BackButton
            {
                Location = Struct.ToPointF(5, 5)
            };
            _nextButton = new NextButton
            {
                Location = Struct.ToPointF(_backButton.Location.X + _backButton.Size.Width + 25, _backButton.Location.Y)
            };
            _findRomsTextControl = new TextControl
            {
                Location = Struct.ToPointF(60, _backButton.Location.Y + _backButton.Size.Height + 25),
                TextFontFamilyName = Styles.NormalFontFamily,
                TextFontSize = Styles.NormalFontSize,
            };
            Controls.Add(_backButton, _nextButton, _findRomsTextControl);

            _backButton.Clicked += BackButton_Clicked;
            _nextButton.Clicked += NextButton_Clicked;

            GetTextForFindRomsTextControlAsync();
        }

        #region PageBase Overrides

        public override void Resized(SizeF size)
        {
            _findRomsTextControl.Size = Struct.ToSizeF(
                size.Width - _findRomsTextControl.Location.X * 2,
                size.Height - _findRomsTextControl.Location.Y * 2
                );
        }

        #endregion

        #region Event Handlers

        void BackButton_Clicked(object sender, EventArgs eventArgs)
        {
            PopPage();
        }

        void NextButton_Clicked(object sender, EventArgs eventArgs)
        {
            ReplacePage(new FindRomsPage2());
        }

        #endregion

        #region Helpers

        async void GetTextForFindRomsTextControlAsync()
        {
            var text = await GetTextAssetAsync(Asset.romimport);
            _findRomsTextControl.Text = text;
        }

        static async Task<string> GetTextAssetAsync(Asset textAsset)
        {
            var bytes = await AssetService.GetAssetBytesAsync(textAsset);
            return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }

        #endregion
    }
}
