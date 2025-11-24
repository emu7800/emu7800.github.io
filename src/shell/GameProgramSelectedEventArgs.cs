// © Mike Murphy

using EMU7800.Services.Dto;
using System;

namespace EMU7800.Shell;

public class GameProgramSelectedEventArgs(GameProgramInfoViewItem gpivi) : EventArgs
{
    public GameProgramInfoViewItem GameProgramInfoViewItem { get; } = gpivi;
}