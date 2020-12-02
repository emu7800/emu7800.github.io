// © Mike Murphy

using EMU7800.Services;
using EMU7800.Services.Dto;
using EMU7800.Win32.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EMU7800.D2D.Shell
{
    public sealed class GameProgramSelectionPage : PageBase
    {
        readonly BackButton _buttonBack;
        readonly LabelControl _labelSelectGameProgram;
        readonly GameProgramSelectionControl _gameProgramSelectionControl;

        readonly JoystickDevice _backStartController = new(0);
        GameControllers _gameControllers = GameControllers.Default;

        bool _isGetGameProgramInfoViewItemCollectionAsyncStarted;

        public GameProgramSelectionPage()
        {
            _buttonBack = new()
            {
                Location = new(5, 5)
            };
            _labelSelectGameProgram = new()
            {
                Text = "Select Game Program",
                TextFontFamilyName = Styles.NormalFontFamily,
                TextFontSize = Styles.NormalFontSize,
                Location = _buttonBack.ToRightOf(25, 12),
                Size = new(400, 200),
                IsVisible = true
            };
            _gameProgramSelectionControl = new()
            {
                Location = _buttonBack.ToBottomOf(-5, 5)
            };
            Controls.Add(_buttonBack, _labelSelectGameProgram, _gameProgramSelectionControl);

            _buttonBack.Clicked += ButtonBack_Clicked;
            _gameProgramSelectionControl.Selected += GameProgramSelectionControl_Selected;

            _backStartController.JoystickDirectionalButtonChanged += (b, down) => {
                if (b == JoystickDirectionalButtonEnum.Back && down)
                    ButtonBack_Clicked(this, new());
                else if (b == JoystickDirectionalButtonEnum.Start)
                    _gameProgramSelectionControl.KeyboardKeyPressed(KeyboardKey.Enter, down);
            };
        }

        #region PageBase Overrides

        public override void OnNavigatingHere()
        {
            base.OnNavigatingHere();

            EnsureGameControllersAreDisposed();

            if (_isGetGameProgramInfoViewItemCollectionAsyncStarted)
                return;
            _isGetGameProgramInfoViewItemCollectionAsyncStarted = true;

            GetGameProgramInfoViewItemCollectionsAsync();
        }

        public override void Resized(D2D_SIZE_F size)
        {
            _gameProgramSelectionControl.Size = new(size.Width, size.Height - 100);
        }

        public override void Update(TimerDevice td)
        {
            base.Update(td);
            _gameControllers.Poll();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                EnsureGameControllersAreDisposed();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Event Handlers

        void GameProgramSelectionControl_Selected(object? sender, GameProgramSelectedEventArgs e)
        {
            var gameProgramInfoViewItem = e.GameProgramInfoViewItem;
            var gamePage = new GamePage(gameProgramInfoViewItem);
            PushPage(gamePage);
        }

        void ButtonBack_Clicked(object? sender, EventArgs eventArgs)
        {
            PopPage();
        }

        #endregion

        #region Helpers

        async void GetGameProgramInfoViewItemCollectionsAsync()
        {
            var gpivics = await Task.Run(() => GetGameProgramInfoViewItemCollection());
            _gameProgramSelectionControl.BindTo(gpivics);
            await Task.Run(() => CheckPersistedMachineStates(gpivics));
        }

        static GameProgramInfoViewItemCollection[] GetGameProgramInfoViewItemCollection()
            => GameProgramLibraryService.GetGameProgramInfoViewItemCollections(DatastoreService.ImportedGameProgramInfo).ToArray();

        static void CheckPersistedMachineStates(IEnumerable<GameProgramInfoViewItemCollection> gpivics)
        {
            foreach (var gpvi in gpivics.SelectMany(gpvi => gpvi.GameProgramInfoViewItemSet))
            {
                var pse = DatastoreService.PersistedMachineExists(gpvi.ImportedGameProgramInfo.GameProgramInfo);
                gpvi.ImportedGameProgramInfo.PersistedStateExists = pse;
            }
        }

        void EnsureGameControllersAreDisposed()
        {
            _gameControllers.Dispose();
            _gameControllers = GameControllers.Default;
            _backStartController.ClearEventHandlers();
        }

        #endregion
    }
}
