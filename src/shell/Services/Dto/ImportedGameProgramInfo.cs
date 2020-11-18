// © Mike Murphy

using System;
using System.Collections.Generic;

namespace EMU7800.Services.Dto
{
    public record ImportedGameProgramInfo
    {
        public GameProgramInfo GameProgramInfo { get; init; } = new();
        public ISet<string> StorageKeySet { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public bool PersistedStateExists { get; set; }
    }
}