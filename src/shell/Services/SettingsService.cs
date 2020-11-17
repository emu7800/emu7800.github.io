// � Mike Murphy

using EMU7800.Services.Dto;

namespace EMU7800.Services
{
    public class SettingsService
    {
        #region Fields

        static ApplicationSettings _applicationSettings = new();
        static bool _applicationSettingsLoaded;

        #endregion

        public static ApplicationSettings GetSettings()
        {
            if (!_applicationSettingsLoaded)
            {
                var (result, settings) = DatastoreService.GetSettings();
                if (result.IsOk)
                {
                    _applicationSettings = settings;
                    _applicationSettingsLoaded = true;
                }
            }
            return _applicationSettings.ToDeepCopy();
        }

        public static void SaveSettings(ApplicationSettings settings)
        {
            // don't bother saving if nothing has changed
            if (settings.ShowTouchControls == _applicationSettings.ShowTouchControls
                && settings.TouchControlSeparation == _applicationSettings.TouchControlSeparation)
                return;
            _applicationSettings = settings.ToDeepCopy();
            DatastoreService.SaveSettings(settings);
        }
    }
}