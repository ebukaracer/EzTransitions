using System.Collections.Generic;
using UnityEngine;

// ReSharper disable MemberCanBeProtected.Global

namespace Racer.EzTransitions.Utilities
{
    public abstract class SingletonPattern
    {
        private SingletonPattern()
        {
        }

        /// <summary>
        /// A static instance is similar to a singleton, but instead of destroying any new instances,
        /// it overrides the current instance. This is handy for resetting the state.
        /// </summary>
        public abstract class StaticInstance<T> : MonoBehaviour where T : MonoBehaviour
        {
            public static T Instance { get; private set; }

            protected virtual void Awake() => Instance = this as T;
        }

        /// <summary>
        /// This transforms the static instance into a basic singleton. This will destroy any new 
        /// version created leaving the original intact.
        /// </summary>
        public abstract class Singleton<T> : StaticInstance<T> where T : MonoBehaviour
        {
            protected override void Awake()
            {
                if (Instance != null)
                    Destroy(gameObject);
                else
                    base.Awake();
            }
        }

        /// <summary>
        /// This will persist/survive through scene loads. 
        /// </summary>
        public abstract class SingletonPersistent<T> : Singleton<T> where T : MonoBehaviour
        {
            protected override void Awake()
            {
                base.Awake();

                var go = gameObject;

                go.transform.parent = null;

                DontDestroyOnLoad(go);
            }
        }
    }

    internal static class Utility
    {
        #region GetWaitForSeconds

        private static readonly Dictionary<float, WaitForSeconds> WaitDelay = new();

        /// <summary>
        /// Container that stores/reuses newly created WaitForSeconds.
        /// </summary>
        /// <param name="time">time(s) to wait</param>
        /// <returns>new WaitForSeconds</returns>
        public static WaitForSeconds GetWaitForSeconds(float time)
        {
            if (WaitDelay.TryGetValue(time, out var waitForSeconds)) return waitForSeconds;

            WaitDelay[time] = new WaitForSeconds(time);

            return WaitDelay[time];
        }

        #endregion
    }
}