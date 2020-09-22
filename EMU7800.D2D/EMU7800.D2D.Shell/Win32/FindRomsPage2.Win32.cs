// © Mike Murphy

using System.Threading.Tasks;

namespace EMU7800.D2D.Shell
{
    public partial class FindRomsPage2
    {
        async void StartImport()
        {
            _buttonCancel.IsVisible = true;

            var result = await Task.Run(() => _romImportService.Import());

            if (_romImportService.CancelRequested)
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
