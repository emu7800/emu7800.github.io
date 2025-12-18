// © Mike Murphy

using System.Collections.Generic;

namespace EMU7800.Services.Dto;

public record GameProgramInfoViewItemCollection(string Name, List<GameProgramInfoViewItem> GameProgramInfoViewItems);
