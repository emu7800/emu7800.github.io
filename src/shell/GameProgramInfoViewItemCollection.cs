// © Mike Murphy

using System;
using System.Collections.Generic;

namespace EMU7800.Services.Dto
{
    public record GameProgramInfoViewItemCollection
    {
        public string Name { get; init; } = string.Empty;
        public IEnumerable<GameProgramInfoViewItem> GameProgramInfoViewItemSet { get; init; } = Array.Empty<GameProgramInfoViewItem>();
    }
}
