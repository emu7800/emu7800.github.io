// © Mike Murphy

using System;
using System.Collections.Generic;

namespace EMU7800.Services.Dto
{
    public class ImportedGameProgramInfo
    {
        public GameProgramInfo GameProgramInfo { get; set; } = new GameProgramInfo();
        public ISet<string> StorageKeySet { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public bool PersistedStateExists { get; set; }
    }
}