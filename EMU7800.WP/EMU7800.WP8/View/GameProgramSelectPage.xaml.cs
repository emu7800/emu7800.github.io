using EMU7800.WP.ViewModel;
using Microsoft.Phone.Controls;
using System;
using System.Windows;
using System.Windows.Controls;

namespace EMU7800.WP.View
{
    public partial class GameProgramSelectPage
    {
        readonly GameProgramSelectViewModel _viewModel;

        public GameProgramSelectPage()
        {
            InitializeComponent();

            var currentApp = (App)Application.Current;
            _viewModel = currentApp.Services.GetService <GameProgramSelectViewModel>();
            games2600.ItemsSource = _viewModel.Games2600;
            games7800.ItemsSource = _viewModel.Games7800;
            gamesAtari.ItemsSource = _viewModel.GamesAtari;
            gamesActivision.ItemsSource = _viewModel.GamesActivision;

            // EasterEgg logic for revealing the Activision titles
            if (currentApp.GgeRetsae == 7)
                currentApp.GgeRetsae++;
            else
                pivotControl.Items.Remove(pivotitemActivision);

            gamesImagic.ItemsSource = _viewModel.GamesImagic;
            gamesOther.ItemsSource = _viewModel.GamesOther;

            Loaded += GameProgramSelect_Loaded;
        }

        protected void OnNavigatedFrom(NavigatingEventArgs e)
        {
            StateUtils.RestoreState(State, pivotControl, 0);
        }

        protected void OnNavigatedTo(NavigatingEventArgs e)
        {
            StateUtils.PreserveState(State, pivotControl);
        }

        void GameProgramSelect_Loaded(object sender, RoutedEventArgs e)
        {
        }

        void Button_Tap(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            var vm = (GameProgramSelectItemViewModel)button.Tag;
            var uriString = string.Format("/View/GamePage.xaml?id={0}", vm.Id);
            var uri = new Uri(uriString, UriKind.Relative);
            try
            {
                NavigationService.Navigate(uri);
            }
            catch (InvalidOperationException)
            {
                // navigation already in progress.
            }
        }
    }
}