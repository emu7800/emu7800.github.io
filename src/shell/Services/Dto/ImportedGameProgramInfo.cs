// © Mike Murphy

using System;
using System.Collections.Generic;

namespace EMU7800.Services.Dto
{
    public record ImportedGameProgramInfo
    {
        public GameProgramInfo GameProgramInfo { get; }
        public ISet<string> StorageKeySet { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public bool PersistedStateExists { get; set; }

        public ImportedGameProgramInfo(GameProgramInfo gpi)
        {
            GameProgramInfo = gpi;
        }

        public ImportedGameProgramInfo(GameProgramInfo gpi, string romPath) : this(gpi)
        {
            StorageKeySet.Add(romPath);
        }
    }
}