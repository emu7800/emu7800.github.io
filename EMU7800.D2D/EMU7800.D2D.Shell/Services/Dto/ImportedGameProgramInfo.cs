// © Mike Murphy

using System;
using System.Collections.Generic;

namespace EMU7800.Services.Dto
{
    public class ImportedGameProgramInfo
    {
        public GameProgramInfo GameProgramInfo { get; set; }
        public ISet<string> StorageKeySet { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public bool PersistedStateExists { get; set; }
    }
}