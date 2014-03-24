// © Mike Murphy

using System.Collections.Generic;

namespace EMU7800.Services.Dto
{
    public class ImportedGameProgramInfo
    {
        public GameProgramInfo GameProgramInfo { get; set; }
        public IList<string> StorageKeySet { get; set; }
        public bool PersistedStateExists { get; set; }
    }
}