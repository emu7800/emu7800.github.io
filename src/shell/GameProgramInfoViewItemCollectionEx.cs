// © Mike Murphy

using EMU7800.Win32.Interop;
using System;

namespace EMU7800.D2D.Shell
{
    public class GameProgramInfoViewItemCollectionEx
    {
        public string Name { get; set; } = string.Empty;
        public TextLayout NameTextLayout { get; set; } = TextLayout.Default;
        public GameProgramInfoViewItemEx[] GameProgramInfoViewItemSet { get; set; } = Array.Empty<GameProgramInfoViewItemEx>();
    }
}
