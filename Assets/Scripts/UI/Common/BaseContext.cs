using UnityEngine;

namespace UI.Common
{
    public abstract class BaseContext : MonoBehaviour, IContext
    {
        public abstract void ResolveContext();
    }

    public interface IContext
    {
        public void ResolveContext();
    }
}