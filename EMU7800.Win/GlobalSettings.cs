/*
 * GlobalSettings
 *
 * Provides read/write access to settings persisted across program invocations.
 * 
 * Copyright © 2004-2008 Mike Murphy
 * 
 */
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reflection;
using System.Xml;
using EMU7800.Core;

namespace EMU7800.Win
{
    public class GlobalSettings
    {
        #region Fields

        const string ConfigRoot = "EMU7800.Configuration";

        static XmlDocument _configDoc;
        static bool? _cachedNopRegisterDumping;
        static int? _cachedFrameRateAdjust, _cachedJoyBTrigger, _cachedJoyBBooster, _cachedPaddleFactor;

        readonly ILogger _logger;

        #endregion

        #region Public Properties

        // cannot databind to static members
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public string BaseDirectory
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public string OutputDirectory
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public string RomDirectory
        {
            get
            {
                var romDir = GetVal("ROMDirectory", Path.Combine(BaseDirectory, "roms"));
                if (!Directory.Exists(romDir))
                    romDir = BaseDirectory;
                return romDir;
            }
            set
            {
                if (Directory.Exists(value))
                {
                    SetVal("ROMDirectory", value);
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public string HostSelect
        {
            get { return GetVal("HostSelect", string.Empty); }
            set { SetVal("HostSelect", value); }
        }

        public int FrameRateAdjust
        {
            get
            {
                if (!_cachedFrameRateAdjust.HasValue)
                {
                    _cachedFrameRateAdjust = GetVal("FrameRateAdjust", 0);
                }
                return _cachedFrameRateAdjust.Value;
            }
            set
            {
                SetVal("FrameRateAdjust", value.ToString());
                _cachedFrameRateAdjust = value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public bool Skip7800BIOS
        {
            get { return GetVal("Skip7800BIOS", false); }
            set { SetVal("Skip7800BIOS", value.ToString()); }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public bool Use7800HSC
        {
            get { return GetVal("Use7800HSC", false); }
            set { SetVal("Use7800HSC", value.ToString()); }
        }

        public bool NOPRegisterDumping
        {
            get
            {
                if (!_cachedNopRegisterDumping.HasValue)
                {
                    _cachedNopRegisterDumping = GetVal("NOPRegisterDumping", false);
                }
                return _cachedNopRegisterDumping.Value;
            }
            set
            {
                SetVal("NOPRegisterDumping", value.ToString());
                _cachedNopRegisterDumping = value;
            }
        }

        public int JoyBTrigger
        {
            get
            {
                if (!_cachedJoyBTrigger.HasValue)
                {
                    _cachedJoyBTrigger = GetVal("JoyBTrigger", 0);
                }
                return _cachedJoyBTrigger.Value;
            }
            set
            {
                SetVal("JoyBTrigger", value.ToString());
                _cachedJoyBTrigger = value;
            }
        }

        public int JoyBBooster
        {
            get
            {
                if (!_cachedJoyBBooster.HasValue)
                {
                    _cachedJoyBBooster = GetVal("JoyBBooster", 1);
                }
                return _cachedJoyBBooster.Value;
            }
            set
            {
                SetVal("JoyBBooster", value.ToString());
                _cachedJoyBBooster = value;
            }
        }

        public int PaddleFactor
        {
            get
            {
                if (!_cachedPaddleFactor.HasValue)
                {
                    _cachedPaddleFactor = GetVal("PaddleFactor", 50 /* seems to simulate the original paddles closest */);
                }
                return _cachedPaddleFactor.Value;
            }
            set
            {
                SetVal("PaddleFactor", value.ToString());
                _cachedPaddleFactor = value;
            }
        }

        public string KeyBindings
        {
            get { return GetVal("KeyBindings", string.Empty); }
            set { SetVal("KeyBindings", value); }
        }

        public string GetUserValue(string name)
        {
            return GetVal("UserValue" + name, string.Empty);
        }

        public void SetUserValue(string name, object value)
        {
            SetVal("UserValue" + name, value);
        }

        public void Save()
        {
            Persist();
        }

        #endregion

        #region Constructors

        public GlobalSettings(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");
            _logger = logger;
        }

        #endregion

        #region Property Helpers

        bool GetVal(string name, bool defaultVal)
        {
            LoadIfNecessary();
            bool value;
            return bool.TryParse(GetVal(name, defaultVal.ToString()), out value) ? value : defaultVal;
        }

        int GetVal(string name, int defaultVal)
        {
            LoadIfNecessary();
            int value;
            return int.TryParse(GetVal(name, defaultVal.ToString()), out value) ? value : defaultVal;
        }

        string GetVal(string name, string defaultVal)
        {
            LoadIfNecessary();
            if (_configDoc.DocumentElement != null)
            {
                var n = _configDoc.DocumentElement.SelectSingleNode(name);
                return n != null ? n.InnerText : defaultVal;
            }
            return null;
        }

        void SetVal(string name, object val)
        {
            LoadIfNecessary();
            if (_configDoc.DocumentElement != null)
            {
                var n = _configDoc.DocumentElement.SelectSingleNode(name);
                if (n == null)
                {
                    n = _configDoc.CreateElement(name);
                    if (_configDoc.DocumentElement != null) _configDoc.DocumentElement.AppendChild(n);
                }
                n.InnerText = val.ToString();
            }
            ReportChangedSetting(name, val);
        }

        #endregion

        #region Backing Store Helpers

        void LoadIfNecessary()
        {
            if (_configDoc != null)
                return;

            _configDoc = new XmlDocument();
            try
            {
                using (var isf = IsolatedStorageFile.GetUserStoreForAssembly())
                using (var fs = new IsolatedStorageFileStream("EMU7800.configuration", FileMode.Open, FileAccess.Read, isf))
                {
                    _configDoc.Load(fs);
                }
            }
            catch (Exception ex)
            {
                if (Util.IsCriticalException(ex))
                    throw;
                _configDoc.RemoveAll();
                if (!(ex is FileNotFoundException))
                {
                    _logger.WriteLine("GlobalSettings: unable to load configuration: {0}", ex.Message);
                }
            }

            if (_configDoc.DocumentElement == null || !_configDoc.DocumentElement.Name.Equals(ConfigRoot))
            {
                _configDoc.RemoveAll();
                _configDoc.AppendChild(_configDoc.CreateElement(ConfigRoot));
            }
        }

        void Persist()
        {
            try
            {
                using (var isf = IsolatedStorageFile.GetUserStoreForAssembly())
                using (var fs = new IsolatedStorageFileStream("EMU7800.configuration", FileMode.Create, FileAccess.Write, isf))
                {
                    _configDoc.Save(fs);
                }
            }
            catch (Exception ex)
            {
                if (Util.IsCriticalException(ex))
                    throw;
                _logger.WriteLine("GlobalSettings: unable to save global settings: {0}", ex.Message);
            }
        }

        void ReportChangedSetting(string name, object val)
        {
            _logger.WriteLine("GlobalSetting {0} changed to {1}", name, val);
        }

        #endregion
    }
}
