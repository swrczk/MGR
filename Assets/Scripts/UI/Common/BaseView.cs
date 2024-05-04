using UnityEngine;

namespace UI.Common
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

        protected virtual  void UnregisterEvents()
        {
        }
    }

    public interface IView
    {
    }
}