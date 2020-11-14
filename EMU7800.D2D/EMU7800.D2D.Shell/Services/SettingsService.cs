// � Mike Murphy

using EMU7800.Services.Dto;

namespace EMU7800.Services
{
    public class SettingsService
    {
        #region Fields

        static ApplicationSettings _applicationSettings = new ApplicationSettings();
        static bool _applicationSettingsLoaded;

        readonly DatastoreService _datastoreService = new DatastoreService();

        #endregion

        public ApplicationSettings GetSettings()
        {
            if (!_applicationSettingsLoaded)
            {
                var (result, settings) = _datastoreService.GetSettings();
                if (result.IsOk)
                {
                    _applicationSettings = settings;
                    _applicationSettingsLoaded = true;
                }
            }
            return _applicationSettings.ToDeepCopy();
        }

        public void SaveSettings(ApplicationSettings settings)
        {
            // don't bother saving if nothing has changed
            if (settings.ShowTouchControls == _applicationSettings.ShowTouchControls
                && settings.TouchControlSeparation == _applicationSettings.TouchControlSeparation)
                return;
            _applicationSettings = settings.ToDeepCopy();
            _datastoreService.SaveSettings(settings);
        }
    }
}