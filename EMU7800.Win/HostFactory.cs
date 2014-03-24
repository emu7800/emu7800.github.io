using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using EMU7800.Core;

namespace EMU7800.Win
{
    public class HostFactory
    {
        #region Fields

        static readonly IDictionary<string, Type> _registeredHostTypes = new Dictionary<string, Type>();
        readonly ILogger _logger;

        #endregion

        #region Constructors

        public HostFactory(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException("logger");

            _logger = logger;

        }

        static HostFactory()
        {
            RegisterType<DirectX.HostDirectX>("DirectX (DX9)");
            RegisterType<DirectX.HostDirectXFullscreen>("DirectX (DX9 Fullscreen)");
#if DEBUG
            RegisterType<Gdi.HostGdi>("Windows (GDI)");
#endif
        }

        static void RegisterType<T>(string name)
        {
            _registeredHostTypes.Add(name, typeof(T));
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Create an instance of the specified host.
        /// </summary>
        /// <param name="hostName"></param>
        /// <param name="m"></param>
        /// <exception cref="InvalidOperationException">Cannot instantiate specified host.</exception>
        public HostBase Create(string hostName, MachineBase m)
        {
            if (!_registeredHostTypes.ContainsKey(hostName))
            {
                var message = string.Format("Host name not registered: {0}", hostName);
                _logger.WriteLine(message);
                throw new InvalidOperationException(message);
            }

            var type = _registeredHostTypes[hostName];
            var host = Activator.CreateInstance(type, new object[] {m, _logger}) as HostBase;
            if (host == null)
            {
                var message = string.Format("Host type is not instantiatable: {0}", type.FullName);
                _logger.WriteLine(message);
                throw new InvalidOperationException(message);
            }
            return host;
        }

        public ICollection<string> GetRegisteredHostNames()
        {
            return new ReadOnlyCollection<string>(_registeredHostTypes.Keys.ToList());
        }

        #endregion
    }
}
