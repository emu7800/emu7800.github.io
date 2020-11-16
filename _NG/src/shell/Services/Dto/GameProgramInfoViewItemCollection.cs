// © Mike Murphy

using System;
using System.Collections.Generic;

namespace EMU7800.Services.Dto
{
    public class GameProgramInfoViewItemCollection
    {
        public string Name { get; set; } = string.Empty;
        public IEnumerable<GameProgramInfoViewItem> GameProgramInfoViewItemSet { get; set; } = Array.Empty<GameProgramInfoViewItem>();
    }
}
