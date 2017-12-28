using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using EMU7800.Core;
using EMU7800.SL.Model;
using EMU7800.SL.View;

namespace EMU7800.SL.ViewModel
{
    public class MainPageViewModel : ViewModelBase
    {
        #region Fields

        bool _isColorSet = true, _isLeftDifficultyClear = true, _isRightDifficultyClear = true;
        int _currentPlayerNo;

        #endregion

        #region Constructors

        public MainPageViewModel()
        {
            KeyboardBindings = new Dictionary<Key, MachineInput>
            {
                { Key.Z, MachineInput.Fire },
                { Key.Right, MachineInput.Right },
                { Key.Left, MachineInput.Left },
                { Key.Down, MachineInput.Down },
                { Key.Up, MachineInput.Up },
                { Key.R, MachineInput.Reset },
                { Key.S, MachineInput.Select },
                { Key.C, MachineInput.Color },
                { Key.D1, MachineInput.SetKeyboardToPlayer1 },
                { Key.D2, MachineInput.SetKeyboardToPlayer2 },
                { Key.D3, MachineInput.SetKeyboardToPlayer3 },
                { Key.D4, MachineInput.SetKeyboardToPlayer4 },
                { Key.F, MachineInput.ShowFrameStats },
            };

            ControllerTypeCollection = ControllerInfo.ControllerTypeCollection;

            SelectedGameProgram = GameProgramInfo.DefaultGameProgram;

            var gameProgramRepository = new GameProgramRepository();
            GameProgramCollection = gameProgramRepository.GetAllGamePrograms().ToList();
        }

        #endregion

        public int CurrentPlayerNo
        {
            get { return _currentPlayerNo; }
            set
            {
                if (value < 0) value *= -1;
                if (value > 3) value %= 3;
                if (_currentPlayerNo == value)
                    return;
                _currentPlayerNo = value;
                RaisePropertyChanged("CurrentPlayerNo");
            }
        }

        public GameProgramId SelectedGameProgramId
        {
            get { return SelectedGameProgram.Id; }
            set
            {
                if (SelectedGameProgram.Id == value)
                    return;
                var gp = GameProgramCollection.FirstOrDefault(p => p.Id == value);
                if (gp == null)
                    return;
                SelectedGameProgram = gp;
                MediaSourceForSelectedGameProgram = new Emu7800MediaStreamSource(new MachineToStreamAdapter(gp));
                if (!IsColorSet)
                {
                    _isColorSet = true;
                    IsColorSet = false;
                }
                if (!IsLeftDifficultyClear)
                {
                    _isLeftDifficultyClear = true;
                    IsLeftDifficultyClear = false;
                }
                if (!IsRightDifficultyClear)
                {
                    _isRightDifficultyClear = true;
                    IsRightDifficultyClear = false;
                }
                RaisePropertyChanged("SelectedGameProgramId");
                RaisePropertyChanged("SelectedGameProgram");
            }
        }

        public void SelectRandomGameProgram()
        {
            var r = new Random();
            var id = r.Next(0, GameProgramCollection.Count);
            SelectedGameProgramId = GameProgramCollection.ElementAt(id).Id;
        }

        public bool IsColorSet
        {
            get { return _isColorSet; }
            set
            {
                if (_isColorSet == value)
                    return;
                _isColorSet = value;
                RaiseMachineInput(MachineInput.Color, true);
                RaisePropertyChanged("IsColorSet");
                RaisePropertyChanged("IsBwSet");
            }
        }

        public bool IsBwSet
        {
            get { return !_isColorSet; }
            set
            {
                if (_isColorSet == !value)
                    return;
                _isColorSet = !value;
                RaiseMachineInput(MachineInput.Color, true);
                RaisePropertyChanged("IsColorSet");
                RaisePropertyChanged("IsBwSet");
            }
        }

        public bool IsLeftDifficultySet
        {
            get { return !IsLeftDifficultyClear; }
            set { IsLeftDifficultyClear = !value; }
        }

        public bool IsLeftDifficultyClear
        {
            get { return _isLeftDifficultyClear; }
            set
            {
                if (_isLeftDifficultyClear == value)
                    return;
                _isLeftDifficultyClear = value;
                RaiseMachineInput(MachineInput.LeftDifficulty, true);
                RaisePropertyChanged("IsLeftDifficultyClear");
            }
        }

        public bool IsRightDifficultySet
        {
            get { return !IsRightDifficultyClear; }
            set { IsRightDifficultyClear = !value; }
        }

        public bool IsRightDifficultyClear
        {
            get { return _isRightDifficultyClear; }
            set
            {
                if (_isRightDifficultyClear == value)
                    return;
                _isRightDifficultyClear = value;
                RaiseMachineInput(MachineInput.RightDifficulty, true);
                RaisePropertyChanged("IsRightDifficultyClear");
            }
        }

        public double BlendFactor
        {
            get { return (MediaSourceForSelectedGameProgram != null) ? MediaSourceForSelectedGameProgram.BlendFactor : 0.5; }
            set
            {
                if (MediaSourceForSelectedGameProgram == null)
                    return;
                MediaSourceForSelectedGameProgram.BlendFactor = value;
            }
        }

        public Emu7800MediaStreamSource MediaSourceForSelectedGameProgram { get; private set; }

        public IDictionary<Key, MachineInput> KeyboardBindings { get; private set; }

        public GameProgramInfo SelectedGameProgram { get; private set; }

        public ICollection<ControllerInfo> ControllerTypeCollection { get; private set; }

        public ICollection<GameProgramInfo> GameProgramCollection { get; private set; }

        public void RaiseKeyboardInput(Key key, bool down)
        {
            if (MediaSourceForSelectedGameProgram == null)
                return;
            if (!KeyboardBindings.ContainsKey(key))
                return;

            var machineInput = KeyboardBindings[key];
            switch (machineInput)
            {
                case MachineInput.SetKeyboardToPlayer1:
                    CurrentPlayerNo = 0;
                    PostMessage("Keyboard to Player 1");
                    return;
                case MachineInput.SetKeyboardToPlayer2:
                    CurrentPlayerNo = 1;
                    PostMessage("Keyboard to Player 2");
                    return;
                case MachineInput.SetKeyboardToPlayer3:
                    CurrentPlayerNo = 2;
                    PostMessage("Keyboard to Player 3");
                    return;
                case MachineInput.SetKeyboardToPlayer4:
                    CurrentPlayerNo = 3;
                    PostMessage("Keyboard to Player 4");
                    return;
                case MachineInput.Color:
                    if (!down) return;
                    IsColorSet = !IsColorSet;
                    PostMessage("TV Type: {0}", IsColorSet ? "Color" : "B/W");
                    return;
                case MachineInput.ShowFrameStats:
                    if (!down) return;
                    var fps = MediaSourceForSelectedGameProgram.RenderedFramesPerSecond;
                    var fdps = MediaSourceForSelectedGameProgram.DroppedFramesPerSecond;
                    PostMessage("FPS:{0:0.0} FDPS:{1:0.0} ({2:0.0})", fps, fdps, fps + fdps);
                    break;
                default:
                    RaiseMachineInput(machineInput, down);
                    break;
            }
        }

        public void RaiseMachineInput(MachineInput machineInput, bool down)
        {
            if (MediaSourceForSelectedGameProgram == null)
                return;
            MediaSourceForSelectedGameProgram.RaiseInput(CurrentPlayerNo, machineInput, down);
        }

        void PostMessage(string format, params object[] args)
        {
            if (MediaSourceForSelectedGameProgram == null)
                return;
            MediaSourceForSelectedGameProgram.PostedMessage = string.Format(format, args);
        }
    }
}
