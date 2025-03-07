﻿// © Mike Murphy

using EMU7800.Assets;
using EMU7800.Win32.Interop;
using System;
using System.Threading.Tasks;

namespace EMU7800.D2D.Shell;

public sealed class AboutPage : PageBase
{
    readonly ButtonBase _buttonBack;
    readonly TextControl _textcontrolAbout;

    public AboutPage()
    {
        _buttonBack = new BackButton
        {
            Location = new(60, 5)
        };
        _textcontrolAbout = new TextControl
        {
            TextFontFamilyName = Styles.NormalFontFamily,
            TextFontSize = Styles.NormalFontSize,
            Location = new(60, 60)
        };
        Controls.Add(_buttonBack, _textcontrolAbout);

       _buttonBack.Clicked += ButtonBack_Clicked;

        GetAboutTextAsync();
    }

    #region PageBase Overrides

    public override void KeyboardKeyPressed(KeyboardKey key, bool down)
    {
        base.KeyboardKeyPressed(key, down);
        if (!down)
            return;
        switch (key)
        {
            case KeyboardKey.Down:
                _textcontrolAbout.ScrollDownByIncrement(1);
                break;
            case KeyboardKey.PageDown:
                _textcontrolAbout.ScrollDownByIncrement(10);
                break;
            case KeyboardKey.Up:
                _textcontrolAbout.ScrollUpByIncrement(1);
                break;
            case KeyboardKey.PageUp:
                _textcontrolAbout.ScrollUpByIncrement(10);
                break;
            case KeyboardKey.Escape:
                PopPage();
                break;
        }
    }

    public override void Resized(D2D_SIZE_F size)
    {
        _textcontrolAbout.Size = new(
            size.Width - _textcontrolAbout.Location.X * 2,
            size.Height - _textcontrolAbout.Location.Y * 2
            );
    }

    #endregion

    #region Event Handlers

    static void ButtonBack_Clicked(object? sender, EventArgs eventArgs)
    {
        PopPage();
    }

    #endregion

    #region Helpers

    async void GetAboutTextAsync()
    {
        var text = await GetTextAssetAsync(Asset.about);
        _textcontrolAbout.Text = text;
    }

    static async Task<string> GetTextAssetAsync(Asset textAsset)
    {
        var bytes = await AssetService.GetAssetBytesAsync(textAsset);
        return System.Text.Encoding.UTF8.GetString(bytes, 0, bytes.Length);
    }

    #endregion
}