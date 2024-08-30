using UnityEngine;

namespace RayTone
{
    public abstract class Singleton<T> : MonoBehaviour where T : Component
    {
        // the instance
        private static T instance;

        /////
        //AWAKE
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Get the instance
        /// </summary>
        public static T Instance
        {
            get
            {
                return instance;
            }
        }
    }
}