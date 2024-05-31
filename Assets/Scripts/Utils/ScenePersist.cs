using UnityEngine;

namespace Utils.Common
{
    public class ScenePersist: MonoBehaviour 
    {
        void Awake()
        {
            int numScenePersists = FindObjectsOfType<ScenePersist>().Length;
            if (numScenePersists > 1)
            {
                ResetScenePersist();
            }
            else
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public void ResetScenePersist()
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }
}