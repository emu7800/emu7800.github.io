using System;
using System.Collections.Generic;

namespace EMU7800.WP
{
    /// <summary>
    /// A ServiceProvider for the application.
    /// This type is exposed through the App.Services property and can be used for ContentManagers
    /// or other types that need access to a ServiceProvider.
    /// </summary>
    public class AppServiceProvider : IServiceProvider
    {
        #region Fields

        // A map of service type to the services themselves
        readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        #endregion

        /// <summary>
        /// Adds a new service to the service provider.
        /// </summary>
        /// <typeparam name="T">The type of the service object.</typeparam>
        /// <param name="service">The service object itself.</param>
        public void AddService<T>(T service) where T : class
        {
            if (service == null)
                throw new ArgumentNullException("service");
            _services.Add(typeof(T), service);
        }

        /// <summary>
        /// Gets a service from the service provider.
        /// </summary>
        /// <typeparam name="T">The type of the service object.</typeparam>
        public T GetService<T>() where T : class
        {
            return (T)_services[typeof(T)];
        }

        /// <summary>
        /// Removes a service from the service provider.
        /// </summary>
        /// <typeparam name="T">The type of the service object.</typeparam>
        public void RemoveService<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        #region IServiceProvider Members

        /// <summary>
        /// Gets a service from the service provider.
        /// </summary>
        /// <param name="serviceType">The type of the service object.</param>
        public object GetService(Type serviceType)
        {
            if (serviceType == null)
                throw new ArgumentNullException("serviceType");
            return _services[serviceType];
        }

        #endregion
    }
}
