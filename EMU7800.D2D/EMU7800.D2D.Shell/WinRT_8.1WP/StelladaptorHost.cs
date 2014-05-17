// © Mike Murphy

using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Core;

namespace EMU7800.D2D.Shell
{
    public enum HidControllerType : byte
    {
        None,
        Stelladaptor,
        A2600Daptor,
        A2600Daptor2
    }

    public struct HidControllerData
    {
        public HidControllerType Type { get; set; }
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public ushort Buttons { get; set; }

        #region Public Helpers

        public bool this[int buttonno]
        {
            get { return GetButton(buttonno); }
            set { SetButton(buttonno, value); }
        }

        public bool GetButton(int buttonno)
        {
            return (Buttons & ((ushort) (1 << buttonno))) != 0;
        }

        public void SetButton(int buttonno, bool value)
        {
            if (value)
                Buttons |= (ushort)(1 << buttonno);
            else
                Buttons &= (ushort)(~(1 << buttonno));
        }

        #endregion
    }

    public static class StelladaptorHost
    {
        #region Fields

        public const int MAX_CONTROLLER_COUNT = 2;

        const int
             GenericDesktopControlsUsagePage = 1,
             ButtonUsagePage                 = 9,
             JoystickUsageId                 = 4,
             DirectionXUsageId               = 48,
             DirectionYUsageId               = 49;

        static readonly IDictionary<string, int> _hidDeviceIds = new Dictionary<string, int>();
        static readonly HidDevice[] _hidDevices = new HidDevice[MAX_CONTROLLER_COUNT];
        static readonly HidControllerData[] _hidControllersData = new HidControllerData[MAX_CONTROLLER_COUNT];

        static DeviceWatcher _deviceWatcher;
        static CoreDispatcher _dispatcher;

        // HID input does not prevent the system from sleeping like keyboard/mouse and xbox controller input apparently does.
        // This mechanism is used to request the system should not sleep while we have USB HID devices captured.
        static readonly DisplayRequest _displayRequest = new DisplayRequest();

        #endregion

        public static HidControllerData GetHidControllerData(int controllerNo)
        {
            return _hidControllersData[controllerNo % MAX_CONTROLLER_COUNT];
        }

        public static void Start()
        {
            if (_deviceWatcher != null)
                return;

            // Ensure the UI dispatcher context is obtained before proceeding, necessary for user prompts
            var coreWindow = CoreWindow.GetForCurrentThread();
            if (coreWindow == null || coreWindow.Dispatcher == null)
                return;

            _dispatcher = coreWindow.Dispatcher;

            CoreApplication.Suspending += CoreApplicationOnSuspending;
            CoreApplication.Resuming   += CoreApplicationOnResuming;

            var deviceSelector = HidDevice.GetDeviceSelector(GenericDesktopControlsUsagePage, JoystickUsageId);
            _deviceWatcher = DeviceInformation.CreateWatcher(deviceSelector);
            _deviceWatcher.Added += DeviceOnAdded;
            _deviceWatcher.Removed += DeviceOnRemoved;
            _deviceWatcher.Start();
        }

        #region Event Handlers

        static void OnInputReportReceived(HidDevice sender, HidInputReportReceivedEventArgs e)
        {
            if (e == null || e.Report == null)
                return;

            var controllerNo = -1;
            for (var i = 0; i < MAX_CONTROLLER_COUNT; i++)
            {
                var hidDevice = _hidDevices[i];
                if (hidDevice != sender)
                    continue;
                controllerNo = i;
                break;
            }

            if (controllerNo < 0)
                return;

            var buttonCount = 0;
            var hcd = _hidControllersData[controllerNo];

            switch (hcd.Type)
            {
                case HidControllerType.Stelladaptor:
                case HidControllerType.A2600Daptor:
                    buttonCount = 2;
                    break;
                case HidControllerType.A2600Daptor2:
                    buttonCount = 6;
                    break;
            }

            if (buttonCount <= 0)
                return;

            try
            {
                var ncX = e.Report.GetNumericControl(GenericDesktopControlsUsagePage, DirectionXUsageId);
                hcd.X = (ushort)ncX.Value;

                var ncY = e.Report.GetNumericControl(GenericDesktopControlsUsagePage, DirectionYUsageId);
                hcd.Y = (ushort)ncY.Value;

                for (var i = 0; i < buttonCount; i++)
                {
                    var buttonUsageId = (ushort) (i + 1);
                    var bc = e.Report.GetBooleanControl(ButtonUsagePage, buttonUsageId);
                    hcd[i] = bc.IsActive;
                }
            }
            catch (Exception)
            {
                hcd.Type = HidControllerType.None;
            }

            _hidControllersData[controllerNo] = hcd;
        }

        static void CoreApplicationOnSuspending(object sender, SuspendingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.StelladaptorHost.CoreApplicationOnSuspending");

            if (_deviceWatcher == null)
                return;

            _deviceWatcher.Stop();

            // devices are closed on suspend regardless, but clean-up the associated data structures
            for (var i = 0; i < _hidDevices.Length; i++)
            {
                CloseHidDevice(i);
            }

            _hidDeviceIds.Clear();
        }

        static void CoreApplicationOnResuming(object sender, object o)
        {
            System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.StelladaptorHost.CoreApplicationOnResuming");

            if (_deviceWatcher == null)
                return;

            _deviceWatcher.Start();
        }

        static void DeviceOnRemoved(DeviceWatcher sender, DeviceInformationUpdate e)
        {
            if (e == null || _dispatcher == null)
                return;

            // Eliminates warning CS4014, there is nothing to resume on the current synchronization context
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => DeviceOnRemoved2(e))
                .AsTask()
                    .ConfigureAwait(false);
        }

        static void DeviceOnRemoved2(DeviceInformationUpdate e)
        {
            System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.StelladaptorHost.DeviceOnRemoved2: {0}", e.Id);

            int controllerNo;
            if (!_hidDeviceIds.TryGetValue(e.Id, out controllerNo))
                return;

            CloseHidDevice(controllerNo);
            _hidDeviceIds.Remove(e.Id);

            // allow sleep if this is the last HID device captured
            _displayRequest.RequestRelease();
        }

        static void DeviceOnAdded(DeviceWatcher sender, DeviceInformation e)
        {
            if (e == null || _dispatcher == null)
                return;

            // Eliminates warning CS4014, there is nothing to resume on the current synchronization context
            _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => DeviceOnAdded2(e))
                .AsTask()
                    .ConfigureAwait(false);
        }

        static async void DeviceOnAdded2(DeviceInformation e)
        {
            System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.StelladaptorHost.DeviceOnAdded2: {0}", e.Id);

            int controllerNo;
            if (_hidDeviceIds.TryGetValue(e.Id, out controllerNo))
                return;

            for (; controllerNo < MAX_CONTROLLER_COUNT; controllerNo++)
            {
                if (_hidControllersData[controllerNo].Type == HidControllerType.None)
                    break;
            }
            if (controllerNo >= MAX_CONTROLLER_COUNT)
                return;

            var hcd = new HidControllerData();

            switch (e.Name)
            {
                case "Stelladaptor 2600-to-USB Interface":
                    hcd.Type = HidControllerType.Stelladaptor;
                    hcd.X = 0x7f;
                    hcd.Y = 0x7f;
                    break;
                case "2600-daptor":
                    hcd.Type = HidControllerType.A2600Daptor;
                    hcd.X = 0x7ff;
                    hcd.Y = 0x7ff;
                    break;
                case "2600-daptor II":
                    hcd.Type = HidControllerType.A2600Daptor2;
                    hcd.X = 0x7ff;
                    hcd.Y = 0x7ff;
                    break;
                default:
                    return;
            }

            // Ensure anything left around has been disposed, nominally should not be necessary
            CloseHidDevice(controllerNo);

            try
            {
                _hidDevices[controllerNo] = await HidDevice.FromIdAsync(e.Id, FileAccessMode.Read);
            }
            catch (Exception)
            {
                // InvalidOperationException can occur if this is not running on the UI thread and the user needs a consent prompt
                System.Diagnostics.Debug.WriteLine("EMU7800.D2D.Shell.StelladaptorHost.DeviceOnAdded2: Unexpected Exception on HidDevice.FromIdAsync: {0}", e.Id);
                return;
            }

            if (_hidDevices[controllerNo] == null)
                return;  // user declined access

            _hidDeviceIds.Add(e.Id, controllerNo);
            _hidControllersData[controllerNo] = hcd;

            // all set, start receiving input
            _hidDevices[controllerNo].InputReportReceived += OnInputReportReceived;

            // dont sleep while we have a HID device captured
            _displayRequest.RequestActive();
        }

        #endregion

        #region Helpers

        static void CloseHidDevice(int controllerNo)
        {
            if (_hidDevices[controllerNo] == null)
                return;

            _hidDevices[controllerNo].InputReportReceived -= OnInputReportReceived;
            _hidDevices[controllerNo].Dispose();
            _hidDevices[controllerNo] = null;

            // hoping OnInputReportReceived events have quiesced by now...
            _hidControllersData[controllerNo] = new HidControllerData();
        }

        #endregion
    }
}
