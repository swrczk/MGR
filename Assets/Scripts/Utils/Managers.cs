using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Utils
{
    public class Managers : Singleton<Managers>
    {
        private readonly Dictionary<Type, IManager> _managers = new Dictionary<Type, IManager>();

        protected override void Awake()
        {
            base.Awake();
            InitializeManagers();
            foreach (var manager in _managers)
            {
                manager.Value.Initialize();
            }
        }

        public T GetManager<T>() where T : class, IManager
        {
            _managers.TryGetValue(typeof(T), out IManager manager);
            return manager as T;
        }

        private void InitializeManagers()
        {
            var managerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(IManager).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

            var defaultManager = Resources.Load<GameObject>($"Managers/DefaultManager");
            foreach (var type in managerTypes)
            {
                var managerPrefab = Resources.Load<GameObject>($"Managers/{type.Name}");
                var managerComponent = managerPrefab != null
                    ? Instantiate(managerPrefab, transform).GetComponent(type)
                    : Instantiate(defaultManager, transform).AddComponent(type);
                managerComponent.name = type.Name;

                if (managerComponent is IManager manager)
                {
                    _managers.Add(type, manager);
                }
            }
        }
    }
}