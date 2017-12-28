using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using EMU7800.Core;
using EMU7800.SL.ViewModel;

namespace EMU7800.SL.View
{
    public partial class MainPage
    {
        MainPageViewModel ViewModel
        {
            get { return (MainPageViewModel) Resources["viewModel"]; }
        }

        public MainPage()
        {
            InitializeComponent();

            KeyDown += (senderKeyDown, eKeyDown) => ViewModel.RaiseKeyboardInput(eKeyDown.Key, true);
            KeyUp += (senderKeyUp, eKeyUp) => ViewModel.RaiseKeyboardInput(eKeyUp.Key, false);

            buttonSelect.Click += (s, e) => ViewModel.RaiseMachineInput(MachineInput.Select, buttonSelect.IsPressed);
            buttonReset.Click += (s, e) => ViewModel.RaiseMachineInput(MachineInput.Reset, buttonReset.IsPressed);

            ViewModel.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "SelectedGameProgram")
                {
                    mediaplayerScreen.SetSource(ViewModel.MediaSourceForSelectedGameProgram);
                    ViewModel.MediaSourceForSelectedGameProgram.SetHostingMediaElement(mediaplayerScreen);
                }
            };

            ViewModel.SelectRandomGameProgram();
        }
    }

    public class DownUpButton : Button
    {
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            IsPressed = true;
            base.OnClick();
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            IsPressed = false;
            base.OnClick();
        }
    }

    public class NoKeySlider : Slider
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
        }
    }
}
