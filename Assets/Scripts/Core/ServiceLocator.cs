using System;
using System.Collections.Generic;

namespace GachaGame.Core
{
    public sealed class ServiceLocator
    {
        private static readonly Lazy<ServiceLocator> instanceHolder = new(() => new ServiceLocator());
        public static ServiceLocator Instance => instanceHolder.Value;

        private readonly Dictionary<Type, IService> services = new();

        private ServiceLocator() { }

        public void Register(Type type, IService service)
        {
            if (services.ContainsKey(type))
                throw new InvalidOperationException($"Service of type {type.Name} is already registered.");
            services[type] = service;
        }

        public void Register<T>(T service) where T : class, IService => Register(typeof(T), service);

        public IService Get(Type type)
        {
            if (!services.TryGetValue(type, out var service))
                throw new InvalidOperationException($"Service of type {type.Name} is not registered.");
            return service;
        }

        public T Get<T>() where T : class, IService => (T)Get(typeof(T));

        public bool TryGet<T>(out T service) where T : class, IService
        {
            if (services.TryGetValue(typeof(T), out var found))
            {
                service = (T)found;
                return true;
            }
            service = null;
            return false;
        }

        public bool IsRegistered(Type type) => services.ContainsKey(type);
        public bool IsRegistered<T>() where T : class, IService => IsRegistered(typeof(T));

        public void Unregister(Type type) => services.Remove(type);
        public void Unregister<T>() where T : class, IService => Unregister(typeof(T));

        public void Clear() => services.Clear();
    }
}
