using UnityEngine;

namespace Utils.MVC
{
    public abstract class BaseView : MonoBehaviour, IView
    {
        private void Start()
        {
            RegisterEvents();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
        }

        protected virtual void RegisterEvents()
        {
        }

        protected virtual void UnregisterEvents()
        {
        }

        public virtual void Setup()
        {
        }
    }

    public abstract class BaseMainView : BaseView, IMainView
    {
    }

    public interface IView
    {
    }

    public interface IMainView
    {
    }
}