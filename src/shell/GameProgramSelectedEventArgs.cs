// © Mike Murphy

using System;
using EMU7800.Services.Dto;

namespace EMU7800.D2D.Shell
{
    public class GameProgramSelectedEventArgs : EventArgs
    {
        public GameProgramInfoViewItem GameProgramInfoViewItem { get; }

        public GameProgramSelectedEventArgs(GameProgramInfoViewItem gpivi)
            => GameProgramInfoViewItem = gpivi;
    }
}