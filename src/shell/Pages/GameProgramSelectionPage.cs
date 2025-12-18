// © Mike Murphy

using EMU7800.Services;
using EMU7800.Services.Dto;
using System;
using System.Collections.Generic;

namespace EMU7800.Shell;

public sealed class GameProgramSelectionPage : PageBase
{
    readonly BackButton _buttonBack;
    readonly GameProgramSelectionControl _gameProgramSelectionControl;
    readonly ImportedRoms _importedRoms;
    readonly List<GameProgramInfoViewItemCollection> _gameProgramViewItems;

    public GameProgramSelectionPage(ImportedRoms importedRoms)
    {
        _importedRoms = importedRoms;
        _gameProgramViewItems = GameProgramLibraryService.GetGameProgramInfoViewItemCollections(_importedRoms.GamePrograms);

        _buttonBack = new()
        {
            Location = new(5, 5)
        };
        LabelControl labelSelectGameProgram = new()
        {
            Text = "Select Game Program",
            TextFontFamilyName = Styles.NormalFontFamily,
            TextFontSize = Styles.NormalFontSize,
            Location = _buttonBack.ToRightOf(25, 12),
            Size = new(400, 200),
            IsVisible = true
        };
        _gameProgramSelectionControl = new(_gameProgramViewItems)
        {
            Location = _buttonBack.ToBottomOf(-5, 5)
        };
        Controls.Add(_buttonBack, labelSelectGameProgram, _gameProgramSelectionControl);

        _buttonBack.Clicked += ButtonBack_Clicked;
        _gameProgramSelectionControl.Selected += GameProgramSelectionControl_Selected;
    }

    #region PageBase Overrides

    public override void OnNavigatingHere(object[] dependencies)
    {
        base.OnNavigatingHere(dependencies);
    }

    public override void Resized(SizeF size)
    {
        _gameProgramSelectionControl.Size = new(size.Width, size.Height - 100);
    }

    public override void KeyboardKeyPressed(KeyboardKey key, bool down)
    {
        base.KeyboardKeyPressed(key, down);
        if (!down)
            return;
        switch (key)
        {
            case KeyboardKey.Escape:
                PopPage();
                break;
        }
    }

    #endregion

    #region Event Handlers

    void GameProgramSelectionControl_Selected(object? sender, GameProgramSelectedEventArgs e)
    {
        var gameProgramInfoViewItem = e.GameProgramInfoViewItem;
        var gamePage = new GamePage(gameProgramInfoViewItem, _importedRoms.SpecialBinaries);
        PushPage(gamePage);
    }

    void ButtonBack_Clicked(object? sender, EventArgs eventArgs)
    {
        PopPage();
    }

    #endregion
}