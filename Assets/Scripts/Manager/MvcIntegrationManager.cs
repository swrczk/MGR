using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utils.MVC;

namespace Manager
{
    public class MvcIntegrationManager : BaseManager
    {
        private readonly Dictionary<Type, Type> _viewToControllerMap = new();
        private BaseController _mainController;

        public override void Initialize()
        {
            base.Initialize();
            ClearChild();
            RegisterViewAndControllerTypes();
            FindAndAssignControllers();
        }

        public override void OnGameStarted()
        {
            base.OnGameStarted();
            _mainController.Setup();
        }

        public BaseController CreateAndAssignViewController(Type viewType, Transform root)
        {
            if (_viewToControllerMap.TryGetValue(viewType, out Type controllerType))
            {
                GameObject viewObject = new GameObject(viewType.Name);
                GameObject controllerObject = new GameObject(controllerType.Name);

                var view = (BaseView)viewObject.AddComponent(viewType);
                var controller = (BaseController)controllerObject.AddComponent(controllerType);

                viewObject.transform.parent = root;
                controller.AssignView(view);
                controllerObject.transform.parent = viewObject.transform;
                return controller;
            }
            else
            {
                Debug.LogError($"No controller registered for view type {viewType.Name}");
                return null;
            }
        }

        private void ClearChild()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        private void RegisterViewAndControllerTypes()
        {
            var controllerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(IController).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

            foreach (var controllerType in controllerTypes)
            {
                var tempController = Activator.CreateInstance(controllerType) as BaseController;
                _viewToControllerMap[tempController.GetViewType()] = controllerType;
            }
        }

        private void FindAndAssignControllers()
        {
            var views = FindObjectsImplementingInterface<IView>();
            foreach (var view in views)
            {
                var viewType = view.GetType();
                if (view is IMainView && _viewToControllerMap.TryGetValue(viewType, out Type controllerType))
                {
                    var controllerObject = new GameObject(controllerType.Name);
                    var controller = (BaseController)controllerObject.AddComponent(controllerType);
                    controller.AssignView(view);
                    controllerObject.transform.parent = this.transform;
                    _mainController = controller;
                }
            }
        }

        private static IEnumerable<T> FindObjectsImplementingInterface<T>() where T : class
        {
            var components = FindObjectsOfType<MonoBehaviour>();
            foreach (var component in components)
            {
                if (component is T)
                    yield return component as T;
            }
        }
    }
}