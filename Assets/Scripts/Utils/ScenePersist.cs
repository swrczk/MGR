using UnityEngine;

namespace Utils
{
    public class ScenePersist: MonoBehaviour 
    {
        void Awake()
        {
            var numScenePersists = FindObjectsOfType<ScenePersist>().Length;
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