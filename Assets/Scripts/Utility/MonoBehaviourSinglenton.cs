using System;

using UnityEngine;

namespace Game.Utility
{
    public abstract class MonoBehaviourSinglenton<T> : MonoBehaviour where T : MonoBehaviourSinglenton<T>
    {
        private static T instance;

        public static T Instance {
            get {
                if (instance == null)
                {
                    instance = FindObjectOfType<T>(true);
                    if (instance == null)
                    {
#if UNITY_EDITOR
                        Debug.LogError($"Singlenton of type {typeof(T).Name} was not found. This may be normal if you haven't opened any scene.");
#else
                        Debug.LogError($"Singlenton of type {typeof(T).Name} was not found.");
#endif
                    }
                    else
                    {
#if UNITY_EDITOR
                        if (Application.isPlaying)
#endif
                            instance.Awake_();
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null)
            {
                if (instance == this)
                    return;

                Debug.LogError($"{typeof(T).Name} is a singlenton.");
                Destroy(this);
            }

            try
            {
                instance = (T)(object)this;
            }
            catch (InvalidCastException e)
            {
                Debug.LogException(new InvalidOperationException($"Instance is not of type {nameof(T)}", e));
                return;
            }

            Awake_();
        }

        protected virtual void Awake_() { }
    }
}