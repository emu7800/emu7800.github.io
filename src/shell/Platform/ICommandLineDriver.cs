using EMU7800.Services.Dto;

namespace EMU7800.Shell;

public interface ICommandLineDriver
{
    void Start(bool startMaximized);
    void StartGameProgram(GameProgramInfoViewItem gpivi, bool startMaximized);
}
