using UnityEngine;

namespace Manager
{
    public abstract class BaseManager : MonoBehaviour, IManager
    {
        public virtual void Initialize()
        {
        }

        public virtual void OnGameStarted()
        {
        }
    }
}