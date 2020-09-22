// © Mike Murphy

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMU7800.D2D.Interop;
using EMU7800.Services;
using EMU7800.Services.Dto;

namespace EMU7800.D2D.Shell
{
    public sealed class GameProgramSelectionPage : PageBase
    {
        readonly ButtonBase _buttonBack;
        readonly LabelControl _labelSelectGameProgram;
        readonly GameProgramSelectionControl _gameProgramSelectionControl;

        GameControllersWrapperBase _gameControllers = new GameControllersWrapperBase();

        bool _isGetGameProgramInfoViewItemCollectionAsyncStarted;

        public GameProgramSelectionPage()
        {
            _buttonBack = new BackButton
            {
                Location = Struct.ToPointF(5, 5)
            };
            _labelSelectGameProgram = new LabelControl
            {
                Text = "Select Game Program",
                TextFontFamilyName = Styles.NormalFontFamily,
                TextFontSize = Styles.NormalFontSize,
                Location = Struct.ToRightOf(_buttonBack, 25, 12),
                Size = Struct.ToSizeF(400, 200),
                IsVisible = true
            };
            _gameProgramSelectionControl = new GameProgramSelectionControl
            {
                Location = Struct.ToBottomOf(_buttonBack, -5, 5)
            };
            Controls.Add(_buttonBack, _labelSelectGameProgram, _gameProgramSelectionControl);

            _buttonBack.Clicked += ButtonBack_Clicked;
            _gameProgramSelectionControl.Selected += GameProgramSelectionControl_Selected;
        }

        #region PageBase Overrides

        public override void OnNavigatingHere()
        {
            base.OnNavigatingHere();

            EnsureGameControllersAreDisposed();
            _gameControllers = new GameControllersWrapper(_gameProgramSelectionControl);

            if (_isGetGameProgramInfoViewItemCollectionAsyncStarted)
                return;
            _isGetGameProgramInfoViewItemCollectionAsyncStarted = true;

            GetGameProgramInfoViewItemCollectionsAsync();
        }

        public override void Resized(SizeF size)
        {
            _gameProgramSelectionControl.Size = Struct.ToSizeF(size.Width, size.Height - 100);
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

        void GameProgramSelectionControl_Selected(object sender, GameProgramSelectedEventArgs e)
        {
            var gameProgramInfoViewItem = e.GameProgramInfoViewItem;
            var gamePage = new GamePage(gameProgramInfoViewItem);
            PushPage(gamePage);
        }

        void ButtonBack_Clicked(object sender, EventArgs eventArgs)
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
        {
            var datastoreService = new DatastoreService();
            var gameProgramLibraryService = new GameProgramLibraryService();

            var refRepositoryCsvResult = datastoreService.GetGameProgramInfoFromReferenceRepository();
            var importRepositoryCsvResult = datastoreService.GetGameProgramInfoFromImportRepository();

            var gameProgramInfoDict = RomPropertiesService.ToGameProgramInfo(refRepositoryCsvResult.Values.Select(st => st.Line))
                .GroupBy(gpi => gpi.MD5).ToDictionary(g => g.Key, g => g.ToList());

            var isEasterEggOn = TitlePage.IsEasterEggOn;

            var importedGameProgramInfoSet = RomPropertiesService.ToImportedGameProgramInfo(gameProgramInfoDict, importRepositoryCsvResult.Values.Select(st => st.Line))
                .Where(igpi => isEasterEggOn || Filter(igpi.GameProgramInfo));

            var result = gameProgramLibraryService.GetGameProgramInfoViewItemCollections(importedGameProgramInfoSet);
            return result.ToArray();
        }

        static bool Filter(GameProgramInfo gpi)
            => gpi.Manufacturer     != "Activision"
                && gpi.Manufacturer != "Mystique"
                && gpi.Manufacturer != "Playaround"
                && gpi.Title        != "Pitfall!";

        static void CheckPersistedMachineStates(IEnumerable<GameProgramInfoViewItemCollection> gpivics)
        {
            var datastoreService = new DatastoreService();
            foreach (var gpvi in gpivics.SelectMany(gpvi => gpvi.GameProgramInfoViewItemSet))
            {
                gpvi.ImportedGameProgramInfo.PersistedStateExists = datastoreService.PersistedMachineExists(gpvi.ImportedGameProgramInfo.GameProgramInfo);
            }
        }

        void EnsureGameControllersAreDisposed()
        {
            _gameControllers.Dispose();
            _gameControllers = new GameControllersWrapperBase();
        }

        #endregion
    }
}
