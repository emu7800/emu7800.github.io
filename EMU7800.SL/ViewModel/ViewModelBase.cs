using System.ComponentModel;
using System.Windows;

namespace EMU7800.SL.ViewModel
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public bool IsDesignTime
        {
            get
            {
                var currentApp = Application.Current;
                return currentApp == null || currentApp.GetType() == typeof (Application);
            }
        }

        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler == null)
                return;
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
