// © Mike Murphy

using System.Collections.Generic;

namespace EMU7800.Services.Dto;

public sealed record ImportedRoms(
    List<ImportedGameProgramInfo> GamePrograms,
    List<ImportedSpecialBinaryInfo> SpecialBinaries,
    int FileExamined,
    int FilesRecognized);
