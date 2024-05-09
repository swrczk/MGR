using UnityEngine;

namespace Utils
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