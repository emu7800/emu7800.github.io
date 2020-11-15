// © Mike Murphy

using EMU7800.Services;
using System.Threading.Tasks;

namespace EMU7800.D2D.Shell
{
    public partial class FindRomsPage2
    {
        async void StartImport()
        {
            _buttonCancel.IsVisible = true;

            var result = await Task.Run(() => RomImportService.Import());

            if (RomImportService.CancelRequested)
            {
                _labelStep.Text = result.IsFail ? "Canceled via internal error." : "Canceled.";
            }
            else
            {
                _labelStep.Text = "Completed.";
            }

            _buttonOk.IsVisible = true;
            _buttonCancel.IsVisible = false;
        }
    }
}
