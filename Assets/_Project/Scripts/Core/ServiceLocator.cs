using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryGame.Core
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T service)
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] {type.Name} zaten kayýtlý, üzerine yazýlýyor.");
            }
            _services[type] = service;
        }

        public static T Get<T>()
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            Debug.LogError($"[ServiceLocator] {type.Name} bulunamadý. Register edildi mi?");
            return default;
        }

        public static bool Has<T>()
        {
            return _services.ContainsKey(typeof(T));
        }

        public static void Clear()
        {
            _services.Clear();
        }
    }
}