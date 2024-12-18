﻿// © Mike Murphy

using System;
using System.Linq;
using EMU7800.Core;
using EMU7800.Win32.Interop;
using EMU7800.Services;
using EMU7800.Services.Dto;

namespace EMU7800.D2D.Shell;

public sealed class GamePage : PageBase
{
    #region Fields

    readonly ApplicationSettings _settings;
    readonly GameProgramInfoViewItem _gameProgramInfoViewItem;
    readonly GameControl _gameControl;
    readonly ButtonBase _buttonBack, _buttonSettings;
    readonly LabelControl _labelInfoText;

    const int HudButtonWidth = 80, HudButtonHeight = 50, HudStartY = 50, HudGapX = 25;
    readonly ButtonToggle _hud_buttonPower, _hud_buttonLD, _hud_buttonRD, _hud_buttonSound, _hud_buttonPaused, _hud_buttonAntiAliasMode, _hud_buttonShowTouchControls;
    readonly ButtonBase _hud_buttonColor, _hud_buttonSelect, _hud_buttonReset, _hud_buttonClose, _hud_buttonFpsMinus, _hud_buttonFpsPlus, _hud_buttonInput;
    readonly NumberControl _hud_numbercontrolFPS;
    readonly LabelControl _hud_labelFPS, _hud_controllers;
    readonly ControlCollection _hud_controlCollection;

#if PROFILE
    readonly NumberControl _numbercontrolRefreshRate;
#endif

    readonly ButtonTouchControl _touchbuttonLeft, _touchbuttonRight, _touchbuttonUp, _touchbuttonDown, _touchbuttonFire, _touchbuttonFire2;
    readonly ControlCollection _touchbuttonCollection;

    float _infoTextVisibilityTimer, _fpsChangeTimer;
    int _fpsChangeDirection, _hudPlayerInputNo;

    bool _isTooNarrowForHud, _isHudOn;
    bool _isAlreadyNavigatedHere, _isAlreadyNavigatedAway = true;
    readonly bool _startFreshReq;

    int _backAndSettingsButtonVisibilityCounter;

    D2D_SIZE_F _lastResize;

    #endregion

    public GamePage(GameProgramInfoViewItem gameProgramInfoViewItem, bool startFresh = false)
    {
        _gameProgramInfoViewItem = gameProgramInfoViewItem ?? throw new ArgumentNullException(nameof(gameProgramInfoViewItem));
        _startFreshReq = startFresh;

        _settings = DatastoreService.GetSettings();

        _gameControl = new GameControl();
        _buttonBack = new BackButton
        {
            Location = new(5, 5)
        };
        _buttonSettings = new SettingsButton
        {
             Location = _buttonBack.ToRightOf(25, 0)
        };
        _labelInfoText = new LabelControl
        {
            TextFontFamilyName = Styles.ExtraLargeFontFamily,
            TextFontSize = Styles.ExtraLargeFontSize,
            TextAlignment = DWriteTextAlignment.Center,
            Text = string.Empty,
            IsVisible = false
        };

        _hud_buttonPower = new ButtonToggle
        {
            Text = "Power",
            Size = new(HudButtonWidth, HudButtonHeight),
            IsChecked = true
        };
        _hud_buttonColor = new Button
        {
            Text = "Color",
            Size = new(HudButtonWidth, HudButtonHeight)
        };
        _hud_buttonLD = new ButtonToggle
        {
            Text = "A/b",
            Size = new(HudButtonWidth, HudButtonHeight)
        };
        _hud_buttonRD = new ButtonToggle
        {
            Text = "A/b",
            Size = new(HudButtonWidth, HudButtonHeight)
        };
        _hud_buttonSelect = new Button
        {
            Text = "Select",
            Size = new(HudButtonWidth, HudButtonHeight)
        };
        _hud_buttonReset = new Button
        {
            Text = "Reset",
            Size = new(HudButtonWidth, HudButtonHeight)
        };
        _hud_buttonSound = new ButtonToggle
        {
            Text = "Sound",
            Size = new(HudButtonWidth, HudButtonHeight)
        };
        _hud_buttonPaused = new ButtonToggle
        {
            Text = "Paused",
            Size = new(HudButtonWidth, HudButtonHeight)
        };
        _hud_buttonAntiAliasMode = new ButtonToggle
        {
            Text = "Fuzzy",
            Size = new(HudButtonWidth, HudButtonHeight)
        };
        _hud_buttonClose = new CheckButton();
        _hud_numbercontrolFPS = new()
        {
            TextFontFamilyName = Styles.ExtraLargeFontFamily,
            TextFontSize = Styles.ExtraLargeFontSize
        };
        _hud_labelFPS = new()
        {
            TextFontFamilyName = Styles.ExtraLargeFontFamily,
            TextFontSize = Styles.ExtraLargeFontSize,
            Text = "FPS",
            Size = new(100, 50)
        };
        _hud_controllers = new()
        {
            TextFontFamilyName = Styles.SmallFontFamily,
            TextFontSize = Styles.SmallFontSize,
            TextAlignment = DWriteTextAlignment.Center,
            Text = string.Empty
        };
        _hud_buttonFpsMinus = new MinusButton();
        _hud_buttonFpsPlus = new PlusButton();
        _hud_buttonInput = new Button
        {
            Text = "Input",
            Size = new(HudButtonWidth, HudButtonHeight)
        };
        _hud_buttonShowTouchControls = new ButtonToggle
         {
             Text = "Touch Controllers",
             Size = new(2 * HudButtonWidth + HudGapX, HudButtonHeight)
         };

        _hud_controlCollection = new ControlCollection();
        _hud_controlCollection.Add(_hud_buttonPower, _hud_buttonColor, _hud_buttonLD, _hud_buttonRD, _hud_buttonSelect,
            _hud_buttonReset, _hud_buttonClose, _hud_buttonSound, _hud_buttonPaused, _hud_numbercontrolFPS, _hud_labelFPS, _hud_controllers,
            _hud_buttonAntiAliasMode, _hud_buttonFpsMinus, _hud_buttonFpsPlus, _hud_buttonShowTouchControls, _hud_buttonInput);
        _hud_controlCollection.IsVisible = false;

        _touchbuttonLeft = new LeftButton { ExpandBoundingRectangleVertically = true };
        _touchbuttonRight = new RightButton { ExpandBoundingRectangleVertically = true };
        _touchbuttonUp = new UpButton { ExpandBoundingRectangleHorizontally = true };
        _touchbuttonDown = new DownButton { ExpandBoundingRectangleHorizontally = true };
        _touchbuttonFire = new FireButton();
        _touchbuttonFire2 = new FireButton();
        _touchbuttonCollection = new ControlCollection();
        _touchbuttonCollection.Add(_touchbuttonLeft, _touchbuttonRight, _touchbuttonUp, _touchbuttonDown, _touchbuttonFire, _touchbuttonFire2);

        _gameControl.IsInTouchMode = _touchbuttonCollection.IsVisible = _settings.ShowTouchControls;

        Controls.Add(_gameControl, _labelInfoText, _hud_controlCollection, _touchbuttonCollection);

        if (!_startFreshReq)
            Controls.Add(_buttonBack, _buttonSettings);

        ResetBackAndSettingsButtonVisibilityCounter();

#if PROFILE
        _numbercontrolRefreshRate = new NumberControl { Radix = 1, UseComma = false };
        Controls.Add(_numbercontrolRefreshRate);
#endif

        _buttonBack.Clicked += ButtonBack_Clicked;
        _buttonSettings.Clicked += ButtonSettings_Clicked;

        _hud_buttonClose.Clicked += (s, e) => HideHud();
        _hud_buttonSound.Checked += (s, e) => _gameControl.IsSoundOn = true;
        _hud_buttonSound.Unchecked += (s, e) => _gameControl.IsSoundOn = false;
        _hud_buttonPaused.Checked += (s, e) => _gameControl.IsPaused = true;
        _hud_buttonPaused.Unchecked += (s, e) => _gameControl.IsPaused = false;
        _hud_buttonAntiAliasMode.Checked += (s, e) => _gameControl.IsAntiAliasOn = true;
        _hud_buttonAntiAliasMode.Unchecked += (s, e) => _gameControl.IsAntiAliasOn = false;
        _hud_buttonShowTouchControls.Checked += (s, e) => HandleShowTouchControlsCheckedChanged(true);
        _hud_buttonShowTouchControls.Unchecked += (s, e) => HandleShowTouchControlsCheckedChanged(false);
        _hud_buttonInput.Clicked += Hud_buttonInput_Clicked;

        _hud_buttonPower.Checked += Hud_buttonPower_Checked;
        _hud_buttonPower.Unchecked += Hud_buttonPower_Unchecked;
        _hud_buttonColor.Pressed += (s, e) => RaiseMachineInputFromHud(MachineInput.Color, true);
        _hud_buttonColor.Released += (s, e) => RaiseMachineInputFromHud(MachineInput.Color, false);
        _hud_buttonLD.Pressed += (s, e) => RaiseMachineInputFromHud(MachineInput.LeftDifficulty, true);
        _hud_buttonLD.Released += (s, e) => RaiseMachineInputFromHud(MachineInput.LeftDifficulty, false);
        _hud_buttonRD.Pressed += (s, e) => RaiseMachineInputFromHud(MachineInput.RightDifficulty, true);
        _hud_buttonRD.Released += (s, e) => RaiseMachineInputFromHud(MachineInput.RightDifficulty, false);
        _hud_buttonSelect.Pressed += (s, e) => RaiseMachineInputFromHud(MachineInput.Select, true);
        _hud_buttonSelect.Released += (s, e) => RaiseMachineInputFromHud(MachineInput.Select, false);
        _hud_buttonReset.Pressed += (s, e) => RaiseMachineInputFromHud(MachineInput.Reset, true);
        _hud_buttonReset.Released += (s, e) => RaiseMachineInputFromHud(MachineInput.Reset, false);

        _touchbuttonLeft.Pressed += (s, e) => RaiseKeyboardKeyPressed(KeyboardKey.Left, true);
        _touchbuttonLeft.Released += (s, e) => RaiseKeyboardKeyPressed(KeyboardKey.Left, false);
        _touchbuttonRight.Pressed += (s, e) => RaiseKeyboardKeyPressed(KeyboardKey.Right, true);
        _touchbuttonRight.Released += (s, e) => RaiseKeyboardKeyPressed(KeyboardKey.Right, false);
        _touchbuttonUp.Pressed += (s, e) => RaiseKeyboardKeyPressed(KeyboardKey.Up, true);
        _touchbuttonUp.Released += (s, e) => RaiseKeyboardKeyPressed(KeyboardKey.Up, false);
        _touchbuttonDown.Pressed += (s, e) => RaiseKeyboardKeyPressed(KeyboardKey.Down, true);
        _touchbuttonDown.Released += (s, e) => RaiseKeyboardKeyPressed(KeyboardKey.Down, false);
        _touchbuttonFire.Pressed += (s, e) => RaiseKeyboardKeyPressed(KeyboardKey.Z, true);
        _touchbuttonFire.Released += (s, e) => RaiseKeyboardKeyPressed(KeyboardKey.Z, false);
        _touchbuttonFire2.Pressed += (s, e) => RaiseKeyboardKeyPressed(KeyboardKey.X, true);
        _touchbuttonFire2.Released += (s, e) => RaiseKeyboardKeyPressed(KeyboardKey.X, false);
    }

    #region PageBase Overrides

    public override void OnNavigatingHere()
    {
        base.OnNavigatingHere();

        if (_isAlreadyNavigatedHere)
            return;
        _isAlreadyNavigatedHere = true;
        _isAlreadyNavigatedAway = false;

        ResetBackAndSettingsButtonVisibilityCounter();

        if (!_hud_buttonPower.IsChecked)
        {
            _hud_buttonPower.IsChecked = true;
            _hud_buttonPaused.IsChecked = _gameControl.IsPaused = false;
        }

        _gameControl.Start(_gameProgramInfoViewItem.ImportedGameProgramInfo, _startFreshReq);

        _gameProgramInfoViewItem.ImportedGameProgramInfo.PersistedStateAt = DateTime.UtcNow;
    }

    public override void OnNavigatingAway()
    {
        base.OnNavigatingAway();

        if (_isAlreadyNavigatedAway)
            return;
        _isAlreadyNavigatedAway = true;
        _isAlreadyNavigatedHere = false;

        DatastoreService.SaveSettings(_settings);
        _gameControl.Stop();

        HideHud();

        _buttonBack.IsVisible = !_isTooNarrowForHud;
        _buttonSettings.IsVisible = !_isTooNarrowForHud;
    }

    public override void Resized(D2D_SIZE_F size)
    {
        base.Resized(size);

        _lastResize = size;

        var mult = Math.Min(size.Width / 4, size.Height / 3);
        var screenWidth  = 4 * mult - 5;
        var screenHeight = 3 * mult - 5;

        var x = size.Width / 2 - screenWidth / 2;
        var y = size.Height / 2 - screenHeight / 2;

        _gameControl.Location = new(x, y);
        _gameControl.Size = new(screenWidth, screenHeight);

        _hud_buttonClose.Location = new(
            size.Width / 2 - _hud_buttonClose.Size.Width / 2,
            size.Height - 5 - _hud_buttonClose.Size.Height
            );

        _labelInfoText.Location = new(0, size.Height / 2);
        _labelInfoText.Size = new(size.Width, 200);

        var hudx = size.Width / 2.0f - (6.0f * HudButtonWidth + 5.0f * HudGapX + HudGapX) / 2.0f;

        _hud_buttonPower.Location = new(hudx, HudStartY);
        _hud_buttonColor.Location = _hud_buttonPower.ToRightOf(HudGapX, 0);
        _hud_buttonLD.Location = _hud_buttonColor.ToRightOf(HudGapX, 0);
        _hud_buttonRD.Location = _hud_buttonLD.ToRightOf(2 * HudGapX, 0);
        _hud_buttonSelect.Location = _hud_buttonRD.ToRightOf(HudGapX, 0);
        _hud_buttonReset.Location = _hud_buttonSelect.ToRightOf(HudGapX, 0);

        _hud_buttonSound.Location = new(hudx, 3 * HudStartY);
        _hud_buttonPaused.Location = _hud_buttonSound.ToRightOf(HudGapX, 0);
        _hud_buttonAntiAliasMode.Location = _hud_buttonPaused.ToRightOf(HudGapX, 0);

        _hud_buttonShowTouchControls.Location = _hud_buttonAntiAliasMode.ToRightOf(2 * HudGapX, 0);
        _hud_buttonInput.Location = new(_hud_buttonReset.Location.X, 3 * HudStartY);

        var fpsControlsWidth = 3*_hud_buttonFpsMinus.Size.Width + 3*_hud_buttonFpsPlus.Size.Width;
        _hud_buttonFpsMinus.Location = new(size.Width/2 - fpsControlsWidth/2, 5 * HudStartY);
        _hud_labelFPS.Location = _hud_buttonFpsMinus.ToRightOf(50, 0);
        _hud_numbercontrolFPS.Location = _hud_labelFPS.ToRightOf(-5, 0);
        _hud_buttonFpsPlus.Location = new(size.Width / 2 - fpsControlsWidth / 2 + fpsControlsWidth - _hud_buttonFpsPlus.Size.Width, 5 * HudStartY);

        _hud_controllers.Location = new(0, _hud_buttonClose.Location.Y - 50);
        _hud_controllers.Size = new(size.Width, 50);

        var touchWidth = (int)_touchbuttonLeft.Size.Width;
        var touchY = (int)size.Height / 2 - 3 * touchWidth / 2;
        var separation = _settings.TouchControlSeparation;

        // WP8.1 introduced a notification window activated by swiping from the edge.
        // WinX mobile can have navigation buttons occlude a margin on the right.
        // So, keep the touch buttons off of the left and right edge of the screen.
        const int LEFTG = 35, RIGHTG = 35;

        _touchbuttonUp.Location    = new(LEFTG + touchWidth + separation, touchY - separation);
        _touchbuttonLeft.Location  = new(LEFTG + 0, touchY + touchWidth);
        _touchbuttonRight.Location = new(LEFTG + 2 * touchWidth + 2 * separation, touchY + touchWidth);
        _touchbuttonDown.Location  = new(LEFTG + touchWidth + separation, touchY + 2 * touchWidth + separation);
        _touchbuttonFire.Location  = new(size.Width - 2 * touchWidth - separation - RIGHTG, touchY + touchWidth + separation);
        _touchbuttonFire2.Location = new(size.Width - touchWidth - RIGHTG, touchY);

        _isTooNarrowForHud = size.Width < 330f;

        if (_isHudOn && _isTooNarrowForHud)
            HideHud();

        _touchbuttonCollection.IsVisible = !_isTooNarrowForHud && _settings.ShowTouchControls;
        _hud_controlCollection.IsVisible = !_isTooNarrowForHud && _isHudOn;
        _buttonBack.IsVisible = !_isTooNarrowForHud;
        _buttonSettings.IsVisible = !_isTooNarrowForHud;

#if PROFILE
        _frameIdleRect = new(Struct.ToPointF(5, size.Height / 2 - 25), Struct.ToSizeF(25, size.Height / 2));
        _buffersQueuedRect = new(Struct.ToPointF(35, size.Height - 50), Struct.ToSizeF(25, 25));
        _numbercontrolRefreshRate.Location = new(size.Width, size.Height - 50);
#endif
    }

    public override void KeyboardKeyPressed(KeyboardKey key, bool down)
    {
        base.KeyboardKeyPressed(key, down);

        switch (key)
        {
            case KeyboardKey.Escape:
                if (down)
                    return;
                if (_isHudOn)
                    HideHud();
                else
                    PopPage();
                break;
            case KeyboardKey.H:
                if (down)
                    return;
                if (_isHudOn)
                    HideHud();
                else if (!_isTooNarrowForHud)
                    ShowHud();
                break;
            case KeyboardKey.R:
                _gameControl.RaiseMachineInput(MachineInput.Reset, down);
                break;
            case KeyboardKey.S:
                _gameControl.RaiseMachineInput(MachineInput.Select, down);
                break;
            case KeyboardKey.P:
                _gameControl.RaiseMachineInput(MachineInput.Pause, down);
                break;
            case KeyboardKey.F1:
                if (down)
                    return;
                ChangeCurrentKeyboardPlayerNo(1);
                break;
            case KeyboardKey.F2:
                if (down)
                    return;
                ChangeCurrentKeyboardPlayerNo(2);
                break;
            case KeyboardKey.F3:
                if (down)
                    return;
                ChangeCurrentKeyboardPlayerNo(3);
                break;
            case KeyboardKey.F4:
                if (down)
                    return;
                ChangeCurrentKeyboardPlayerNo(4);
                break;
            case KeyboardKey.Q:
                if (down)
                    return;
                var swapped = _gameControl.SwapLeftControllerPaddles();
                PostInfoText($"P1/P2 paddles {(swapped ? "" : "un")}swapped");
                break;
            case KeyboardKey.W:
                if (down)
                    return;
                swapped = _gameControl.SwapJacks();
                PostInfoText($"Input jacks {(swapped ? "" : "un")}swapped");
                break;
            case KeyboardKey.E:
                if (down)
                    return;
                swapped = _gameControl.SwapRightControllerPaddles();
                PostInfoText($"P3/P4 paddles {(swapped ? "" : "un")}swapped");
                break;
            case KeyboardKey.PageUp:
                if (!_hud_buttonPower.IsChecked)
                    PowerOn();
                break;
            case KeyboardKey.PageDown:
                if (_hud_buttonPower.IsChecked)
                    PowerOff();
                break;
        }
    }

    public override void MouseMoved(int pointerId, int x, int y, int dx, int dy)
    {
        ResetBackAndSettingsButtonVisibilityCounter();
        base.MouseMoved(pointerId, x, y, dx, dy);
    }

    public override void MouseButtonChanged(int pointerId, int x, int y, bool down)
    {
        ResetBackAndSettingsButtonVisibilityCounter();
        if (_isHudOn && down && _settings.ShowTouchControls && y >= _touchbuttonUp.Location.Y && y <= _touchbuttonDown.Location.Y)
        {
            _settings.TouchControlSeparation += 5;
            Resized(_lastResize);
        }
        base.MouseButtonChanged(pointerId, x, y, down);
    }

    public override void ControllerButtonChanged(int controllerNo, MachineInput input, bool down)
    {
        base.ControllerButtonChanged(controllerNo, input, down);
        switch (input)
        {
            case MachineInput.Select:
                KeyboardKeyPressed(KeyboardKey.S, down);
                break;
            case MachineInput.Reset:
                KeyboardKeyPressed(KeyboardKey.R, down);
                break;
            case MachineInput.End:
                KeyboardKeyPressed(KeyboardKey.Escape, down);
                break;
        }
    }

    public override void Update(TimerDevice td)
    {
        base.Update(td);
#if PROFILE
        if (!_isHudOn)
            _numbercontrolRefreshRate.Value = (int)(10.0f / td.DeltaInSeconds);
#endif
        if (_backAndSettingsButtonVisibilityCounter > 0 && --_backAndSettingsButtonVisibilityCounter == 0)
        {
            _buttonBack.IsVisible = false;
            _buttonSettings.IsVisible = false;
        }

        _labelInfoText.IsVisible = _infoTextVisibilityTimer > 0;
        if (_infoTextVisibilityTimer > 0)
            _infoTextVisibilityTimer -= td.DeltaInSeconds;

        if (_hud_buttonFpsMinus.IsPressed)
            _fpsChangeDirection = -1;
        else if (_hud_buttonFpsPlus.IsPressed)
            _fpsChangeDirection = 1;
        else
            _fpsChangeDirection = 0;

        if (_hud_controlCollection.IsVisible)
        {
            _hud_numbercontrolFPS.Value = _gameControl.CurrentFrameRate;
            if (_fpsChangeDirection == 0)
            {
                _fpsChangeTimer = 0;
            }
            else
            {
                _fpsChangeTimer -= td.DeltaInSeconds;
                if (_fpsChangeTimer < 0)
                {
                    _fpsChangeTimer += 0.25f;
                    _gameControl.ProposeNewFrameRate(_hud_numbercontrolFPS.Value + _fpsChangeDirection);
                }
            }
        }
    }

#if PROFILE
    D2D_RECT_F _frameIdleRect;
    D2D_RECT_F _buffersQueuedRect;
    public override void Render()
    {
        base.Render();

        var frameIdleTime = _gameControl.FrameIdleTime;
        var buffersQueued = _gameControl.BuffersQueued;

        GraphicsDevice.DrawRectangle(_frameIdleRect, 1.0f, D2DSolidColorBrush.White);
        var rect = _frameIdleRect;
        rect.Top = rect.Bottom - (rect.Bottom - rect.Top) * frameIdleTime;
        rect.Left += 5;
        rect.Right -= 5;
        GraphicsDevice.FillRectangle(rect, D2DSolidColorBrush.White);

        rect = _buffersQueuedRect;
        for (var i = 0; i < buffersQueued; i++)
        {
            GraphicsDevice.FillRectangle(rect, D2DSolidColorBrush.White);
            rect.Top -= 30;
            rect.Bottom -= 30;
        }
    }
#endif

    #endregion

    #region Event Handlers

    void ButtonBack_Clicked(object? sender, EventArgs e)
    {
        if (_isHudOn)
            HideHud();
        else
            PopPage();
    }

    void ButtonSettings_Clicked(object? sender, EventArgs e)
    {
        if (_isHudOn)
            HideHud();
        else
            ShowHud();
    }

    void Hud_buttonPower_Checked(object? sender, EventArgs e)
    {
        PowerOn();
    }

    void Hud_buttonPower_Unchecked(object? sender, EventArgs e)
    {
        PowerOff();
    }

    void Hud_buttonInput_Clicked(object? sender, EventArgs e)
    {
        var key = new[] { KeyboardKey.F1, KeyboardKey.F2, KeyboardKey.F3, KeyboardKey.F4 }[++_hudPlayerInputNo & 3];
        KeyboardKeyPressed(key, false);
    }

    #endregion

    #region Helpers

    void PowerOn()
    {
        _gameControl.Stop();
        _hud_buttonPaused.IsChecked = _gameControl.IsPaused = false;
        _gameControl.Start(_gameProgramInfoViewItem.ImportedGameProgramInfo, true);
        _gameProgramInfoViewItem.ImportedGameProgramInfo.PersistedStateAt = DateTime.UtcNow;
        _hud_buttonPower.IsChecked = true;
    }

    void PowerOff()
    {
        _gameControl.Stop();
        _gameControl.StartSnow();
        _gameProgramInfoViewItem.ImportedGameProgramInfo.PersistedStateAt = DateTime.MinValue;
        _hud_buttonPower.IsChecked = false;
        DatastoreService.PurgePersistedMachine(_gameProgramInfoViewItem.ImportedGameProgramInfo.GameProgramInfo);
    }

    void ShowHud()
    {
        _gameControl.SwitchToDarkerPalette();
        _hud_controlCollection.IsVisible = true;
        _buttonBack.IsVisible = false;
        _buttonSettings.IsVisible = false;
        _hud_buttonLD.IsChecked = _gameControl.IsLeftDifficultyAConsoleSwitchSet;
        _hud_buttonRD.IsChecked = _gameControl.IsRightDifficultyAConsoleSwitchSet;
        _hud_buttonSound.IsChecked = _gameControl.IsSoundOn;
        _hud_buttonPaused.IsChecked = _gameControl.IsPaused = true;
        _hud_buttonAntiAliasMode.IsChecked = _gameControl.IsAntiAliasOn;
        _hud_controllers.Text = BuildControllersTextForHud();
        _hud_buttonShowTouchControls.IsChecked = _settings.ShowTouchControls;
        _isHudOn = true;
    }

    void HideHud()
    {
        _gameControl.SwitchToNormalPalette();
        _hud_controlCollection.IsVisible = false;
        _buttonBack.IsVisible = true;
        _buttonSettings.IsVisible = true;
        _hud_buttonPaused.IsChecked = _gameControl.IsPaused = false;
        _isHudOn = false;
        ResetBackAndSettingsButtonVisibilityCounter();
    }

    void RaiseMachineInputFromHud(MachineInput machineInput, bool down)
    {
        _hud_buttonPaused.IsChecked = _gameControl.IsPaused = false;
        _gameControl.RaiseMachineInput(machineInput, down);
    }

    void RaiseKeyboardKeyPressed(KeyboardKey key, bool down)
    {
        _gameControl.KeyboardKeyPressed(key, down);
    }

    void HandleShowTouchControlsCheckedChanged(bool isChecked)
    {
        _settings.ShowTouchControls = _touchbuttonCollection.IsVisible = _gameControl.IsInTouchMode = isChecked;
        if (!isChecked)
            _settings.TouchControlSeparation = 0;
        Resized(_lastResize);
    }

    void ChangeCurrentKeyboardPlayerNo(int newPlayerNo)
    {
        PostInfoText("Input to P" + newPlayerNo);
        _gameControl.ChangeCurrentKeyboardPlayerNo(newPlayerNo - 1);
    }

    void PostInfoText(string text)
    {
        _labelInfoText.Text = text;
        _infoTextVisibilityTimer = 2.0f;
    }

    static string BuildControllersTextForHud()
        => string.Join("; ", Enumerable.Range(0, GameControllers.Controllers.Length)
            .Select(i => new
            {
                P = i + 1,
                C = GameControllers.Controllers[i].Info
            })
            .Where(r => !string.IsNullOrWhiteSpace(r.C))
            .Select(r => $"P{r.P}: {r.C}"));

    void ResetBackAndSettingsButtonVisibilityCounter()
    {
        _backAndSettingsButtonVisibilityCounter = 600;
        _buttonBack.IsVisible = true;
        _buttonSettings.IsVisible = true;
    }

    #endregion
}