using UI.Common;
using UnityEngine;
using Utils;

namespace UI
{
    public abstract class BaseContext : MonoBehaviour, IContext
    {
        public MvcIntegrationManager MvcIntegrationManager { get; private set; }


        public virtual void ResolveContext()
        {
            MvcIntegrationManager = Managers.Instance.GetManager<MvcIntegrationManager>();
        }

        public BaseController AddView(IView view, Transform root)
        {
            var type = view.GetType();
            return MvcIntegrationManager.CreateAndAssignViewController(type, root);
        }
    }

    public interface IContext
    {
        public void ResolveContext();
    }
}