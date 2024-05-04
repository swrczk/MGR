using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Utils;

namespace UI.Common
{
    public class MvcIntegrationManager : BaseManager
    {
        void Start()
        {
            FindAndAssignControllers();
        }
        
        private void FindAndAssignControllers()
        {
            var view = FindObjectsImplementingInterface<IView>().First();
            var viewType = view.GetType();
            var list = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => typeof(IController).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);

            foreach (var controllerType in list)
            {
                var controllerObject = new GameObject(controllerType.Name);
                var controller = (BaseController)controllerObject.AddComponent(controllerType);
                if (viewType == controller.GetViewType())
                {
                    controllerObject.transform.parent = this.transform;
                    controller.AssignView(view);
                }else
                {
                    Destroy(controllerObject);
                }
            }
        } // Metoda generyczna do wyszukiwania wszystkich komponentów implementujących określony interfejs

        public static IEnumerable<T> FindObjectsImplementingInterface<T>() where T : class
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