// © Mike Murphy

using EMU7800.Services.Dto;

namespace EMU7800.Services
{
    public class SettingsService
    {
        #region Fields

        static ApplicationSettings _applicationSettings;
        readonly DatastoreService _datastoreService = new DatastoreService();

        #endregion

        public ApplicationSettings GetSettings()
        {
            if (_applicationSettings == null)
            {
                _applicationSettings = _datastoreService.GetSettings()
                    ?? new ApplicationSettings { ShowTouchControls = false };
            }
            return ToDeepCopy(_applicationSettings);
        }

        public void SaveSettings(ApplicationSettings settings)
        {
            if (settings == null)
                return;
            if (_applicationSettings != null)
            {
                // don't bother saving if nothing has changed
                if (settings.ShowTouchControls == _applicationSettings.ShowTouchControls
                    && settings.TouchControlSeparation == _applicationSettings.TouchControlSeparation)
                    return;
            }
            _applicationSettings = ToDeepCopy(settings);
            _datastoreService.SaveSettings(settings);
        }

        #region Helpers

        static ApplicationSettings ToDeepCopy(ApplicationSettings settings)
        {
            var copyOfSettings = new ApplicationSettings
            {
                ShowTouchControls = settings.ShowTouchControls,
                TouchControlSeparation = settings.TouchControlSeparation
            };
            return copyOfSettings;
        }

        #endregion
    }
}