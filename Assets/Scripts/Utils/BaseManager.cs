using UnityEngine;

namespace Utils
{
    public abstract class BaseManager : MonoBehaviour, IManager
    {
        public virtual void Initialize()
        {
        }
    }
}