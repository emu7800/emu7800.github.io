using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Windows;

using EMU7800.Core;
using EMU7800.WP.Model;

namespace EMU7800.WP.ViewModel
{
    public class GameProgramSelectItemViewModel : INotifyPropertyChanged
    {
        #region Fields

        readonly GameProgramInfo _gameProgramInfo;
        bool _isPaused;

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public GameProgramId Id { get { return _gameProgramInfo.Id; } }

        public string Title { get { return _gameProgramInfo.Title; } }

        public string TitleDetails { get; private set; }

        public MachineType MachineType { get { return _gameProgramInfo.MachineType; } }

        public string MachineTypeName
        {
            get
            {
                switch (MachineType)
                {
                    case MachineType.A2600NTSC:
                    case MachineType.A2600PAL:
                        return "2600";
                    case MachineType.A7800NTSC:
                    case MachineType.A7800PAL:
                        return "7800";
                    //case MachineType.None:
                    default:
                        return "None";
                }
            }
        }

        public string Manufacturer { get { return _gameProgramInfo.Manufacturer; } }

        public bool IsPaused
        {
            get { return _isPaused; }
            set
            {
                if (_isPaused == value)
                    return;
                _isPaused = value;
                RaisePropertyChanged("IsPaused");
                RaisePropertyChanged("PauseButtonVisibility");
                RaisePropertyChanged("PlayButtonVisibility");
            }
        }

        public Visibility PauseButtonVisibility { get { return IsPaused ? Visibility.Visible : Visibility.Collapsed; } }

        public Visibility PlayButtonVisibility { get { return IsPaused ? Visibility.Collapsed : Visibility.Visible; } }

        #region Constructors

        public GameProgramSelectItemViewModel(GameProgramInfo gameProgramInfo, bool isPaused)
        {
            if (gameProgramInfo == null)
                throw new ArgumentNullException("gameProgramInfo");

            _gameProgramInfo = gameProgramInfo;

            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(gameProgramInfo.Manufacturer))
                sb.Append(gameProgramInfo.Manufacturer);
            if (sb.Length > 0)
                sb.Append(", ");
            sb.Append(MachineTypeName);
            if (!string.IsNullOrWhiteSpace(gameProgramInfo.Year))
            {
                if (sb.Length > 0)
                    sb.Append(", ");
                sb.Append(gameProgramInfo.Year);
            }
            TitleDetails = sb.ToString();

            IsPaused = isPaused;
        }

        #endregion

        #region Helpers

        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        void RaisePropertyChanged(string propertyName)
        {
            VerifyPropertyName(propertyName);

            var handler = PropertyChanged;
            if (handler == null)
            return;

            handler(this, new PropertyChangedEventArgs(propertyName));
        }

        [Conditional("DEBUG")]
        void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real, public, instance property on this object.
            var thisType = GetType();
            if (thisType.GetProperty(propertyName) == null)
                throw new ArgumentException("Invalid property name", propertyName);
        }

        #endregion
    }
}
