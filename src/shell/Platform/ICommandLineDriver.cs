using EMU7800.Services.Dto;

namespace EMU7800.Shell;

public interface ICommandLineDriver
{
    void AttachConsole(bool allocNewConsole = false);
    void Start(bool startMaximized);
    void StartGameProgram(GameProgramInfoViewItem gpivi, bool startMaximized);
}
